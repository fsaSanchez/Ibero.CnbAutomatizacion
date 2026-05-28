using Ibero.CnbAutomatizacion.Data.Persistence.CNB_Ibero;
using Microsoft.EntityFrameworkCore;

namespace Ibero.CnbAutomatizacion.Data.Repository.BitacoraGenerals.Impl;

public class BitacoraGeneralRepository : IBitacoraGeneralRepository
{
    private readonly CNB_IberoContext _context;

    public BitacoraGeneralRepository(CNB_IberoContext context) => _context = context;

    public async Task RegistrarAsync(string tipoAccion, string descripcion, string estadoAccion,
        long? idCorreoRaw = null, long? idPersonaDesaparecida = null, string? mensajeError = null)
    {
        var entrada = new BitacoraGeneral
        {
            TipoAccion = tipoAccion,
            Descripcion = descripcion,
            EstadoAccion = estadoAccion,
            IdCorreoRaw = idCorreoRaw,
            IdPersonaDesaparecida = idPersonaDesaparecida,
            MensajeError = mensajeError,
            FechaAccion = DateTime.UtcNow
        };
        _context.BitacoraGenerals.Add(entrada);
        await _context.SaveChangesAsync();
    }

    public async Task<List<BitacoraGeneral>> GetAllAsync()
        => await _context.BitacoraGenerals
            .AsNoTracking()
            .OrderByDescending(x => x.FechaAccion)
            .ToListAsync();
}
