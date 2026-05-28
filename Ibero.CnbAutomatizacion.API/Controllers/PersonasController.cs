using Ibero.CnbAutomatizacion.Business.Service.Personas;
using Ibero.CnbAutomatizacion.Entity.Request.Personas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ibero.CnbAutomatizacion.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PersonasController(IPersonaDesaparecidaService service) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PersonaFilterRequest filter)
        => Ok(await service.GetPagedAsync(filter));

    [AllowAnonymous]
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
        => Ok(await service.GetByIdAsync(id));

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
        => Ok(await service.DeleteAsync(id));

    [HttpPost("{id:long}/publicar")]
    public async Task<IActionResult> Publicar(long id)
        => Ok(await service.PublicarAsync(id));
}
