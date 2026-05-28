using System;
using System.Collections.Generic;

namespace Ibero.CnbAutomatizacion.Data.Persistence.CNB_Ibero;

public partial class VwEstadisticasSistema
{
    public int? TotalPersonas { get; set; }

    public int? PersonasCompletas { get; set; }

    public int? PersonasIncompletas { get; set; }

    public int? PersonasPublicadas { get; set; }

    public int? CorreosPendientes { get; set; }

    public int? PublicacionesFallidasHoy { get; set; }
}
