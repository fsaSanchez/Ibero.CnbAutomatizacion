using Hangfire;
using Ibero.CnbAutomatizacion.Business.Service.Correos;

namespace Ibero.CnbAutomatizacion.API.Jobs;

public class IngestaCorreosJob(ICorreoIngestaService service, IBackgroundJobClient jobClient)
{
    [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public async Task Ejecutar()
    {
        await service.EjecutarIngestaAsync();
        jobClient.Enqueue<ProcesamientoPdfJob>(j => j.Ejecutar());
    }
}
