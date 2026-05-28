namespace Ibero.CnbAutomatizacion.Entity.Response.Bitacora;

public class BitacoraGeneralResponse
{
    public long IdBitacoraGeneral { get; set; }
    public string TipoAccion { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string EstadoAccion { get; set; } = string.Empty;
    public string? MensajeError { get; set; }
    public long? IdCorreoRaw { get; set; }
    public long? IdPersonaDesaparecida { get; set; }
    public string? Usuario { get; set; }
    public string? IpOrigen { get; set; }
    public DateTime FechaAccion { get; set; }
}
