namespace Ibero.CnbAutomatizacion.Entity.Response.Personas;

public class FotoPersonaResponse
{
    public long IdFotoPersona { get; set; }
    public string RutaDisco { get; set; } = string.Empty;
    public string NombreArchivo { get; set; } = string.Empty;
    public long TamanoBytes { get; set; }
    public bool Principal { get; set; }
    public string Image64{ get; set; }
}
