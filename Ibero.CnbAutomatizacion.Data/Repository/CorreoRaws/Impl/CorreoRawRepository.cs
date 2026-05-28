using Ibero.CnbAutomatizacion.Data.Persistence.CNB_Ibero;
using Microsoft.EntityFrameworkCore;

namespace Ibero.CnbAutomatizacion.Data.Repository.CorreoRaws.Impl;

public class CorreoRawRepository : ICorreoRawRepository
{
    private readonly CNB_IberoContext _context;

    public CorreoRawRepository(CNB_IberoContext context) => _context = context;

    public async Task<bool> ExisteAsync(string idExterno)
        => await _context.CorreoRaws
            .AsNoTracking()
            .AnyAsync(x => x.IdExterno == idExterno);

    public async Task<CorreoRaw?> GetByIdAsync(long id)
        => await _context.CorreoRaws
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdCorreoRaw == id);

    public async Task<List<CorreoRaw>> GetAllAsync()
        => await _context.CorreoRaws
            .Where(x => x.Activo == true)
            .AsNoTracking()
            .ToListAsync();

    public async Task AddAsync(CorreoRaw entity)
    {
        _context.CorreoRaws.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(CorreoRaw entity)
    {
        _context.CorreoRaws.Update(entity);
        await _context.SaveChangesAsync();
    }
}
