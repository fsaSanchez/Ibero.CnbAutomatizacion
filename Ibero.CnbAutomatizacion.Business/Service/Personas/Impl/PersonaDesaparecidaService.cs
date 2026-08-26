using Ibero.CnbAutomatizacion.Business.Service.Publicacion;
using Ibero.CnbAutomatizacion.Data.Repository.ConfiguracionSistemas;
using Ibero.CnbAutomatizacion.Data.Repository.PersonaDesaparecidas;
using Ibero.CnbAutomatizacion.Entity.Request.Personas;
using Ibero.CnbAutomatizacion.Entity.Response.Personas;
using Ibero.CnbAutomatizacion.Entity.Response.Result;
using Mapster;

namespace Ibero.CnbAutomatizacion.Business.Service.Personas.Impl;

public class PersonaDesaparecidaService(
    IPersonaDesaparecidaRepository repository,
    IPublicacionFacebookService publicacionService,
    IConfiguracionSistemaRepository configRepo) : BaseService, IPersonaDesaparecidaService
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
        persona.FechaActualizacion = DateTime.Now;
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

    public async Task<CommonResponse> ObtenerArchivoBase64Async(string ruta)
    {
        if (string.IsNullOrWhiteSpace(ruta))
            return CreateResponseFail("La ruta del archivo no puede estar vacía.", 400);

        // Canonicalizar la ruta solicitada para prevenir path traversal
        string rutaCanonica;
        try { rutaCanonica = Path.GetFullPath(ruta); }
        catch { return CreateResponseFail("La ruta del archivo no es válida.", 400); }

        // Verificar que la ruta esté dentro de uno de los directorios permitidos
        var rutaFotos = Path.GetFullPath(
            await configRepo.ObtenerValorAsync("ruta_almacenamiento_fotos", @"C:\Datos\PUI\Fotos\"));
        var rutaPdfs = Path.GetFullPath(
            await configRepo.ObtenerValorAsync("ruta_almacenamiento_pdfs", @"C:\Datos\PUI\PDFs\"));

        bool enRutaPermitida = rutaCanonica.StartsWith(rutaFotos, StringComparison.OrdinalIgnoreCase)
                            || rutaCanonica.StartsWith(rutaPdfs,  StringComparison.OrdinalIgnoreCase);

        if (!enRutaPermitida)
            return CreateResponseFail("Acceso denegado: la ruta no pertenece a un directorio permitido.", 403);

        if (!File.Exists(rutaCanonica))
            return CreateResponseFail("El archivo no existe en el servidor.", 404);

        var bytes = await File.ReadAllBytesAsync(rutaCanonica);
        var extension = Path.GetExtension(rutaCanonica).ToLowerInvariant();
        var contentType = extension switch
        {
            ".png"  => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".pdf"  => "application/pdf",
            _       => "application/octet-stream"
        };

        return CreateResponseOk(data: new
        {
            base64      = Convert.ToBase64String(bytes),
            contentType,
            nombreArchivo = Path.GetFileName(rutaCanonica)
        });
    }
}
