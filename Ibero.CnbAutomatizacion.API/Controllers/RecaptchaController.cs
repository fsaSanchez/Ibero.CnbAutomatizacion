using Ibero.CnbAutomatizacion.Business.Service.Recaptcha;
using Ibero.CnbAutomatizacion.Entity.Request.Recaptcha;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ibero.CnbAutomatizacion.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class RecaptchaController(IRecaptchaService service) : ControllerBase
{
    [HttpPost("verificar")]
    public async Task<IActionResult> Verificar([FromBody] RecaptchaVerifyRequest request)
        => Ok(await service.VerificarAsync(request));
}
