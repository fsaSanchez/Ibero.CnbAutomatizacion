using Ibero.CnbAutomatizacion.Business.Service.Procesamiento;

namespace Ibero.CnbAutomatizacion.API.Jobs;

public class ProcesamientoPdfJob(IProcesamientoPdfService service)
{
    public async Task Ejecutar() => await service.ProcesarPendientesAsync();
}
