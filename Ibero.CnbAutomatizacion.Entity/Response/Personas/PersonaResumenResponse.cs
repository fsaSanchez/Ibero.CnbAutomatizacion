namespace Ibero.CnbAutomatizacion.Entity.Response.Personas;

public class PersonaResumenResponse
{
    public long IdPersonaDesaparecida { get; set; }
    public string FolioUnicoIdentificacion { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int? EdadActual { get; set; }
    public string? Sexo { get; set; }
    public string? LugarHechos { get; set; }
    public DateOnly? FechaHechos { get; set; }
    public string EstadoProcesamiento { get; set; } = string.Empty;
    public bool FlagPublicadoFacebook { get; set; }
    public string? RutaFotoPrincipal { get; set; }
}
