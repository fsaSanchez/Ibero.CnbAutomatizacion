using Ibero.CnbAutomatizacion.API.Filters;
using Ibero.CnbAutomatizacion.Business.Service.Configuraciones;
using Ibero.CnbAutomatizacion.Entity.Request.Configuracion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ibero.CnbAutomatizacion.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ConfiguracionController(IConfiguracionSistemaService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await service.GetAllAsync());

    [HttpPut("{clave}")]
    [ServiceFilter(typeof(FilterValidation<ConfiguracionUpdateRequest>))]
    public async Task<IActionResult> Update(string clave, [FromBody] ConfiguracionUpdateRequest request)
        => Ok(await service.UpdateAsync(clave, request));
}
