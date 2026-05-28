using Ibero.CnbAutomatizacion.Entity.Response.Result;

namespace Ibero.CnbAutomatizacion.Business.Service.Bitacoras;

public interface IBitacoraService
{
    Task<CommonResponse> GetPublicacionesAsync();
    Task<CommonResponse> GetGeneralAsync();
}
