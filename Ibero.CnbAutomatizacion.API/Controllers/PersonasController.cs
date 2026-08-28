using Ibero.CnbAutomatizacion.Business.Service.CargaPdf;
using Ibero.CnbAutomatizacion.Business.Service.Personas;
using Ibero.CnbAutomatizacion.Business.Service.Pdf;
using Ibero.CnbAutomatizacion.Entity.Request.Personas;
using Ibero.CnbAutomatizacion.Entity.Response.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ibero.CnbAutomatizacion.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PersonasController : ControllerBase
{
    private readonly IPersonaDesaparecidaService _personaDesaparecidaService;
    private readonly ICargaPdfManualService _cargaPdfService;
    private readonly IPdfExtractorService _pdfExtractor;

    private const long MaxPdfBytes = 10L * 1024 * 1024; // 10 MB

    public PersonasController(
        IPersonaDesaparecidaService personaDesaparecidaService,
        ICargaPdfManualService cargaPdfService,
        IPdfExtractorService pdfExtractor)
    {
        _personaDesaparecidaService = personaDesaparecidaService;
        _cargaPdfService = cargaPdfService;
        _pdfExtractor = pdfExtractor;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PersonaFilterRequest filter)
        => Ok(await _personaDesaparecidaService.GetPagedAsync(filter));

    [AllowAnonymous]
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
        => Ok(await _personaDesaparecidaService.GetByIdAsync(id));

    [AllowAnonymous]
    [HttpGet("archivo")]
    public async Task<IActionResult> GetArchivo([FromQuery] string ruta)
        => Ok(await _personaDesaparecidaService.ObtenerArchivoBase64Async(ruta));

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
        => Ok(await _personaDesaparecidaService.DeleteAsync(id));

    [HttpPost("{id:long}/publicar")]
    public async Task<IActionResult> Publicar(long id)
        => Ok(await _personaDesaparecidaService.PublicarAsync(id));

    [HttpPost("cese-difusion")]
    public async Task<IActionResult> CeseDifusion([FromBody] CeseDifusionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Fui))
            return Ok(Fail("El FUI es requerido."));

        return Ok(await _personaDesaparecidaService.CeseDifusionAsync(request.Fui.Trim()));
    }

    [HttpPost("cargar-pdf-manual")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CargarPdfManual(IFormFile archivo)
    {
        var (bytes, error) = await LeerPdfAsync(archivo);
        if (error != null) return Ok(Fail(error));

        return Ok(await _cargaPdfService.CargarAsync(bytes!, archivo!.FileName));
    }

    [HttpPost("pdf/listar-imagenes")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ListarImagenesPdf(IFormFile archivo)
    {
        var (bytes, error) = await LeerPdfAsync(archivo);
        if (error != null) return Ok(Fail(error));

        var imagenes = await _pdfExtractor.ListarImagenesAsync(bytes!);
        return Ok(new CommonResponse
        {
            Success = true,
            Code = 200,
            Message = imagenes.Count > 0
                ? $"Se encontraron {imagenes.Count} imagen(es) en el PDF."
                : "No se encontraron imágenes en el PDF.",
            Data = imagenes
        });
    }

    private async Task<(byte[]? Bytes, string? Error)> LeerPdfAsync(IFormFile? archivo)
    {
        if (archivo is null || archivo.Length == 0)
            return (null, "No se recibió ningún archivo.");

        if (!archivo.ContentType.Contains("pdf", StringComparison.OrdinalIgnoreCase) &&
            !Path.GetExtension(archivo.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            return (null, "Solo se aceptan archivos PDF.");

        if (archivo.Length > MaxPdfBytes)
            return (null, "El archivo supera el límite de 10 MB.");

        using var ms = new MemoryStream();
        await archivo.CopyToAsync(ms);
        return (ms.ToArray(), null);
    }

    private static CommonResponse Fail(string mensaje) =>
        new() { Success = false, Code = 400, Message = mensaje };
}