namespace Ibero.CnbAutomatizacion.Entity.Response.Bitacora;

public class BitacoraPublicacionResponse
{
    public long IdBitacoraPublicacion { get; set; }
    public long IdPersonaDesaparecida { get; set; }
    public string TipoRedSocial { get; set; } = string.Empty;
    public string EstadoPublicacion { get; set; } = string.Empty;
    public string TipoPublicacion { get; set; } = string.Empty;
    public string? IdPublicacionExterna { get; set; }
    public string? UsuarioPublicador { get; set; }
    public string? MensajeError { get; set; }
    public DateTime FechaIntento { get; set; }
    public DateTime? FechaPublicacionReal { get; set; }
    public DateTime FechaCreacion { get; set; }
}
