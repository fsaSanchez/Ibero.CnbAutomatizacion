using System.Net.Http;
using System.Text.Json;

namespace Ibero.CnbAutomatizacion.Business.Service.Publicacion.Impl;

public class FacebookGraphClient(HttpClient httpClient) : IFacebookGraphClient
{
    public async Task<string?> PublicarFotoAsync(string pageId, string accessToken, byte[] fotoBytes, string caption)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(accessToken), "access_token");
        content.Add(new StringContent(caption), "caption");
        content.Add(new ByteArrayContent(fotoBytes), "source", "foto.jpg");

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

    private static async Task<string?> ExtraerIdAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.TryGetProperty("id", out var id) ? id.GetString() : null;
    }
}
