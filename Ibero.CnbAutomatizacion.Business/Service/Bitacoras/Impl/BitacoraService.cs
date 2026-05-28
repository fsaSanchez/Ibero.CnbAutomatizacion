using Ibero.CnbAutomatizacion.Data.Repository.BitacoraGenerals;
using Ibero.CnbAutomatizacion.Data.Repository.BitacoraPublicaciones;
using Ibero.CnbAutomatizacion.Entity.Response.Bitacora;
using Ibero.CnbAutomatizacion.Entity.Response.Result;
using Mapster;

namespace Ibero.CnbAutomatizacion.Business.Service.Bitacoras.Impl;

public class BitacoraService(
    IBitacoraPublicacionRepository publicacionRepo,
    IBitacoraGeneralRepository generalRepo) : BaseService, IBitacoraService
{
    public async Task<CommonResponse> GetPublicacionesAsync()
    {
        var items = await publicacionRepo.GetAllAsync();
        return CreateResponseOk(data: items.Adapt<List<BitacoraPublicacionResponse>>());
    }

    public async Task<CommonResponse> GetGeneralAsync()
    {
        var items = await generalRepo.GetAllAsync();
        return CreateResponseOk(data: items.Adapt<List<BitacoraGeneralResponse>>());
    }
}
