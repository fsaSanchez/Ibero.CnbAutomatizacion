using System;
using System.Collections.Generic;

namespace Ibero.CnbAutomatizacion.Data.Persistence.CNB_Ibero;

public partial class VwPersonasPublicablesHoy
{
    public long IdPersonaDesaparecida { get; set; }

    public string FolioUnicoIdentificacion { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public int? EdadActual { get; set; }

    public string? LugarHechos { get; set; }

    public string? CaracteristicasFisicas { get; set; }

    public string EstadoProcesamiento { get; set; } = null!;

    public bool FlagPublicadoFacebook { get; set; }

    public string? RutaFoto { get; set; }

    public DateOnly? FechaCreacionDia { get; set; }
}
