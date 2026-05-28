using Hangfire;
using Ibero.CnbAutomatizacion.Business.Service.Procesamiento;

namespace Ibero.CnbAutomatizacion.API.Jobs;

public class ProcesamientoPdfJob(IProcesamientoPdfService service)
{
    [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public async Task Ejecutar() => await service.ProcesarPendientesAsync();
}
