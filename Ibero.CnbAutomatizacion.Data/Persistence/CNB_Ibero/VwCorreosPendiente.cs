using System;
using System.Collections.Generic;

namespace Ibero.CnbAutomatizacion.Data.Persistence.CNB_Ibero;

public partial class VwCorreosPendiente
{
    public long IdCorreoRaw { get; set; }

    public string? Asunto { get; set; }

    public string? Remitente { get; set; }

    public DateTime FechaRecepcion { get; set; }

    public string EstadoProcesamiento { get; set; } = null!;

    public int? Reintentos { get; set; }

    public string? MensajeError { get; set; }
}
