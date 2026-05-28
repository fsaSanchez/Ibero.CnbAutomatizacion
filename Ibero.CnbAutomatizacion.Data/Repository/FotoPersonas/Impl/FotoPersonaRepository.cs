using Ibero.CnbAutomatizacion.Data.Persistence.CNB_Ibero;
using Microsoft.EntityFrameworkCore;

namespace Ibero.CnbAutomatizacion.Data.Repository.FotoPersonas.Impl;

public class FotoPersonaRepository : IFotoPersonaRepository
{
    private readonly CNB_IberoContext _context;

    public FotoPersonaRepository(CNB_IberoContext context) => _context = context;

    public async Task AddAsync(FotoPersona entity)
    {
        _context.FotoPersonas.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<List<FotoPersona>> GetByPersonaAsync(long idPersonaDesaparecida)
        => await _context.FotoPersonas
            .Where(x => x.IdPersonaDesaparecida == idPersonaDesaparecida && x.Activo == true)
            .AsNoTracking()
            .ToListAsync();
}
