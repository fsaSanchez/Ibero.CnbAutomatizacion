using Ibero.CnbAutomatizacion.Data.Persistence.CNB_Ibero;

namespace Ibero.CnbAutomatizacion.Data.Repository.ConfiguracionSistemas;

public interface IConfiguracionSistemaRepository
{
    Task<string?> ObtenerValorAsync(string clave);
    Task<string> ObtenerValorAsync(string clave, string valorDefault);
    Task<List<ConfiguracionSistema>> GetAllAsync();
    Task<ConfiguracionSistema?> GetByClaveAsync(string clave);
    Task UpdateAsync(ConfiguracionSistema entity);
}
