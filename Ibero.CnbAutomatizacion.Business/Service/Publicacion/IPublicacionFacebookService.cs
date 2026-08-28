using Ibero.CnbAutomatizacion.Entity.Response.Result;

namespace Ibero.CnbAutomatizacion.Business.Service.Publicacion;

public interface IPublicacionFacebookService
{
    Task<CommonResponse> PublicarAsync(long idPersonaDesaparecida, string tipoPublicacion);
    Task<int> EliminarPublicacionesDePersonaAsync(long idPersonaDesaparecida);
}
