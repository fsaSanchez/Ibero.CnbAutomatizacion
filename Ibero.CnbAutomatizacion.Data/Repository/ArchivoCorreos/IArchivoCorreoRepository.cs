using Ibero.CnbAutomatizacion.Data.Persistence.CNB_Ibero;

namespace Ibero.CnbAutomatizacion.Data.Repository.ArchivoCorreos;

public interface IArchivoCorreoRepository
{
    Task AddAsync(ArchivoCorreo entity);
    Task<List<ArchivoCorreo>> GetByCorreoRawAsync(long idCorreoRaw);
}
