using Ibero.CnbAutomatizacion.Data.Persistence.CNB_Ibero;
using Ibero.CnbAutomatizacion.Entity.Request.Personas;
using Microsoft.EntityFrameworkCore;

namespace Ibero.CnbAutomatizacion.Data.Repository.PersonaDesaparecidas.Impl;

public class PersonaDesaparecidaRepository : IPersonaDesaparecidaRepository
{
    private readonly CNB_IberoContext _context;

    public PersonaDesaparecidaRepository(CNB_IberoContext context) => _context = context;

    public async Task AddAsync(PersonaDesaparecidum entity)
    {
        _context.PersonaDesaparecida.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(PersonaDesaparecidum entity)
    {
        _context.PersonaDesaparecida.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<PersonaDesaparecidum?> GetByIdAsync(long id)
        => await _context.PersonaDesaparecida
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdPersonaDesaparecida == id && x.Activo);

    public async Task<PersonaDesaparecidum?> GetByIdWithFotosAsync(long id)
        => await _context.PersonaDesaparecida
            .Include(p => p.FotoPersonas.Where(f => f.Activo))
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdPersonaDesaparecida == id && x.Activo);

    public async Task<List<PersonaDesaparecidum>> GetByCorreoRawAsync(long idCorreoRaw)
        => await _context.PersonaDesaparecida
            .Where(x => x.IdCorreoRaw == idCorreoRaw && x.Activo == true)
            .AsNoTracking()
            .ToListAsync();

    public async Task<List<PersonaDesaparecidum>> GetIncompletosAsync()
        => await _context.PersonaDesaparecida
            .Where(x => x.EstadoProcesamiento == "incompleto" && x.Activo == true)
            .AsNoTracking()
            .ToListAsync();

    public async Task<List<PersonaDesaparecidum>> GetAllAsync()
        => await _context.PersonaDesaparecida
            .Where(x => x.Activo == true)
            .AsNoTracking()
            .ToListAsync();

    public async Task<(List<PersonaDesaparecidum> Items, int Total)> GetPagedAsync(PersonaFilterRequest filter)
    {
        var query = _context.PersonaDesaparecida
            .Include(p => p.FotoPersonas.Where(f => f.Activo))
            .Where(p => p.Activo)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Folio))
            query = query.Where(p => p.FolioUnicoIdentificacion.Contains(filter.Folio));

        if (!string.IsNullOrWhiteSpace(filter.Nombre))
            query = query.Where(p => p.Nombre.Contains(filter.Nombre));

        if (!string.IsNullOrWhiteSpace(filter.Estado))
            query = query.Where(p => p.EstadoProcesamiento == filter.Estado);

        if (filter.FechaHechos.HasValue)
            query = query.Where(p => p.FechaHechos == filter.FechaHechos.Value);

        if (!string.IsNullOrWhiteSpace(filter.Busqueda))
        {
            var b = filter.Busqueda;
            query = query.Where(p =>
                p.Nombre.Contains(b) ||
                p.FolioUnicoIdentificacion.Contains(b) ||
                (p.LugarHechos != null && p.LugarHechos.Contains(b)) ||
                (p.CarpetaInvestigacion != null && p.CarpetaInvestigacion.Contains(b)));
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.FechaCreacion)
            .Skip((filter.Pagina - 1) * filter.TamanioPagina)
            .Take(filter.TamanioPagina)
            .ToListAsync();

        return (items, total);
    }
}
