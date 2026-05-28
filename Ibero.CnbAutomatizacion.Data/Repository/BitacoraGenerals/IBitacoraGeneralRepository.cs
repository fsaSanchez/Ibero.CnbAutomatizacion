using Ibero.CnbAutomatizacion.Data.Persistence.CNB_Ibero;

namespace Ibero.CnbAutomatizacion.Data.Repository.BitacoraGenerals;

public interface IBitacoraGeneralRepository
{
    Task RegistrarAsync(string tipoAccion, string descripcion, string estadoAccion,
        long? idCorreoRaw = null, long? idPersonaDesaparecida = null, string? mensajeError = null);
    Task<List<BitacoraGeneral>> GetAllAsync();
}
