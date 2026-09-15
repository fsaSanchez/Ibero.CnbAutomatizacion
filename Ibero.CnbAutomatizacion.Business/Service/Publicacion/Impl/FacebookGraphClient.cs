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
        // Paso 1: subir la foto sin publicarla (published=false) para obtener su id.
        // Paso 2: crear el post real en /feed con attached_media referenciando esa foto.
        // Publicar directo a /photos no garantiza una historia visible en el feed público;
        // este es el patrón que Meta documenta como confiable.
        var photoId = await SubirFotoNoPublicadaAsync(pageId, accessToken, fotoBytes);
        if (photoId is null)
            return null;

        return await PublicarEnFeedConFotoAsync(pageId, accessToken, caption, photoId);
    }

    private async Task<string?> SubirFotoNoPublicadaAsync(string pageId, string accessToken, byte[] fotoBytes)
    {
        using var content = new MultipartFormDataContent();

        content.Add(new StringContent(accessToken), "access_token");
        content.Add(new StringContent("false"), "published");

        // ✅ ByteArrayContent necesita ContentType explícito
        var imageContent = new ByteArrayContent(fotoBytes);
        imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        content.Add(imageContent, "source", "foto.jpg");

        // ✅ BaseAddress ya incluye https://graph.facebook.com/{version}/
        //    La ruta relativa NO debe empezar con "/"
        var response = await httpClient.PostAsync($"{pageId}/photos", content);

        return await ExtraerIdAsync(response);
    }

 private async Task<string?> PublicarEnFeedConFotoAsync(string pageId, string accessToken, string caption, string photoId)
{
    // Esta es la forma que Facebook documenta, y evita problemas de encoding
    var jsonMedia = System.Text.Json.JsonSerializer.Serialize(new { media_fbid = photoId });

    using var content = new FormUrlEncodedContent(new Dictionary<string, string>
    {
        ["access_token"] = accessToken,
        ["message"] = caption,
        ["attached_media[0]"] = jsonMedia
    });

    var response = await httpClient.PostAsync($"{pageId}/feed", content);
    var body = await response.Content.ReadAsStringAsync();

    if (!response.IsSuccessStatusCode)
    {
        // Aqui vas a ver si sigue siendo el error de "app en desarrollo"
        throw new Exception($"Error en /feed: {body}");
    }

    return await ExtraerIdAsync(response);
}

    public async Task<string?> PublicarTextoAsync(string pageId, string accessToken, string message)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["access_token"] = accessToken,
            ["message"] = message
        });

        var response = await httpClient.PostAsync($"{pageId}/feed", content);
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
