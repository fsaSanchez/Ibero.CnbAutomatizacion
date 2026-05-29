using Ibero.CnbAutomatizacion.Business.Service.CargaPdf;
using Ibero.CnbAutomatizacion.Business.Service.Personas;
using Ibero.CnbAutomatizacion.Entity.Request.Personas;
using Ibero.CnbAutomatizacion.Entity.Response.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ibero.CnbAutomatizacion.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PersonasController(
    IPersonaDesaparecidaService service,
    ICargaPdfManualService cargaPdfService) : ControllerBase
{
    private const long MaxPdfBytes = 10L * 1024 * 1024; // 10 MB

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

    [HttpPost("cargar-pdf-manual")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CargarPdfManual(IFormFile archivo)
    {
        if (archivo is null || archivo.Length == 0)
            return Ok(Fail("No se recibió ningún archivo."));

        if (!archivo.ContentType.Contains("pdf", StringComparison.OrdinalIgnoreCase) &&
            !Path.GetExtension(archivo.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            return Ok(Fail("Solo se aceptan archivos PDF."));

        if (archivo.Length > MaxPdfBytes)
            return Ok(Fail("El archivo supera el límite de 10 MB."));

        using var ms = new MemoryStream();
        await archivo.CopyToAsync(ms);
        return Ok(await cargaPdfService.CargarAsync(ms.ToArray(), archivo.FileName));
    }

    private static CommonResponse Fail(string mensaje) =>
        new() { Success = false, Code = 400, Message = mensaje };
}
