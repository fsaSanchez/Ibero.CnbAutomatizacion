using Ibero.CnbAutomatizacion.Data.Persistence.CNB_Ibero;
using Microsoft.EntityFrameworkCore;

namespace Ibero.CnbAutomatizacion.Data.Repository.ArchivoCorreos.Impl;

public class ArchivoCorreoRepository : IArchivoCorreoRepository
{
    private readonly CNB_IberoContext _context;

    public ArchivoCorreoRepository(CNB_IberoContext context) => _context = context;

    public async Task AddAsync(ArchivoCorreo entity)
    {
        _context.ArchivoCorreos.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<List<ArchivoCorreo>> GetByCorreoRawAsync(long idCorreoRaw)
        => await _context.ArchivoCorreos
            .Where(x => x.IdCorreoRaw == idCorreoRaw && x.Activo == true)
            .AsNoTracking()
            .ToListAsync();
}
