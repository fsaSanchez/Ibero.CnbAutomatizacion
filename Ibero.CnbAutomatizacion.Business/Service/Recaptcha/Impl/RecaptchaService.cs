using System.Text.Json;
using System.Text.Json.Serialization;
using Ibero.CnbAutomatizacion.Entity.Request.Recaptcha;
using Ibero.CnbAutomatizacion.Entity.Response.Result;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Ibero.CnbAutomatizacion.Business.Service.Recaptcha.Impl;

public class RecaptchaService(HttpClient httpClient, IConfiguration config, IHostEnvironment environment)
    : BaseService, IRecaptchaService
{
    public async Task<CommonResponse> VerificarAsync(RecaptchaVerifyRequest request)
    {
        if (environment.IsDevelopment())
            return CreateResponseOk("reCAPTCHA omitido en ambiente de desarrollo.", data: new { Score = 1.0 });

        if (string.IsNullOrWhiteSpace(request.Token))
            return CreateResponseFail("Token de reCAPTCHA no recibido.");

        var secretKey = config["Recaptcha:SecretKey"];
        if (string.IsNullOrWhiteSpace(secretKey) || secretKey == "YOUR_RECAPTCHA_SECRET_KEY_HERE")
            return CreateResponseFail("reCAPTCHA no está configurado en el servidor.", 500);

        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["secret"] = secretKey,
            ["response"] = request.Token
        });

        var response = await httpClient.PostAsync("siteverify", content);
        if (!response.IsSuccessStatusCode)
            return CreateResponseFail("No se pudo validar reCAPTCHA con Google.", 502);

        var body = await response.Content.ReadAsStringAsync();
        var resultado = JsonSerializer.Deserialize<RecaptchaGoogleResponse>(body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (resultado is null || !resultado.Success)
            return CreateResponseFail("Verificación de reCAPTCHA fallida.");

        if (!string.IsNullOrWhiteSpace(request.Action) &&
            !string.IsNullOrWhiteSpace(resultado.Action) &&
            !string.Equals(resultado.Action, request.Action, StringComparison.OrdinalIgnoreCase))
            return CreateResponseFail("Acción de reCAPTCHA no coincide.");

        var minScore = double.TryParse(config["Recaptcha:MinScore"], out var parsedMinScore)
            ? parsedMinScore
            : 0.5;
        if (resultado.Score < minScore)
            return CreateResponseFail("No se pudo verificar que la solicitud proviene de un humano.");

        return CreateResponseOk("Verificación exitosa.", data: new { resultado.Score });
    }

    private class RecaptchaGoogleResponse
    {
        public bool Success { get; set; }
        public double Score { get; set; }
        public string? Action { get; set; }

        [JsonPropertyName("error-codes")]
        public List<string>? ErrorCodes { get; set; }
    }
}
