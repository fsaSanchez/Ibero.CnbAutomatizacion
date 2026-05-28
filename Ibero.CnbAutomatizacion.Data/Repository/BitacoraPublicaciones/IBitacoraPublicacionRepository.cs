using Ibero.CnbAutomatizacion.Data.Persistence.CNB_Ibero;

namespace Ibero.CnbAutomatizacion.Data.Repository.BitacoraPublicaciones;

public interface IBitacoraPublicacionRepository
{
    Task<List<BitacoraPublicacion>> GetAllAsync();
    Task<List<BitacoraPublicacion>> GetByPersonaAsync(long idPersona);
    Task RegistrarAsync(BitacoraPublicacion entity);
    Task<bool> HuboPublicacionExitosaHoyAsync();
}
