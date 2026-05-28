using Ibero.CnbAutomatizacion.Data.Persistence.CNB_Ibero;
using Microsoft.EntityFrameworkCore;

namespace Ibero.CnbAutomatizacion.Data.Repository.VwPersonasPublicables.Impl;

public class VwPersonasPublicablesHoyRepository : IVwPersonasPublicablesHoyRepository
{
    private readonly CNB_IberoContext _context;

    public VwPersonasPublicablesHoyRepository(CNB_IberoContext context) => _context = context;

    public async Task<VwPersonasPublicablesHoy?> ObtenerParaPublicarAsync()
    {
        var candidatos = await _context.VwPersonasPublicablesHoys
            .Where(v => !v.FlagPublicadoFacebook)
            .AsNoTracking()
            .ToListAsync();

        if (candidatos.Count == 0)
            return null;

        return candidatos[Random.Shared.Next(candidatos.Count)];
    }
}
