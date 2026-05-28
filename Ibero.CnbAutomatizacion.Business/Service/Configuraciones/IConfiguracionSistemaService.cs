using Ibero.CnbAutomatizacion.Entity.Request.Configuracion;
using Ibero.CnbAutomatizacion.Entity.Response.Result;

namespace Ibero.CnbAutomatizacion.Business.Service.Configuraciones;

public interface IConfiguracionSistemaService
{
    Task<CommonResponse> GetAllAsync();
    Task<CommonResponse> UpdateAsync(string clave, ConfiguracionUpdateRequest request);
}
