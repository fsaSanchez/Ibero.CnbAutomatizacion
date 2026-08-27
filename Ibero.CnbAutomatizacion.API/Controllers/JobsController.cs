using Hangfire;
using Ibero.CnbAutomatizacion.API.Jobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ibero.CnbAutomatizacion.API.Controllers;

/// <summary>
/// Permite disparar manualmente la ejecución de los jobs de Hangfire,
/// sin esperar a que se cumpla su intervalo programado.
/// No modifica la lógica ni la programación (schedule) de los jobs existentes.
/// </summary>
[Route("api/[controller]")]
[ApiController]
//[Authorize]
public class JobsController(IBackgroundJobClient jobClient) : ControllerBase
{
    // POST: api/jobs/ingesta-correos
    [HttpPost("ingesta-correos")]
    public IActionResult EjecutarIngestaCorreos()
    {
        var jobId = jobClient.Enqueue<IngestaCorreosJob>(j => j.Ejecutar());
        return Ok(new { message = "Ingesta de correos encolada para ejecución inmediata.", jobId });
    }
}
