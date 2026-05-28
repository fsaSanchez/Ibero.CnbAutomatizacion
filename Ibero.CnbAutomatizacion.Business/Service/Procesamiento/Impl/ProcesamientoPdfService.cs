using Ibero.CnbAutomatizacion.Business.Models;
using Ibero.CnbAutomatizacion.Business.Service.Pdf;
using Ibero.CnbAutomatizacion.Data.Persistence.CNB_Ibero;
using Ibero.CnbAutomatizacion.Data.Repository.ArchivoCorreos;
using Ibero.CnbAutomatizacion.Data.Repository.BitacoraGenerals;
using Ibero.CnbAutomatizacion.Data.Repository.ConfiguracionSistemas;
using Ibero.CnbAutomatizacion.Data.Repository.CorreoRaws;
using Ibero.CnbAutomatizacion.Data.Repository.FotoPersonas;
using Ibero.CnbAutomatizacion.Data.Repository.PersonaDesaparecidas;
using Microsoft.Extensions.Logging;

namespace Ibero.CnbAutomatizacion.Business.Service.Procesamiento.Impl;

public class ProcesamientoPdfService : IProcesamientoPdfService
{
    private readonly ICorreoRawRepository _correoRawRepo;
    private readonly IArchivoCorreoRepository _archivoCorreoRepo;
    private readonly IPersonaDesaparecidaRepository _personaRepo;
    private readonly IFotoPersonaRepository _fotoRepo;
    private readonly IBitacoraGeneralRepository _bitacoraRepo;
    private readonly IConfiguracionSistemaRepository _configRepo;
    private readonly IPdfExtractorService _pdfExtractor;
    private readonly IRegexExtractorService _regexExtractor;
    private readonly IOpenAiExtractorService _openAiExtractor;
    private readonly ILogger<ProcesamientoPdfService> _logger;

    public ProcesamientoPdfService(
        ICorreoRawRepository correoRawRepo,
        IArchivoCorreoRepository archivoCorreoRepo,
        IPersonaDesaparecidaRepository personaRepo,
        IFotoPersonaRepository fotoRepo,
        IBitacoraGeneralRepository bitacoraRepo,
        IConfiguracionSistemaRepository configRepo,
        IPdfExtractorService pdfExtractor,
        IRegexExtractorService regexExtractor,
        IOpenAiExtractorService openAiExtractor,
        ILogger<ProcesamientoPdfService> logger)
    {
        _correoRawRepo = correoRawRepo;
        _archivoCorreoRepo = archivoCorreoRepo;
        _personaRepo = personaRepo;
        _fotoRepo = fotoRepo;
        _bitacoraRepo = bitacoraRepo;
        _configRepo = configRepo;
        _pdfExtractor = pdfExtractor;
        _regexExtractor = regexExtractor;
        _openAiExtractor = openAiExtractor;
        _logger = logger;
    }

    public async Task ProcesarPendientesAsync()
    {
        var pendientes = await _correoRawRepo.GetByEstadoAsync("pendiente");
        _logger.LogInformation("Procesando PDFs de {Total} correos pendientes", pendientes.Count);
        foreach (var correo in pendientes)
            await ProcesarCorreoAsync(correo);
    }

    public async Task ReintentarIncompletosAsync()
    {
        var maxReintentos = int.Parse(
            await _configRepo.ObtenerValorAsync("reintentos_maximos_pdf", "3"));

        var incompletos = await _correoRawRepo.GetByEstadoAsync("incompleto", maxReintentos);
        _logger.LogInformation("Reintentando {Total} correos incompletos", incompletos.Count);
        foreach (var correo in incompletos)
            await ProcesarCorreoAsync(correo);
    }

    private async Task ProcesarCorreoAsync(CorreoRaw correo)
    {
        var archivos = await _archivoCorreoRepo.GetByCorreoRawAsync(correo.IdCorreoRaw);
        var pdfs = archivos.Where(a =>
            a.TipoContenido?.Contains("pdf", StringComparison.OrdinalIgnoreCase) == true ||
            a.NombreArchivo.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)).ToList();

        if (pdfs.Count == 0)
        {
            correo.EstadoProcesamiento = "error";
            correo.MensajeError = "No se encontraron PDFs adjuntos";
            correo.FechaActualizacion = DateTime.UtcNow;
            await _correoRawRepo.UpdateAsync(correo);
            await _bitacoraRepo.RegistrarAsync("PROCESAMIENTO_PDF",
                "No se encontraron PDFs adjuntos", "error",
                idCorreoRaw: correo.IdCorreoRaw, mensajeError: correo.MensajeError);
            return;
        }

        var rutaFotos = await _configRepo.ObtenerValorAsync(
            "ruta_almacenamiento_fotos", @"C:\Datos\PUI\Fotos\");

        var hayError = false;
        var hayIncompleto = false;

        foreach (var archivo in pdfs)
        {
            var resultado = await ProcesarArchivoAsync(archivo, correo, rutaFotos);
            if (resultado == "error") hayError = true;
            if (resultado == "incompleto") hayIncompleto = true;
        }

        correo.EstadoProcesamiento = hayError && !hayIncompleto ? "error"
            : hayIncompleto ? "incompleto"
            : "completo";
        correo.Reintentos = (correo.Reintentos ?? 0) + 1;
        correo.FechaProcesamiento = DateTime.UtcNow;
        correo.FechaActualizacion = DateTime.UtcNow;
        await _correoRawRepo.UpdateAsync(correo);
    }

