namespace Ibero.CnbAutomatizacion.Business.Models;

public class FichaExtraidaDto
{
    public string? FolioUnicoIdentificacion { get; set; }
    public string? Nombre { get; set; }
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
    public byte[]? FotoBytes { get; set; }
    public string FotoExtension { get; set; } = ".jpg";
    public List<string> CamposIncompletos { get; set; } = [];
}
