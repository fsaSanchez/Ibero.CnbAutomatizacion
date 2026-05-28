using Ibero.CnbAutomatizacion.Data.Persistence.CNB_Ibero;

namespace Ibero.CnbAutomatizacion.Data.Repository.VwPersonasPublicables;

public interface IVwPersonasPublicablesHoyRepository
{
    Task<VwPersonasPublicablesHoy?> ObtenerParaPublicarAsync();
}
