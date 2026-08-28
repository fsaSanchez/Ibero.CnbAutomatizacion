using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Ibero.CnbAutomatizacion.Business.Service.Publicacion.Impl;

public class FacebookGraphClient(HttpClient httpClient) : IFacebookGraphClient
{
    public async Task<string?> PublicarFotoAsync(
      string pageId,
      string accessToken,
      byte[] fotoBytes,
      string caption)
    {
        using var content = new MultipartFormDataContent();
        var debugUrl = $"https://graph.facebook.com/debug_token?input_token={accessToken}&access_token={accessToken}";
        var debug = await httpClient.GetStringAsync(debugUrl);

        // ✅ Campo correcto es "message", no "caption"
        content.Add(new StringContent(accessToken), "access_token");
        content.Add(new StringContent(caption), "message");

        // ✅ ByteArrayContent necesita ContentType explícito
        var imageContent = new ByteArrayContent(fotoBytes);
        imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        content.Add(imageContent, "source", "foto.jpg");

        // ✅ BaseAddress ya tiene https://graph.facebook.com/
        //    La ruta relativa NO debe empezar con "/"
        var response = await httpClient.PostAsync($"v19.0/{pageId}/photos", content);

        return await ExtraerIdAsync(response);
    }

    public async Task<string?> PublicarTextoAsync(string pageId, string accessToken, string message)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["access_token"] = accessToken,
            ["message"] = message
        });

        var response = await httpClient.PostAsync($"v19.0/{pageId}/feed", content);
        return await ExtraerIdAsync(response);
    }

    public async Task<bool> EliminarPublicacionAsync(string idPublicacionExterna, string accessToken)
    {
        var response = await httpClient.DeleteAsync(
            $"{idPublicacionExterna}?access_token={Uri.EscapeDataString(accessToken)}");

        if (response.IsSuccessStatusCode)
            return true;

        // Si el post ya no existe en Facebook (borrado manual, expiró, etc.), el objetivo
        // de la eliminación ya se cumplió, así que se considera éxito.
        var json = await response.Content.ReadAsStringAsync();
        return json.Contains("does not exist", StringComparison.OrdinalIgnoreCase)
            || json.Contains("\"code\":100", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<string?> ExtraerIdAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.TryGetProperty("id", out var id) ? id.GetString() : null;
    }
}
