using Ibero.CnbAutomatizacion.Business.Service.Procesamiento;

namespace Ibero.CnbAutomatizacion.API.Jobs;

public class ReintentoDatosIncompletosJob(IProcesamientoPdfService service)
{
    public async Task Ejecutar() => await service.ReintentarIncompletosAsync();
}
