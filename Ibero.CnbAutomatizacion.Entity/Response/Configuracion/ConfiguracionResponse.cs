namespace Ibero.CnbAutomatizacion.Entity.Response.Configuracion;

public class ConfiguracionResponse
{
    public long IdConfiguracion { get; set; }
    public string Clave { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
    public string TipoValor { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Editable { get; set; }
    public DateTime FechaActualizacion { get; set; }
}
