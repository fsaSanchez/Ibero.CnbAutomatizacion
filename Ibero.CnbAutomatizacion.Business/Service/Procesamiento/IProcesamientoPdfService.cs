namespace Ibero.CnbAutomatizacion.Business.Service.Procesamiento;

public interface IProcesamientoPdfService
{
    Task ProcesarPendientesAsync();
    Task ReintentarIncompletosAsync();
}
