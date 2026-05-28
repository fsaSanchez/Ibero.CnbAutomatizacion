using System;
using System.Collections.Generic;

namespace Ibero.CnbAutomatizacion.Data.Persistence.CNB_Ibero;

public partial class FotoPersona
{
    public long IdFotoPersona { get; set; }

    public long IdPersonaDesaparecida { get; set; }

    public string RutaDisco { get; set; } = null!;

    public string NombreArchivo { get; set; } = null!;

    public long TamanoBytes { get; set; }

    public string? TipoContenido { get; set; }

    public string? IdLaserfiche { get; set; }

    public bool Principal { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime FechaActualizacion { get; set; }

    public bool Activo { get; set; }

    public virtual PersonaDesaparecidum IdPersonaDesaparecidaNavigation { get; set; } = null!;
}
