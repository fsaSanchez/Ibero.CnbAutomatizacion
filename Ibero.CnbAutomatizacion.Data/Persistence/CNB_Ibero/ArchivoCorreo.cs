using System;
using System.Collections.Generic;

namespace Ibero.CnbAutomatizacion.Data.Persistence.CNB_Ibero;

public partial class ArchivoCorreo
{
    public long IdArchivoCorreo { get; set; }

    public long IdCorreoRaw { get; set; }

    public string NombreArchivo { get; set; } = null!;

    public string RutaDisco { get; set; } = null!;

    public long TamanoBytes { get; set; }

    public string? TipoContenido { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime FechaActualizacion { get; set; }

    public bool Activo { get; set; }

    public virtual CorreoRaw IdCorreoRawNavigation { get; set; } = null!;
}
