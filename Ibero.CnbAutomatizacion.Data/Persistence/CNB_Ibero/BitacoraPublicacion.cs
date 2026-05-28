using System;
using System.Collections.Generic;

namespace Ibero.CnbAutomatizacion.Data.Persistence.CNB_Ibero;

public partial class BitacoraPublicacion
{
    public long IdBitacoraPublicacion { get; set; }

    public long IdPersonaDesaparecida { get; set; }

    public string TipoRedSocial { get; set; } = null!;

    public string EstadoPublicacion { get; set; } = null!;

    public string? IdPublicacionExterna { get; set; }

    public DateTime FechaIntento { get; set; }

    public DateTime? FechaPublicacionReal { get; set; }

    public string? MensajeError { get; set; }

    public string TipoPublicacion { get; set; } = null!;

    public string? UsuarioPublicador { get; set; }

    public DateTime FechaCreacion { get; set; }

    public virtual PersonaDesaparecidum IdPersonaDesaparecidaNavigation { get; set; } = null!;
}
