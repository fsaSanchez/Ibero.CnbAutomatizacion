using Ibero.CnbAutomatizacion.Data.Persistence.CNB_Ibero;

namespace Ibero.CnbAutomatizacion.Data.Repository.FotoPersonas;

public interface IFotoPersonaRepository
{
    Task AddAsync(FotoPersona entity);
    Task<List<FotoPersona>> GetByPersonaAsync(long idPersonaDesaparecida);
}
