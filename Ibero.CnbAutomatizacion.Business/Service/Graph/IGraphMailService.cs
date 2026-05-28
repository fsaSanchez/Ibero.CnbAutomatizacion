namespace Ibero.CnbAutomatizacion.Business.Service.Graph;

public record MensajeCorreoGraph(
    string Id,
    string? Asunto,
    string? Remitente,
    DateTime FechaRecepcion,
    string? Cuerpo);

public record AdjuntoCorreoGraph(
    string Nombre,
    byte[] Contenido,
    long TamanoBytes,
    string? TipoContenido);

public interface IGraphMailService
{
    Task<List<MensajeCorreoGraph>> ObtenerCorreosNoLeidosAsync();
    Task<List<AdjuntoCorreoGraph>> ObtenerAdjuntosPdfAsync(string mensajeId);
    Task MarcarComoLeidoAsync(string mensajeId);
}
