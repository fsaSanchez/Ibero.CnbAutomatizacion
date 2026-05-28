using System;
using System.Collections.Generic;

namespace Ibero.CnbAutomatizacion.Data.Persistence.CNB_Ibero;

public partial class PersonaDesaparecidum
{
    public long IdPersonaDesaparecida { get; set; }

    public long IdCorreoRaw { get; set; }

    public string FolioUnicoIdentificacion { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public int? EdadActual { get; set; }

    public int? EdadMomentoDesaparicion { get; set; }

    public string? Sexo { get; set; }

    public string? Genero { get; set; }

    public string? Nacionalidad { get; set; }

    public string? LugarNacimiento { get; set; }

    public string? LugarHechos { get; set; }

    public DateOnly? FechaHechos { get; set; }

    public DateOnly? FechaPercate { get; set; }

    public string? CaracteristicasFisicas { get; set; }

    public string? SenasParticulares { get; set; }

    public string? PrendasVestir { get; set; }

    public string? AutoridadesCompetentes { get; set; }

    public string? CarpetaInvestigacion { get; set; }

    public string? Idioma { get; set; }

    public string? Discapacidad { get; set; }

    public string EstadoProcesamiento { get; set; } = null!;

    public string? CamposIncompletos { get; set; }

    public DateTime? FechaPublicacionFacebook { get; set; }

    public bool FlagPublicadoFacebook { get; set; }

    public int? IntentoPublicacionFacebook { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime FechaActualizacion { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<BitacoraGeneral> BitacoraGenerals { get; set; } = new List<BitacoraGeneral>();

    public virtual ICollection<BitacoraPublicacion> BitacoraPublicacions { get; set; } = new List<BitacoraPublicacion>();

    public virtual ICollection<FotoPersona> FotoPersonas { get; set; } = new List<FotoPersona>();

    public virtual CorreoRaw IdCorreoRawNavigation { get; set; } = null!;
}
