namespace Ibero.CnbAutomatizacion.Entity.Response.Personas;

public class CeseDifusionResponse
{
    public string Fui { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public long PersonaId { get; set; }
    public int PublicacionesFacebookEliminadas { get; set; }
}
