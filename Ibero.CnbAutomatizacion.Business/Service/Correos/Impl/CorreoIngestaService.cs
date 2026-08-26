using Ibero.CnbAutomatizacion.Business.Service.Graph;
using Ibero.CnbAutomatizacion.Data.Persistence.CNB_Ibero;
using Ibero.CnbAutomatizacion.Data.Repository.ArchivoCorreos;
using Ibero.CnbAutomatizacion.Data.Repository.BitacoraGenerals;
using Ibero.CnbAutomatizacion.Data.Repository.ConfiguracionSistemas;
using Ibero.CnbAutomatizacion.Data.Repository.CorreoRaws;
using Microsoft.Extensions.Logging;

namespace Ibero.CnbAutomatizacion.Business.Service.Correos.Impl;

public class CorreoIngestaService : ICorreoIngestaService
{
    private readonly IGraphMailService _graph;
    private readonly ICorreoRawRepository _correoRawRepo;
    private readonly IArchivoCorreoRepository _archivoCorreoRepo;
    private readonly IBitacoraGeneralRepository _bitacoraRepo;
    private readonly IConfiguracionSistemaRepository _configRepo;
    private readonly ILogger<CorreoIngestaService> _logger;

    public CorreoIngestaService(
        IGraphMailService graph,
        ICorreoRawRepository correoRawRepo,
        IArchivoCorreoRepository archivoCorreoRepo,
        IBitacoraGeneralRepository bitacoraRepo,
        IConfiguracionSistemaRepository configRepo,
        ILogger<CorreoIngestaService> logger)
    {
        _graph = graph;
        _correoRawRepo = correoRawRepo;
        _archivoCorreoRepo = archivoCorreoRepo;
        _bitacoraRepo = bitacoraRepo;
        _configRepo = configRepo;
        _logger = logger;
    }

    public async Task EjecutarIngestaAsync()
    {
        _logger.LogInformation("Iniciando ingesta de correos — {Fecha}", DateTime.Now);

        List<MensajeCorreoGraph> correos;
        try
        {
            correos = await _graph.ObtenerCorreosNoLeidosAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al conectar con Microsoft Graph");
            await _bitacoraRepo.RegistrarAsync("INGESTA_CORREOS", "Error al conectar con Microsoft Graph",
                "error", mensajeError: ex.Message);
            return;
        }

        _logger.LogInformation("Correos no leídos encontrados: {Total}", correos.Count);

        var rutaPdfs = await _configRepo.ObtenerValorAsync("ruta_almacenamiento_pdfs", @"C:\Datos\PUI\PDFs\");

        foreach (var correo in correos)
        {
            await ProcesarCorreoAsync(correo, rutaPdfs);
        }

        await _bitacoraRepo.RegistrarAsync("INGESTA_CORREOS",
            $"Ingesta completada. Correos procesados: {correos.Count}", "exitosa");
    }

    private async Task ProcesarCorreoAsync(MensajeCorreoGraph correo, string rutaPdfs)
    {
        try
        {
            // Deduplicación por id_externo
            if (await _correoRawRepo.ExisteAsync(correo.Id))
            {
                _logger.LogDebug("Correo ya existe en BD, omitiendo: {Id}", correo.Id);
                return;
            }

            //Correos que indican que se detuvo una busqueda. 
            const string textoEnAsuntoDetenerDifusion= "Cese de difusión";
            if (correo.Asunto.Contains(textoEnAsuntoDetenerDifusion))
                return;

            // Guardar correo en correo_raw
            var correoRaw = new CorreoRaw
            {
                IdExterno = correo.Id,
                Asunto = correo.Asunto,
                Remitente = correo.Remitente,
                Destinatario = "pui@ibero.mx",
                CuerpoCorreo = correo.Cuerpo,
                FechaRecepcion = correo.FechaRecepcion,
                EstadoProcesamiento = "pendiente",
                Reintentos = 0,
                FechaCreacion = DateTime.Now,
                FechaActualizacion = DateTime.Now,
                Activo = true
            };
            await _correoRawRepo.AddAsync(correoRaw);

            // Descargar y guardar adjuntos PDF
            var adjuntos = await _graph.ObtenerAdjuntosPdfAsync(correo.Id);
            await GuardarAdjuntosAsync(adjuntos, correoRaw.IdCorreoRaw, rutaPdfs);

            // Marcar como leído en Outlook
            await _graph.MarcarComoLeidoAsync(correo.Id);

            await _bitacoraRepo.RegistrarAsync("INGESTA_CORREO",
                $"Correo ingresado: {correo.Asunto}", "exitosa",
                idCorreoRaw: correoRaw.IdCorreoRaw);

            _logger.LogInformation("Correo procesado: {Asunto}", correo.Asunto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar correo {Id}", correo.Id);
            await _bitacoraRepo.RegistrarAsync("INGESTA_CORREO",
                $"Error al procesar correo: {correo.Asunto}", "error",
                mensajeError: ex.Message);
        }
    }

    private async Task GuardarAdjuntosAsync(List<AdjuntoCorreoGraph> adjuntos, long idCorreoRaw, string rutaBase)
    {
        if (adjuntos.Count == 0) return;

        if (!Directory.Exists(rutaBase))
            Directory.CreateDirectory(rutaBase);

        foreach (var adjunto in adjuntos)
        {
            try
            {
                var nombreArchivo = $"{idCorreoRaw}_{adjunto.Nombre}";
                var rutaCompleta = Path.Combine(rutaBase, nombreArchivo);

                await File.WriteAllBytesAsync(rutaCompleta, adjunto.Contenido);

                var archivoCorreo = new ArchivoCorreo
                {
                    IdCorreoRaw = idCorreoRaw,
                    NombreArchivo = adjunto.Nombre,
                    RutaDisco = rutaCompleta,
                    TamanoBytes = adjunto.TamanoBytes,
                    TipoContenido = adjunto.TipoContenido,
                    FechaCreacion = DateTime.Now,
                    FechaActualizacion = DateTime.Now,
                    Activo = true
                };
                await _archivoCorreoRepo.AddAsync(archivoCorreo);

                _logger.LogInformation("PDF guardado: {Ruta}", rutaCompleta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar adjunto: {Nombre}", adjunto.Nombre);
                await _bitacoraRepo.RegistrarAsync("GUARDAR_PDF",
                    $"Error al guardar adjunto: {adjunto.Nombre}", "error",
                    idCorreoRaw: idCorreoRaw, mensajeError: ex.Message);
            }
        }
    }
}
