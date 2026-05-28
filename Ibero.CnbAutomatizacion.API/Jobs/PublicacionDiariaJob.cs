using Ibero.CnbAutomatizacion.Business.Service.Publicacion;

namespace Ibero.CnbAutomatizacion.API.Jobs;

public class PublicacionDiariaJob(IPublicacionDiariaService service)
{
    public async Task Ejecutar() => await service.EjecutarAsync();
}
