using Ibero.CnbAutomatizacion.Entity.Request.Personas;
using Ibero.CnbAutomatizacion.Entity.Response.Result;

namespace Ibero.CnbAutomatizacion.Business.Service.Personas;

public interface IPersonaDesaparecidaService
{
    Task<CommonResponse> GetPagedAsync(PersonaFilterRequest filter);
    Task<CommonResponse> GetByIdAsync(long id);
    Task<CommonResponse> DeleteAsync(long id);
    Task<CommonResponse> PublicarAsync(long id);
    Task<CommonResponse> ObtenerArchivoBase64Async(string ruta);
}
