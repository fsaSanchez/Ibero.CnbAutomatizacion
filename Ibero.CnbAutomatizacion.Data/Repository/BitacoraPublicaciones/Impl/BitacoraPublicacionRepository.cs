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

    public async Task RegistrarAsync(BitacoraPublicacion entity)
    {
        _context.BitacoraPublicacions.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HuboPublicacionExitosaHoyAsync()
    {
        var hoy = DateTime.Now.Date;
        return await _context.BitacoraPublicacions
            .AsNoTracking()
            .AnyAsync(x => x.FechaIntento.Date == hoy && x.EstadoPublicacion == "exitosa");
    }
}
