using Hangfire;
using Ibero.CnbAutomatizacion.Business.Service.Procesamiento;

namespace Ibero.CnbAutomatizacion.API.Jobs;

public class ReintentoDatosIncompletosJob(IProcesamientoPdfService service)
{
    [AutomaticRetry(Attempts = 2, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public async Task Ejecutar() => await service.ReintentarIncompletosAsync();
}
