using Ibero.CnbAutomatizacion.Business.Service.Bitacoras;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ibero.CnbAutomatizacion.API.Controllers;

[Route("api/[controller]")]
[ApiController]
//[Authorize]
[AllowAnonymous]
public class BitacoraController(IBitacoraService service) : ControllerBase
{
    [HttpGet("publicaciones")]
    public async Task<IActionResult> GetPublicaciones()
        => Ok(await service.GetPublicacionesAsync());

    [HttpGet("general")]
    public async Task<IActionResult> GetGeneral()
        => Ok(await service.GetGeneralAsync());
}
