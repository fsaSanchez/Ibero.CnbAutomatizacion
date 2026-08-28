using Ibero.CnbAutomatizacion.Data.Persistence.CNB_Ibero;
using Ibero.CnbAutomatizacion.Data.Repository.BitacoraPublicaciones;
using Ibero.CnbAutomatizacion.Data.Repository.ConfiguracionSistemas;
using Ibero.CnbAutomatizacion.Data.Repository.PersonaDesaparecidas;
using Ibero.CnbAutomatizacion.Entity.Response.Result;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Ibero.CnbAutomatizacion.Business.Service.Publicacion.Impl;

public class PublicacionFacebookService(
    IPersonaDesaparecidaRepository personaRepo,
    IBitacoraPublicacionRepository bitacoraRepo,
    IConfiguracionSistemaRepository configRepo,
    IFacebookGraphClient facebookClient,
    IConfiguration configuration,
    ILogger<PublicacionFacebookService> logger) : BaseService, IPublicacionFacebookService
{
    public async Task<CommonResponse> PublicarAsync(long idPersonaDesaparecida, string tipoPublicacion)
    {
        var persona = await personaRepo.GetByIdWithFotosAsync(idPersonaDesaparecida);
        if (persona is null)
            return CreateResponseFail("Persona desaparecida no encontrada", 404);

        if (persona.FlagPublicadoFacebook && tipoPublicacion == "manual")
            return CreateResponseFail("Esta persona ya fue publicada en Facebook");

        var pageId = configuration["Facebook:PageId"] ?? string.Empty;
        var accessToken = configuration["Facebook:AccessToken"] ?? string.Empty;

        if (string.IsNullOrWhiteSpace(pageId) || string.IsNullOrWhiteSpace(accessToken))
            return CreateResponseFail("Configuración de Facebook incompleta (PageId / AccessToken)", 500);

        var infoContacto = await configRepo.ObtenerValorAsync("informacion_contacto",
            "Para más información contacte a las autoridades competentes.");

        var caption = ConstruirCaption(persona, infoContacto);

        persona.IntentoPublicacionFacebook = (persona.IntentoPublicacionFacebook ?? 0) + 1;

        string? idExterno = null;
        string estadoPublicacion;
        string? mensajeError = null;

        try
        {
            idExterno = await IntentarPublicarAsync(persona, pageId, accessToken, caption);
            estadoPublicacion = idExterno is not null ? "exitosa" : "fallida";
            if (idExterno is null)
                mensajeError = "La API de Facebook no devolvió un ID de publicación";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al publicar en Facebook para persona {Id}", idPersonaDesaparecida);
            estadoPublicacion = "fallida";
            mensajeError = ex.Message;
        }

        var bitacora = new BitacoraPublicacion
        {
            IdPersonaDesaparecida = idPersonaDesaparecida,
            TipoRedSocial = "facebook",
            EstadoPublicacion = estadoPublicacion,
            TipoPublicacion = tipoPublicacion,
            IdPublicacionExterna = idExterno,
            MensajeError = mensajeError,
            FechaIntento = DateTime.Now,
            FechaPublicacionReal = estadoPublicacion == "exitosa" ? DateTime.Now : null,
            FechaCreacion = DateTime.Now
        };
        await bitacoraRepo.RegistrarAsync(bitacora);

        if (estadoPublicacion == "exitosa")
        {
            persona.FlagPublicadoFacebook = true;
            persona.FechaPublicacionFacebook = DateTime.Now;
            persona.FechaActualizacion = DateTime.Now;
            await personaRepo.UpdateAsync(persona);

            logger.LogInformation("Publicación exitosa en Facebook. Persona: {Id}, Post: {PostId}",
                idPersonaDesaparecida, idExterno);
            return CreateResponseOk("Publicación exitosa en Facebook", data: new { idPublicacion = idExterno });
        }

        persona.FechaActualizacion = DateTime.Now;
        await personaRepo.UpdateAsync(persona);

        return CreateResponseFail($"Error al publicar en Facebook: {mensajeError}", 502);
    }

    /// <summary>
    /// Elimina de Facebook todas las publicaciones exitosas registradas para una persona
    /// (usado por el proceso de cese de difusión). No modifica PublicarAsync.
    /// </summary>
    public async Task<int> EliminarPublicacionesDePersonaAsync(long idPersonaDesaparecida)
    {
        var publicaciones = (await bitacoraRepo.GetByPersonaAsync(idPersonaDesaparecida))
            .Where(p => p.EstadoPublicacion == "exitosa" && !string.IsNullOrWhiteSpace(p.IdPublicacionExterna))
            .ToList();

        if (publicaciones.Count == 0)
            return 0;

        var accessToken = configuration["Facebook:AccessToken"] ?? string.Empty;
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            logger.LogWarning("No se pudo eliminar publicaciones de Facebook para persona {Id}: falta Facebook:AccessToken",
                idPersonaDesaparecida);
            return 0;
        }

        var eliminadas = 0;
        foreach (var publicacion in publicaciones)
        {
            try
            {
                var ok = await facebookClient.EliminarPublicacionAsync(publicacion.IdPublicacionExterna!, accessToken);
                if (ok)
                {
                    publicacion.EstadoPublicacion = "eliminada";
                    await bitacoraRepo.UpdateAsync(publicacion);
                    eliminadas++;
                }
                else
                {
                    logger.LogWarning("No se pudo eliminar de Facebook la publicación {PostId} (persona {Id})",
                        publicacion.IdPublicacionExterna, idPersonaDesaparecida);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al eliminar de Facebook la publicación {PostId} (persona {Id})",
                    publicacion.IdPublicacionExterna, idPersonaDesaparecida);
            }
        }

        return eliminadas;
    }

    private async Task<string?> IntentarPublicarAsync(
        PersonaDesaparecidum persona, string pageId, string accessToken, string caption)
    {
        var fotoPrincipal = persona.FotoPersonas.FirstOrDefault(f => f.Principal && f.Activo);

        if (fotoPrincipal is not null && File.Exists(fotoPrincipal.RutaDisco))
        {
            var bytes = await File.ReadAllBytesAsync(fotoPrincipal.RutaDisco);
            return await facebookClient.PublicarFotoAsync(pageId, accessToken, bytes, caption);
        }

        logger.LogWarning("Sin foto en disco para persona {Id}. Publicando solo texto.", persona.IdPersonaDesaparecida);
        return await facebookClient.PublicarTextoAsync(pageId, accessToken, caption);
    }

    private static string ConstruirCaption(PersonaDesaparecidum p, string infoContacto)
    {
        var sb = new StringBuilder();
        sb.AppendLine("🔴 ALERTA DE PERSONA DESAPARECIDA");
        sb.AppendLine();
        sb.AppendLine($"Nombre: {p.Nombre}");

        if (p.EdadActual.HasValue)
            sb.AppendLine($"Edad: {p.EdadActual} años");

        if (!string.IsNullOrWhiteSpace(p.LugarHechos))
            sb.AppendLine($"Lugar: {p.LugarHechos}");

        if (p.FechaHechos.HasValue)
            sb.AppendLine($"Fecha: {p.FechaHechos.Value:dd/MM/yyyy}");

        if (!string.IsNullOrWhiteSpace(p.CarpetaInvestigacion))
            sb.AppendLine($"Carpeta: {p.CarpetaInvestigacion}");

        sb.AppendLine();
        sb.Append(infoContacto);

        return sb.ToString();
    }
}
