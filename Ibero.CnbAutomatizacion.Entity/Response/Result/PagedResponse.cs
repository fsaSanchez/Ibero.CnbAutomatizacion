namespace Ibero.CnbAutomatizacion.Entity.Response.Result;

public class PagedResponse<T>
{
    public List<T> Datos { get; set; } = [];
    public int TotalRegistros { get; set; }
    public int Pagina { get; set; }
    public int TamanioPagina { get; set; }
    public int TotalPaginas => (int)Math.Ceiling((double)TotalRegistros / TamanioPagina);
}
