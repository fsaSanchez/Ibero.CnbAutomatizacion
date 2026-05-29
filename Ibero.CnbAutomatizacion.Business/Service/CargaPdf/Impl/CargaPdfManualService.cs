using Ibero.CnbAutomatizacion.Business.Service.Pdf;
using Ibero.CnbAutomatizacion.Data.Persistence.CNB_Ibero;
using Ibero.CnbAutomatizacion.Data.Repository.ArchivoCorreos;
using Ibero.CnbAutomatizacion.Data.Repository.BitacoraGenerals;
using Ibero.CnbAutomatizacion.Data.Repository.ConfiguracionSistemas;
using Ibero.CnbAutomatizacion.Data.Repository.CorreoRaws;
using Ibero.CnbAutomatizacion.Data.Repository.FotoPersonas;
using Ibero.CnbAutomatizacion.Data.Repository.PersonaDesaparecidas;
using Ibero.CnbAutomatizacion.Entity.Response.Personas;
using Ibero.CnbAutomatizacion.Entity.Response.Result;
using Microsoft.Extensions.Logging;

namespace Ibero.CnbAutomatizacion.Business.Service.CargaPdf.Impl;

public class CargaPdfManualService(
    ICorreoRawRepository correoRawRepo,
    IArchivoCorreoRepository archivoCorreoRepo,
    IPersonaDesaparecidaRepository personaRepo,
    IFotoPersonaRepository fotoRepo,
    IBitacoraGeneralRepository bitacoraRepo,
    IConfiguracionSistemaRepository configRepo,
    IPdfExtractorService pdfExtractor,
    IRegexExtractorService regexExtractor,
    IOpenAiExtractorService openAiExtractor,
    ILogger<CargaPdfManualService> logger) : BaseService, ICargaPdfManualService
{
    public async Task<CommonResponse> CargarAsync(byte[] pdfBytes, string nombreArchivo)
    {
        var rutaPdfs  = await configRepo.ObtenerValorAsync("ruta_almacenamiento_pdfs",  @"C:\Datos\PUI\PDFs\");
        var rutaFotos = await configRepo.ObtenerValorAsync("ruta_almacenamiento_fotos", @"C:\Datos\PUI\Fotos\");

        // Guardar PDF en disco
        string rutaCompleta;
        try
        {
            if (!Directory.Exists(rutaPdfs)) Directory.CreateDirectory(rutaPdfs);
            var nombre = $"MANUAL_{DateTime.UtcNow:yyyyMMddHHmmss}_{Path.GetFileNameWithoutExtension(nombreArchivo).Replace(" ", "_")}.pdf";
            rutaCompleta = Path.Combine(rutaPdfs, nombre);
            await File.WriteAllBytesAsync(rutaCompleta, pdfBytes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al guardar PDF en disco: {Nombre}", nombreArchivo);
            return CreateResponseFail("No se pudo guardar el archivo en disco. Verifica los permisos de la carpeta de almacenamiento.");
        }

        // Crear CorreoRaw sintético para mantener la trazabilidad
        var correoRaw = new CorreoRaw
        {
            IdExterno           = $"MANUAL-{Guid.NewGuid()}",
            Asunto              = $"Carga manual: {nombreArchivo}",
            Remitente           = "carga-manual",
            Destinatario        = "pui@ibero.mx",
            CuerpoCorreo        = "PDF cargado manualmente desde el portal administrativo.",
            FechaRecepcion      = DateTime.UtcNow,
            EstadoProcesamiento = "pendiente",
            Reintentos          = 0,
            FechaCreacion       = DateTime.UtcNow,
            FechaActualizacion  = DateTime.UtcNow,
            Activo              = true
        };
        await correoRawRepo.AddAsync(correoRaw);

        // Crear registro en archivo_correo
        await archivoCorreoRepo.AddAsync(new ArchivoCorreo
        {
            IdCorreoRaw        = correoRaw.IdCorreoRaw,
            NombreArchivo      = Path.GetFileName(rutaCompleta),
            RutaDisco          = rutaCompleta,
            TamanoBytes        = pdfBytes.Length,
            TipoContenido      = "application/pdf",
            FechaCreacion      = DateTime.UtcNow,
            FechaActualizacion = DateTime.UtcNow,
            Activo             = true
        });

        // Extraer datos del PDF
        try
        {
            var texto = await pdfExtractor.ExtraerTextoAsync(pdfBytes);
            var ficha = regexExtractor.Extraer(texto);

            if (ficha.CamposIncompletos.Count > 0)
            {
                logger.LogInformation("Regex incompleto ({N} campos faltantes), usando OpenAI fallback", ficha.CamposIncompletos.Count);
                ficha = await openAiExtractor.ExtraerAsync(texto, ficha);
            }

            if (string.IsNullOrWhiteSpace(ficha.FolioUnicoIdentificacion) || string.IsNullOrWhiteSpace(ficha.Nombre))
            {
                await MarcarErrorAsync(correoRaw, "No se pudo extraer FUI o Nombre del PDF");
                return CreateResponseFail("No se pudo extraer el Folio Único de Identificación o el Nombre. Verifica que el archivo sea una ficha oficial de búsqueda.");
            }

            var estado = ficha.CamposIncompletos.Count == 0 ? "completo" : "incompleto";

            var persona = new PersonaDesaparecidum
            {
                IdCorreoRaw               = correoRaw.IdCorreoRaw,
                FolioUnicoIdentificacion  = ficha.FolioUnicoIdentificacion!,
                Nombre                    = ficha.Nombre!,
                EdadActual                = ficha.EdadActual,
                EdadMomentoDesaparicion   = ficha.EdadMomentoDesaparicion,
                Sexo                      = ficha.Sexo,
                Genero                    = ficha.Genero,
                Nacionalidad              = ficha.Nacionalidad,
                LugarNacimiento           = ficha.LugarNacimiento,
                LugarHechos               = ficha.LugarHechos,
                FechaHechos               = ficha.FechaHechos,
                FechaPercate              = ficha.FechaPercate,
                CaracteristicasFisicas    = ficha.CaracteristicasFisicas,
                SenasParticulares         = ficha.SenasParticulares,
                PrendasVestir             = ficha.PrendasVestir,
                AutoridadesCompetentes    = ficha.AutoridadesCompetentes,
                CarpetaInvestigacion      = ficha.CarpetaInvestigacion,
                Idioma                    = ficha.Idioma,
                Discapacidad              = ficha.Discapacidad,
                EstadoProcesamiento       = estado,
                CamposIncompletos         = ficha.CamposIncompletos.Count > 0 ? string.Join(",", ficha.CamposIncompletos) : null,
                FlagPublicadoFacebook     = false,
                IntentoPublicacionFacebook = 0,
                FechaCreacion             = DateTime.UtcNow,
                FechaActualizacion        = DateTime.UtcNow,
                Activo                    = true
            };
            await personaRepo.AddAsync(persona);

            await GuardarFotoAsync(pdfBytes, persona.IdPersonaDesaparecida, ficha.FolioUnicoIdentificacion!, rutaFotos);

            correoRaw.EstadoProcesamiento = estado;
            correoRaw.Reintentos          = 1;
            correoRaw.FechaProcesamiento  = DateTime.UtcNow;
            correoRaw.FechaActualizacion  = DateTime.UtcNow;
            await correoRawRepo.UpdateAsync(correoRaw);

            await bitacoraRepo.RegistrarAsync(
                "CARGA_PDF_MANUAL",
                $"PDF cargado manualmente ({estado}). FUI: {ficha.FolioUnicoIdentificacion}",
                "exitoso",
                idCorreoRaw: correoRaw.IdCorreoRaw,
                idPersonaDesaparecida: persona.IdPersonaDesaparecida);

            logger.LogInformation("Carga manual exitosa. FUI: {FUI} | Estado: {Estado}", ficha.FolioUnicoIdentificacion, estado);

            return CreateResponseOk(
                estado == "completo"
                    ? "PDF procesado correctamente. Todos los campos fueron extraídos."
                    : $"PDF procesado con datos incompletos. Campos faltantes: {string.Join(", ", ficha.CamposIncompletos)}.",
                data: new CargaPdfManualResponse
                {
                    IdPersonaDesaparecida    = persona.IdPersonaDesaparecida,
                    Estado                   = estado,
                    Nombre                   = persona.Nombre,
                    FolioUnicoIdentificacion = persona.FolioUnicoIdentificacion,
                    CamposIncompletos        = ficha.CamposIncompletos
                });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al procesar PDF manual: {Nombre}", nombreArchivo);
            await MarcarErrorAsync(correoRaw, ex.Message);
            return CreateResponseFail("Ocurrió un error al procesar el PDF. Asegúrate de que sea un archivo válido.");
        }
    }

    private async Task MarcarErrorAsync(CorreoRaw correoRaw, string mensajeError)
    {
        correoRaw.EstadoProcesamiento = "error";
        correoRaw.MensajeError        = mensajeError;
        correoRaw.FechaActualizacion  = DateTime.UtcNow;
        await correoRawRepo.UpdateAsync(correoRaw);
        await bitacoraRepo.RegistrarAsync(
            "CARGA_PDF_MANUAL", $"Error en carga manual: {mensajeError}", "error",
            idCorreoRaw: correoRaw.IdCorreoRaw, mensajeError: mensajeError);
    }

    private async Task GuardarFotoAsync(byte[] pdfBytes, long idPersona, string fui, string rutaBase)
    {
        try
        {
            var (fotoBytes, ext) = await pdfExtractor.ExtraerFotoAsync(pdfBytes);
            if (fotoBytes == null || fotoBytes.Length == 0) return;

            if (!Directory.Exists(rutaBase)) Directory.CreateDirectory(rutaBase);

            var nombreArchivo = $"{fui}{ext}";
            var rutaCompleta  = Path.Combine(rutaBase, nombreArchivo);
            await File.WriteAllBytesAsync(rutaCompleta, fotoBytes);

            await fotoRepo.AddAsync(new FotoPersona
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
            });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "No se pudo guardar la foto del PDF manual. FUI: {FUI}", fui);
        }
    }
}
