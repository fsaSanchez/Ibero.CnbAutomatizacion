using System;
using System.Collections.Generic;

namespace Ibero.CnbAutomatizacion.Data.Persistence.CNB_Ibero;

public partial class BitacoraGeneral
{
    public long IdBitacoraGeneral { get; set; }

    public string TipoAccion { get; set; } = null!;

    public string? Descripcion { get; set; }

    public long? IdCorreoRaw { get; set; }

    public long? IdPersonaDesaparecida { get; set; }

    public string? Usuario { get; set; }

    public string? IpOrigen { get; set; }

    public string EstadoAccion { get; set; } = null!;

    public string? MensajeError { get; set; }

    public DateTime FechaAccion { get; set; }

    public virtual CorreoRaw? IdCorreoRawNavigation { get; set; }

    public virtual PersonaDesaparecidum? IdPersonaDesaparecidaNavigation { get; set; }
}
