using Ibero.CnbAutomatizacion.Entity.Response.Result;

namespace Ibero.CnbAutomatizacion.Business.Service.Publicacion.Impl;

public class PublicacionFacebookService : BaseService, IPublicacionFacebookService
{
    public Task<CommonResponse> PublicarAsync(long idPersonaDesaparecida, string tipoPublicacion)
        => Task.FromResult(CreateResponseFail("Módulo de publicación Facebook en construcción", 501));
}
