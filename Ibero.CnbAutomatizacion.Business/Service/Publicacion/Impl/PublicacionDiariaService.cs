using Ibero.CnbAutomatizacion.Data.Repository.BitacoraGenerals;
using Ibero.CnbAutomatizacion.Data.Repository.BitacoraPublicaciones;
using Ibero.CnbAutomatizacion.Data.Repository.ConfiguracionSistemas;
using Ibero.CnbAutomatizacion.Data.Repository.VwPersonasPublicables;
using Microsoft.Extensions.Logging;

namespace Ibero.CnbAutomatizacion.Business.Service.Publicacion.Impl;

public class PublicacionDiariaService(
    IBitacoraPublicacionRepository bitacoraPublicacionRepo,
    IVwPersonasPublicablesHoyRepository vwRepo,
    IPublicacionFacebookService publicacionService,
    IBitacoraGeneralRepository bitacoraGeneralRepo,
    IConfiguracionSistemaRepository configRepo,
    ILogger<PublicacionDiariaService> logger) : IPublicacionDiariaService
{
    public async Task EjecutarAsync()
    {
        var habilitado = await configRepo.ObtenerValorAsync("habilitar_publicacion_automatica", "true");
        if (!string.Equals(habilitado, "true", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogInformation("Publicación automática deshabilitada por configuración del sistema.");
            return;
        }

        logger.LogInformation("Iniciando publicación diaria Facebook — {Fecha}", DateTime.Now);

        if (await bitacoraPublicacionRepo.HuboPublicacionExitosaHoyAsync())
        {
            logger.LogInformation("Ya se realizó una publicación exitosa hoy. Omitiendo.");
            return;
        }

        var candidato = await vwRepo.ObtenerParaPublicarAsync();
        if (candidato is null)
        {
            logger.LogInformation("No hay personas candidatas para publicar hoy.");
            await bitacoraGeneralRepo.RegistrarAsync(
                "PUBLICACION_DIARIA", "No hay candidatos para publicar hoy", "informativo");
            return;
        }

        logger.LogInformation("Publicando persona {Id} — {Nombre}", candidato.IdPersonaDesaparecida, candidato.Nombre);

        var resultado = await publicacionService.PublicarAsync(candidato.IdPersonaDesaparecida, "automatica");

        var estadoAccion = resultado.Success ? "exitosa" : "error";
        await bitacoraGeneralRepo.RegistrarAsync(
            "PUBLICACION_DIARIA",
            $"Publicación automática persona {candidato.IdPersonaDesaparecida}: {resultado.Message}",
            estadoAccion,
            idPersonaDesaparecida: candidato.IdPersonaDesaparecida,
            mensajeError: resultado.Success ? null : resultado.Message);

        logger.LogInformation("Publicación diaria finalizada. Estado: {Estado}", estadoAccion);
    }
}
