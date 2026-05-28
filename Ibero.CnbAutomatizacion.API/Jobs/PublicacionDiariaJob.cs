using Hangfire;
using Ibero.CnbAutomatizacion.Business.Service.Publicacion;

namespace Ibero.CnbAutomatizacion.API.Jobs;

public class PublicacionDiariaJob(IPublicacionDiariaService service)
{
    [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public async Task Ejecutar() => await service.EjecutarAsync();
}
