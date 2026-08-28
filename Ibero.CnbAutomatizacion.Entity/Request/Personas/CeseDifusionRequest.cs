using System.ComponentModel.DataAnnotations;

namespace Ibero.CnbAutomatizacion.Entity.Request.Personas;

public class CeseDifusionRequest
{
    [Required]
    public string Fui { get; set; } = string.Empty;
}
