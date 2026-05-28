using Ibero.CnbAutomatizacion.Business.Service.Publicacion;
using Ibero.CnbAutomatizacion.Data.Repository.PersonaDesaparecidas;
using Ibero.CnbAutomatizacion.Entity.Request.Personas;
using Ibero.CnbAutomatizacion.Entity.Response.Personas;
using Ibero.CnbAutomatizacion.Entity.Response.Result;
using Mapster;

namespace Ibero.CnbAutomatizacion.Business.Service.Personas.Impl;

public class PersonaDesaparecidaService(
    IPersonaDesaparecidaRepository repository,
    IPublicacionFacebookService publicacionService) : BaseService, IPersonaDesaparecidaService
{
    public async Task<CommonResponse> GetPagedAsync(PersonaFilterRequest filter)
    {
        var (items, total) = await repository.GetPagedAsync(filter);
        var datos = items.Adapt<List<PersonaResumenResponse>>();
        var paged = new PagedResponse<PersonaResumenResponse>
        {
            Datos = datos,
            TotalRegistros = total,
            Pagina = filter.Pagina,
            TamanioPagina = filter.TamanioPagina
        };
        return CreateResponseOk(data: paged);
    }

    public async Task<CommonResponse> GetByIdAsync(long id)
    {
        var persona = await repository.GetByIdWithFotosAsync(id);
        if (persona is null)
            return CreateResponseFail("Persona desaparecida no encontrada", 404);

        return CreateResponseOk(data: persona.Adapt<PersonaDetalleResponse>());
    }

    public async Task<CommonResponse> DeleteAsync(long id)
    {
        var persona = await repository.GetByIdAsync(id);
        if (persona is null)
            return CreateResponseFail("Persona desaparecida no encontrada", 404);

        persona.Activo = false;
        persona.FechaActualizacion = DateTime.UtcNow;
        await repository.UpdateAsync(persona);
        return CreateResponseOk("Persona eliminada correctamente");
    }

    public async Task<CommonResponse> PublicarAsync(long id)
    {
        var persona = await repository.GetByIdAsync(id);
        if (persona is null)
            return CreateResponseFail("Persona desaparecida no encontrada", 404);

        return await publicacionService.PublicarAsync(id, "manual");
    }
}
