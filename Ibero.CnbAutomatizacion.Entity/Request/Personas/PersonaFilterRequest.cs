namespace Ibero.CnbAutomatizacion.Entity.Request.Personas;

public class PersonaFilterRequest
{
    public string? Folio { get; set; }
    public string? Nombre { get; set; }
    public string? Estado { get; set; }
    public DateOnly? FechaHechos { get; set; }
    public string? Busqueda { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamanioPagina { get; set; } = 20;
}
