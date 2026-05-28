using Ibero.CnbAutomatizacion.Data.Persistence.CNB_Ibero;
using Microsoft.EntityFrameworkCore;

namespace Ibero.CnbAutomatizacion.Data.Repository.BitacoraPublicaciones.Impl;

public class BitacoraPublicacionRepository : IBitacoraPublicacionRepository
{
    private readonly CNB_IberoContext _context;

    public BitacoraPublicacionRepository(CNB_IberoContext context) => _context = context;

    public async Task<List<BitacoraPublicacion>> GetAllAsync()
        => await _context.BitacoraPublicacions
            .AsNoTracking()
            .OrderByDescending(x => x.FechaIntento)
            .ToListAsync();

    public async Task<List<BitacoraPublicacion>> GetByPersonaAsync(long idPersona)
        => await _context.BitacoraPublicacions
            .AsNoTracking()
            .Where(x => x.IdPersonaDesaparecida == idPersona)
            .OrderByDescending(x => x.FechaIntento)
            .ToListAsync();
}
