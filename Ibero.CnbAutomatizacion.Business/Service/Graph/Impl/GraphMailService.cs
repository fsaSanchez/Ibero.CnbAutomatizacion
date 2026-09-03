using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace Ibero.CnbAutomatizacion.Business.Service.Graph.Impl;

public class GraphMailService : IGraphMailService
{
    private readonly GraphServiceClient _graphClient;
    private readonly string _buzon;

    public GraphMailService(IConfiguration config)
    {
        var tenantId = config["MicrosoftGraph:TenantId"]!;
        var clientId = config["MicrosoftGraph:ClientId"]!;
        var clientSecret = config["MicrosoftGraph:ClientSecret"]!;
        _buzon = config["MicrosoftGraph:Buzon"]!;

        var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
        _graphClient = new GraphServiceClient(credential);
    }

    public async Task<List<MensajeCorreoGraph>> ObtenerCorreosNoLeidosAsync()
    {
        var resultado = new List<MensajeCorreoGraph>();
        var hoy = DateTime.Today;
        var mañana = hoy.AddDays(1);
        var mensajes = await _graphClient.Users[_buzon].Messages
        .GetAsync(req =>
        {
            // Combinar condiciones con "and"
            req.QueryParameters.Filter =
     $"isRead eq false and receivedDateTime ge {hoy:yyyy-MM-ddT00:00:00Z} and receivedDateTime lt {mañana:yyyy-MM-ddT00:00:00Z}";
            req.QueryParameters.Select = ["id", "subject", "from", "receivedDateTime", "body"];
            req.QueryParameters.Top = 500;
        });

        if (mensajes?.Value == null) return resultado;

        foreach (var msg in mensajes.Value)
        {
            resultado.Add(new MensajeCorreoGraph(
                Id: msg.Id!,
                Asunto: msg.Subject,
                Remitente: msg.From?.EmailAddress?.Address,
                FechaRecepcion: msg.ReceivedDateTime?.UtcDateTime ?? DateTime.Now,
                Cuerpo: msg.Body?.Content));
        }

        return resultado;
    }

    public async Task<List<AdjuntoCorreoGraph>> ObtenerAdjuntosPdfAsync(string mensajeId)
    {
        var resultado = new List<AdjuntoCorreoGraph>();

        var adjuntos = await _graphClient.Users[_buzon].Messages[mensajeId].Attachments
            .GetAsync();

        if (adjuntos?.Value == null) return resultado;

        foreach (var adjunto in adjuntos.Value)
        {
            if (adjunto is not FileAttachment fileAtt) continue;

            var tipo = fileAtt.ContentType ?? string.Empty;
            if (!tipo.Contains("pdf", StringComparison.OrdinalIgnoreCase) &&
                !fileAtt.Name!.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                continue;

            var contenido = fileAtt.ContentBytes ?? [];

            resultado.Add(new AdjuntoCorreoGraph(
                Nombre: fileAtt.Name!,
                Contenido: contenido,
                TamanoBytes: contenido.Length,
                TipoContenido: tipo));
        }

        return resultado;
    }

    public async Task MarcarComoLeidoAsync(string mensajeId)
    {
        //await _graphClient.Users[_buzon].Messages[mensajeId]
        //    .PatchAsync(new Message { IsRead = true });
    }
}
