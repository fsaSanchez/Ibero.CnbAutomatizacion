using Ibero.CnbAutomatizacion.Data.Persistence.CNB_Ibero;
using Ibero.CnbAutomatizacion.Entity.Response.Bitacora;
using Ibero.CnbAutomatizacion.Entity.Response.Configuracion;
using Ibero.CnbAutomatizacion.Entity.Response.Personas;
using Mapster;

namespace Ibero.CnbAutomatizacion.Business.MappingConfigs;

public class PersonaDesaparecidaMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<PersonaDesaparecidum, PersonaResumenResponse>()
            .Map(dest => dest.RutaFotoPrincipal, src =>
                src.FotoPersonas.FirstOrDefault(f => f.Principal && f.Activo) != null
                    ? src.FotoPersonas.First(f => f.Principal && f.Activo).RutaDisco
                    : null);

        config.NewConfig<PersonaDesaparecidum, PersonaDetalleResponse>()
            .Map(dest => dest.RutaFotoPrincipal, src =>
                src.FotoPersonas.FirstOrDefault(f => f.Principal && f.Activo) != null
                    ? src.FotoPersonas.First(f => f.Principal && f.Activo).RutaDisco
                    : null)
            .Map(dest => dest.Fotos, src => src.FotoPersonas.Where(f => f.Activo).ToList());

        config.NewConfig<FotoPersona, FotoPersonaResponse>();
        config.NewConfig<ConfiguracionSistema, ConfiguracionResponse>();
        config.NewConfig<BitacoraPublicacion, BitacoraPublicacionResponse>();
        config.NewConfig<BitacoraGeneral, BitacoraGeneralResponse>();
    }
}
