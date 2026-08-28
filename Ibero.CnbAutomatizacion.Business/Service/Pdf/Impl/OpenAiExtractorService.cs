using System.Globalization;
using System.Text;
using System.Text.Json;
using Ibero.CnbAutomatizacion.Business.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;

namespace Ibero.CnbAutomatizacion.Business.Service.Pdf.Impl;

public class OpenAiExtractorService : IOpenAiExtractorService
{
    private readonly ChatClient _chatClient;
    private readonly ILogger<OpenAiExtractorService> _logger;

    public OpenAiExtractorService(IConfiguration config, ILogger<OpenAiExtractorService> logger)
    {
        _logger = logger;
        var apiKey = config["OpenAI:ApiKey"] ?? string.Empty;
        var model = config["OpenAI:Model"] ?? "gpt-4o-mini";
        var client = new OpenAIClient(apiKey);
        _chatClient = client.GetChatClient(model);
    }

    public async Task<FichaExtraidaDto> ExtraerAsync(string textoPdf, FichaExtraidaDto parcial)
    {
        var camposFaltantes = string.Join(", ", parcial.CamposIncompletos);
        var prompt = ConstruirPrompt(textoPdf, camposFaltantes);

        try
        {
            var completion = await _chatClient.CompleteChatAsync(

                 
                [
                new SystemChatMessage(
                    "Eres un extractor de datos de fichas de búsqueda de personas desaparecidas mexicanas. " +
                    "Responde SOLO con un objeto JSON válido con los campos solicitados. " +
                    "Si no encuentras el dato, usa null. No incluyas texto adicional."+
             "Sin markdown, sin ```json, sin texto extra."),
                new UserChatMessage(prompt)
            ],new ChatCompletionOptions()
            {
                ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
            });

            var json = completion.Value.Content[0].Text;
            AplicarRespuestaJson(json, parcial);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al llamar OpenAI para extracción de campos: {Campos}", camposFaltantes);
        }

        parcial.CamposIncompletos = RecalcularIncompletos(parcial);
        return parcial;
    }

    public async Task<string?> ExtraerFUIAsync(string cuerpoCorreo)
    {
        var contenido = cuerpoCorreo.Length > 4000 ? cuerpoCorreo[..4000] : cuerpoCorreo;
        var prompt =
            "Extrae el Folio Único de Identificación (FUI) del siguiente contenido de un correo electrónico.\n" +
            "El FUI tiene el formato: 'FI' + 2 dígitos + '-' + 5 grupos de caracteres hexadecimales separados por guiones.\n" +
            "Ejemplo: FI26-1B14E1C30-EC2F-4A1D-A74F-273B6C93279A\n" +
            "Responde ÚNICAMENTE con el FUI encontrado, sin texto adicional, sin markdown.\n" +
            "Si no encuentras ningún FUI en el contenido, responde exactamente: NULL\n\n" +
            "CONTENIDO:\n" + contenido;

        try
        {
            var completion = await _chatClient.CompleteChatAsync(
            [
                new SystemChatMessage(
                    "Eres un extractor preciso de identificadores en texto. Respondes solo el dato solicitado, sin explicaciones."),
                new UserChatMessage(prompt)
            ]);

            var respuesta = completion.Value.Content[0].Text.Trim();
            return string.Equals(respuesta, "NULL", StringComparison.OrdinalIgnoreCase) ? null : respuesta;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al llamar OpenAI para extraer FUI de correo de cese de difusión");
            return null;
        }
    }

    private static string ConstruirPrompt(string texto, string camposFaltantes)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Del siguiente texto de una Ficha de Búsqueda de Persona Desaparecida, extrae SOLO estos campos:");
        sb.AppendLine(camposFaltantes);
        sb.AppendLine();
        sb.AppendLine("Devuelve un JSON con exactamente estas claves (usa null si no encuentras el valor):");
        sb.AppendLine("{");
        sb.AppendLine("  \"folio_unico_identificacion\": \"...\",");
        sb.AppendLine("  \"nombre\": \"...\",");
        sb.AppendLine("  \"edad_actual\": 0,");
        sb.AppendLine("  \"sexo\": \"...\",");
        sb.AppendLine("  \"lugar_hechos\": \"...\",");
        sb.AppendLine("  \"fecha_hechos\": \"dd/MM/yyyy\",");
        sb.AppendLine("  \"carpeta_investigacion\": \"...\"");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("TEXTO DEL PDF:");
        sb.AppendLine(texto.Length > 4000 ? texto[..4000] : texto);
        return sb.ToString();
    }

    private static void AplicarRespuestaJson(string json, FichaExtraidaDto ficha)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        ficha.FolioUnicoIdentificacion ??= GetStr(root, "folio_unico_identificacion");
        ficha.Nombre                   ??= GetStr(root, "nombre");
        ficha.Sexo                     ??= GetStr(root, "sexo");
        ficha.LugarHechos              ??= GetStr(root, "lugar_hechos");
        ficha.CarpetaInvestigacion     ??= GetStr(root, "carpeta_investigacion");

        if (ficha.EdadActual == null && root.TryGetProperty("edad_actual", out var edadEl))
        {
            if (edadEl.ValueKind == JsonValueKind.Number) ficha.EdadActual = edadEl.GetInt32();
            else if (edadEl.ValueKind == JsonValueKind.String && int.TryParse(edadEl.GetString(), out var e))
                ficha.EdadActual = e;
        }

        if (ficha.FechaHechos == null)
        {
            var fStr = GetStr(root, "fecha_hechos");
            if (fStr != null && DateOnly.TryParseExact(fStr, "dd/MM/yyyy",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var fd))
                ficha.FechaHechos = fd;
        }
    }

    private static string? GetStr(JsonElement root, string key)
    {
        if (!root.TryGetProperty(key, out var el)) return null;
        return el.ValueKind == JsonValueKind.String ? el.GetString() : null;
    }

    private static List<string> RecalcularIncompletos(FichaExtraidaDto f)
    {
        var lista = new List<string>();
        if (string.IsNullOrWhiteSpace(f.FolioUnicoIdentificacion)) lista.Add("folio_unico_identificacion");
        if (string.IsNullOrWhiteSpace(f.Nombre))                   lista.Add("nombre");
        if (f.EdadActual == null)                                   lista.Add("edad_actual");
        if (string.IsNullOrWhiteSpace(f.Sexo))                     lista.Add("sexo");
        if (string.IsNullOrWhiteSpace(f.LugarHechos))              lista.Add("lugar_hechos");
        if (f.FechaHechos == null)                                  lista.Add("fecha_hechos");
        if (string.IsNullOrWhiteSpace(f.CarpetaInvestigacion))     lista.Add("carpeta_investigacion");
        return lista;
    }
}
