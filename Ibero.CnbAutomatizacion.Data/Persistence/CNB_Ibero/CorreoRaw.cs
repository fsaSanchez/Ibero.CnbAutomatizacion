using System;
using System.Collections.Generic;

namespace Ibero.CnbAutomatizacion.Data.Persistence.CNB_Ibero;

public partial class CorreoRaw
{
    public long IdCorreoRaw { get; set; }

    public string IdExterno { get; set; } = null!;

    public string? Asunto { get; set; }

    public string? Remitente { get; set; }

    public string Destinatario { get; set; } = null!;

    public string? CuerpoCorreo { get; set; }

    public DateTime FechaRecepcion { get; set; }

    public DateTime? FechaProcesamiento { get; set; }

    public string EstadoProcesamiento { get; set; } = null!;

    public string? MensajeError { get; set; }

    public int? Reintentos { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime FechaActualizacion { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<ArchivoCorreo> ArchivoCorreos { get; set; } = new List<ArchivoCorreo>();

    public virtual ICollection<BitacoraGeneral> BitacoraGenerals { get; set; } = new List<BitacoraGeneral>();

    public virtual ICollection<PersonaDesaparecidum> PersonaDesaparecida { get; set; } = new List<PersonaDesaparecidum>();
}
