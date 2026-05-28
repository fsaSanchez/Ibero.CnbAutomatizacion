using Ibero.CnbAutomatizacion.Data.Persistence.CNB_Ibero;
using Ibero.CnbAutomatizacion.Entity.Request.Personas;

namespace Ibero.CnbAutomatizacion.Data.Repository.PersonaDesaparecidas;

public interface IPersonaDesaparecidaRepository
{
    Task AddAsync(PersonaDesaparecidum entity);
    Task UpdateAsync(PersonaDesaparecidum entity);
    Task<PersonaDesaparecidum?> GetByIdAsync(long id);
    Task<PersonaDesaparecidum?> GetByIdWithFotosAsync(long id);
    Task<List<PersonaDesaparecidum>> GetByCorreoRawAsync(long idCorreoRaw);
    Task<List<PersonaDesaparecidum>> GetIncompletosAsync();
    Task<List<PersonaDesaparecidum>> GetAllAsync();
    Task<(List<PersonaDesaparecidum> Items, int Total)> GetPagedAsync(PersonaFilterRequest filter);
}
