using Ibero.CnbAutomatizacion.Data.Persistence.CNB_Ibero;

namespace Ibero.CnbAutomatizacion.Data.Repository.CorreoRaws;

public interface ICorreoRawRepository
{
    Task<bool> ExisteAsync(string idExterno);
    Task<CorreoRaw?> GetByIdAsync(long id);
    Task<List<CorreoRaw>> GetAllAsync();
    Task<List<CorreoRaw>> GetByEstadoAsync(string estado, int? maxReintentos = null);
    Task AddAsync(CorreoRaw entity);
    Task UpdateAsync(CorreoRaw entity);
}
