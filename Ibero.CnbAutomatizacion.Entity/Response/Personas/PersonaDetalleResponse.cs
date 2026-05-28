namespace Ibero.CnbAutomatizacion.Entity.Response.Personas;

public class PersonaDetalleResponse
{
    public long IdPersonaDesaparecida { get; set; }
    public long IdCorreoRaw { get; set; }
    public string FolioUnicoIdentificacion { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int? EdadActual { get; set; }
    public int? EdadMomentoDesaparicion { get; set; }
    public string? Sexo { get; set; }
    public string? Genero { get; set; }
    public string? Nacionalidad { get; set; }
    public string? LugarNacimiento { get; set; }
    public string? LugarHechos { get; set; }
    public DateOnly? FechaHechos { get; set; }
    public DateOnly? FechaPercate { get; set; }
    public string? CaracteristicasFisicas { get; set; }
    public string? SenasParticulares { get; set; }
    public string? PrendasVestir { get; set; }
    public string? AutoridadesCompetentes { get; set; }
    public string? CarpetaInvestigacion { get; set; }
    public string? Idioma { get; set; }
    public string? Discapacidad { get; set; }
    public string EstadoProcesamiento { get; set; } = string.Empty;
    public string? CamposIncompletos { get; set; }
    public bool FlagPublicadoFacebook { get; set; }
    public DateTime? FechaPublicacionFacebook { get; set; }
    public int? IntentoPublicacionFacebook { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string? RutaFotoPrincipal { get; set; }
    public List<FotoPersonaResponse> Fotos { get; set; } = [];
}