    private async Task<string> ProcesarArchivoAsync(
        ArchivoCorreo archivo, CorreoRaw correo, string rutaFotos)
    {
        try
        {
            if (!File.Exists(archivo.RutaDisco))
            {
                await _bitacoraRepo.RegistrarAsync("PROCESAMIENTO_PDF",
                    $"PDF no encontrado en disco: {archivo.RutaDisco}", "error",
                    idCorreoRaw: correo.IdCorreoRaw);
                return "error";
            }

            var pdfBytes = await File.ReadAllBytesAsync(archivo.RutaDisco);

            // Capa 1: extracción de texto + Regex
            var texto = await _pdfExtractor.ExtraerTextoAsync(pdfBytes);
            var ficha = _regexExtractor.Extraer(texto);

            // Capa 2: fallback OpenAI si hay campos incompletos
            if (ficha.CamposIncompletos.Count > 0)
            {
                _logger.LogInformation("Regex incompleto ({N} campos), usando OpenAI fallback",
                    ficha.CamposIncompletos.Count);
                ficha = await _openAiExtractor.ExtraerAsync(texto, ficha);
            }

            // Sin FUI o Nombre → no se puede guardar
            if (string.IsNullOrWhiteSpace(ficha.FolioUnicoIdentificacion) ||
                string.IsNullOrWhiteSpace(ficha.Nombre))
            {
                await _bitacoraRepo.RegistrarAsync("PROCESAMIENTO_PDF",
                    "No se pudo extraer FUI o Nombre del PDF", "error",
                    idCorreoRaw: correo.IdCorreoRaw,
                    mensajeError: "FolioUnicoIdentificacion o Nombre nulos");
                return "error";
            }

            // Determinar estado
            var estado = ficha.CamposIncompletos.Count == 0 ? "completo" : "incompleto";

            // Guardar persona
            var persona = new PersonaDesaparecidum
            {
                IdCorreoRaw              = correo.IdCorreoRaw,
                FolioUnicoIdentificacion = ficha.FolioUnicoIdentificacion!,
                Nombre                   = ficha.Nombre!,
                EdadActual               = ficha.EdadActual,
                EdadMomentoDesaparicion  = ficha.EdadMomentoDesaparicion,
                Sexo                     = ficha.Sexo,
                Genero                   = ficha.Genero,
                Nacionalidad             = ficha.Nacionalidad,
                LugarNacimiento          = ficha.LugarNacimiento,
                LugarHechos              = ficha.LugarHechos,
                FechaHechos              = ficha.FechaHechos,
                FechaPercate             = ficha.FechaPercate,
                CaracteristicasFisicas   = ficha.CaracteristicasFisicas,
                SenasParticulares        = ficha.SenasParticulares,
                PrendasVestir            = ficha.PrendasVestir,
                AutoridadesCompetentes   = ficha.AutoridadesCompetentes,
                CarpetaInvestigacion     = ficha.CarpetaInvestigacion,
                Idioma                   = ficha.Idioma,
                Discapacidad             = ficha.Discapacidad,
                EstadoProcesamiento      = estado,
                CamposIncompletos        = ficha.CamposIncompletos.Count > 0
                    ? string.Join(",", ficha.CamposIncompletos)
                    : null,
                FlagPublicadoFacebook    = false,
                IntentoPublicacionFacebook = 0,
                FechaCreacion            = DateTime.UtcNow,
                FechaActualizacion       = DateTime.UtcNow,
                Activo                   = true
            };
            await _personaRepo.AddAsync(persona);

            // Guardar foto si se pudo extraer
            await GuardarFotoAsync(pdfBytes, persona.IdPersonaDesaparecida,
                ficha.FolioUnicoIdentificacion!, rutaFotos);

            await _bitacoraRepo.RegistrarAsync("PROCESAMIENTO_PDF",
                $"PDF procesado: {estado}. FUI: {ficha.FolioUnicoIdentificacion}", "exitoso",
                idCorreoRaw: correo.IdCorreoRaw,
                idPersonaDesaparecida: persona.IdPersonaDesaparecida);

            _logger.LogInformation("Persona guardada: {FUI} — {Estado}",
                ficha.FolioUnicoIdentificacion, estado);

            return estado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar archivo {Ruta}", archivo.RutaDisco);
            await _bitacoraRepo.RegistrarAsync("PROCESAMIENTO_PDF",
                $"Error al procesar PDF: {archivo.NombreArchivo}", "error",
                idCorreoRaw: correo.IdCorreoRaw, mensajeError: ex.Message);
            return "error";
        }
    }

    private async Task GuardarFotoAsync(
        byte[] pdfBytes, long idPersona, string fui, string rutaBase)
    {
        try
        {
            var (fotoBytes, ext) = await _pdfExtractor.ExtraerFotoAsync(pdfBytes);
            if (fotoBytes == null || fotoBytes.Length == 0) return;

            if (!Directory.Exists(rutaBase))
                Directory.CreateDirectory(rutaBase);

            var nombreArchivo = $"{fui}{ext}";
            var rutaCompleta = Path.Combine(rutaBase, nombreArchivo);
            await File.WriteAllBytesAsync(rutaCompleta, fotoBytes);

            var foto = new FotoPersona
            {
                IdPersonaDesaparecida = idPersona,
                RutaDisco             = rutaCompleta,
                NombreArchivo         = nombreArchivo,
                TamanoBytes           = fotoBytes.Length,
                TipoContenido         = ext == ".png" ? "image/png" : "image/jpeg",
                Principal             = true,
                FechaCreacion         = DateTime.UtcNow,
                FechaActualizacion    = DateTime.UtcNow,
                Activo                = true
            };
            await _fotoRepo.AddAsync(foto);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo extraer/guardar foto para FUI: {FUI}", fui);
        }
    }
}
