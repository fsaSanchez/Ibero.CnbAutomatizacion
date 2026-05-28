using Ibero.CnbAutomatizacion.Data.Repository.ConfiguracionSistemas;
using Ibero.CnbAutomatizacion.Entity.Request.Configuracion;
using Ibero.CnbAutomatizacion.Entity.Response.Configuracion;
using Ibero.CnbAutomatizacion.Entity.Response.Result;
using Mapster;

namespace Ibero.CnbAutomatizacion.Business.Service.Configuraciones.Impl;

public class ConfiguracionSistemaService(IConfiguracionSistemaRepository repository)
    : BaseService, IConfiguracionSistemaService
{
    public async Task<CommonResponse> GetAllAsync()
    {
        var configs = await repository.GetAllAsync();
        return CreateResponseOk(data: configs.Adapt<List<ConfiguracionResponse>>());
    }

    public async Task<CommonResponse> UpdateAsync(string clave, ConfiguracionUpdateRequest request)
    {
        var config = await repository.GetByClaveAsync(clave);
        if (config is null)
            return CreateResponseFail("Configuración no encontrada", 404);

        if (!config.Editable)
            return CreateResponseFail("Esta configuración no es editable");

        config.Valor = request.Valor;
        config.FechaActualizacion = DateTime.UtcNow;
        await repository.UpdateAsync(config);
        return CreateResponseOk("Configuración actualizada correctamente");
    }
}
