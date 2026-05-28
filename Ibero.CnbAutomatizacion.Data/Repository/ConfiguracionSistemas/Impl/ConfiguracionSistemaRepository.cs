using Ibero.CnbAutomatizacion.Data.Persistence.CNB_Ibero;
using Microsoft.EntityFrameworkCore;

namespace Ibero.CnbAutomatizacion.Data.Repository.ConfiguracionSistemas.Impl;

public class ConfiguracionSistemaRepository : IConfiguracionSistemaRepository
{
    private readonly CNB_IberoContext _context;

    public ConfiguracionSistemaRepository(CNB_IberoContext context) => _context = context;

    public async Task<string?> ObtenerValorAsync(string clave)
        => await _context.ConfiguracionSistemas
            .AsNoTracking()
            .Where(x => x.Clave == clave)
            .Select(x => x.Valor)
            .FirstOrDefaultAsync();

    public async Task<string> ObtenerValorAsync(string clave, string valorDefault)
        => await ObtenerValorAsync(clave) ?? valorDefault;

    public async Task<List<ConfiguracionSistema>> GetAllAsync()
        => await _context.ConfiguracionSistemas
            .AsNoTracking()
            .OrderBy(x => x.Clave)
            .ToListAsync();

    public async Task<ConfiguracionSistema?> GetByClaveAsync(string clave)
        => await _context.ConfiguracionSistemas
            .FirstOrDefaultAsync(x => x.Clave == clave);

    public async Task UpdateAsync(ConfiguracionSistema entity)
    {
        _context.ConfiguracionSistemas.Update(entity);
        await _context.SaveChangesAsync();
    }
}
