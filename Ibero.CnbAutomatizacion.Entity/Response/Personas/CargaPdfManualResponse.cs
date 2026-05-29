namespace Ibero.CnbAutomatizacion.Entity.Response.Personas;

public class CargaPdfManualResponse
{
    public long IdPersonaDesaparecida { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string FolioUnicoIdentificacion { get; set; } = string.Empty;
    public List<string> CamposIncompletos { get; set; } = [];
}
