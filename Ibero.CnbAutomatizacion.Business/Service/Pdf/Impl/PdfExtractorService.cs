using Ibero.CnbAutomatizacion.Data.Repository.ConfiguracionSistemas;
using Ibero.CnbAutomatizacion.Entity.Response.Pdf;
using Microsoft.Graph.Models.ExternalConnectors;
using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace Ibero.CnbAutomatizacion.Business.Service.Pdf.Impl;

public class PdfExtractorService : IPdfExtractorService
{
    private readonly IConfiguracionSistemaRepository _configRepo;

    public PdfExtractorService(IConfiguracionSistemaRepository configRepo)
    {
        _configRepo = configRepo;

    }

    public async Task<string> ExtraerTextoAsync(byte[] pdfBytes)
    {
        var sb = new StringBuilder();
        using var doc = PdfDocument.Open(pdfBytes);
        foreach (var page in doc.GetPages())
        {
            foreach (var word in page.GetWords())
                sb.Append(word.Text).Append(' ');
            sb.AppendLine();
        }
        var textoExtraido= await Task.FromResult(sb.ToString());
        return LimpiezaErroresComunes(textoExtraido);
    }

    /// <summary>
    /// Funciona que ayuda a corregir los errores comunes dectados en el PDF en la versiion aun usada en 2026
    /// SI cambia mucho, pasarla a BD para una mejor configuracion. Ya la BD de esta aplicacion tiene una tabla de parametros.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private string LimpiezaErroresComunes(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

 
        text = text.Replace("Folio Único de Identificación", "Folio:");

     

        text = Regex.Replace(
            text,
            @"Características\s+(.*?)\s+físicas:",
            "Características físicas: $1",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        text = Regex.Replace(
            text,
            @"Señas\s+(.*?)\s+particulares:",
            "Señas particulares: $1",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        text = Regex.Replace(
            text,
            @"Prendas\s+de\s+(.*?)\s+vestir:",
            "Prendas de vestir: $1",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        text = Regex.Replace(
            text,
            @"Autoridades\s+(.*?)\s+Competentes:",
            "Autoridades Competentes: $1",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        text = Regex.Replace(
          text,
          @"Carpeta\s+de\s+(.*?)\s+investigación:",
          "Carpeta de investigación: $1",
          RegexOptions.Singleline | RegexOptions.IgnoreCase);

        text = Regex.Replace(text, @"[ \t]+", " ");

      
        text = Regex.Replace(text, @"\bDATOS\b", "\nDATOS\n");

     
        var campos = new[]
        {
        "Folio:",
        "Edad al momento de la desaparición:",
        "Edad Actual:",
        "Lugar de nacimiento:",
        "Sexo:",
        "Genero:",
        "Nacionalidad:",
        "¿Habla español?:",
        "Idioma o lengua indígena:",
        "Discapacidad:",
        "Fecha de hechos:",
        "Fecha de percato:",
        "Lugar de los hechos:",
        "Características físicas:",
        "Señas particulares:",
        "Prendas de vestir:",
        "Autoridades Competentes:",
        "Carpeta de investigación:"
    };

        foreach (var campo in campos)
        {
            text = Regex.Replace(
                text,
                $@"(?<!\n){Regex.Escape(campo)}",
                "\n" + campo,
                RegexOptions.IgnoreCase);
        }

        // ── 6. Limpieza final 
        text = Regex.Replace(text, @"\n+", "\n");
        var marcador = "La información que se visualiza";

          //El utimo campos, quitar todos lo que estra a apartir del Marcado. Es un dato que no cambia. a PDF  2026
        var index = text.IndexOf(marcador, StringComparison.OrdinalIgnoreCase);
        if (index > -1)
        {
            text = text.Substring(0, index);
        }

        return text.Trim();
    }



    public async Task<(byte[]? Bytes, string Extension)> ExtraerFotoAsync(byte[] pdfBytes)
    {
        try
        {
            var valorConfig = await _configRepo.ObtenerValorAsync("indice_foto");
            var indiceImagen = int.TryParse(valorConfig, out var indiceConfigurado) ? indiceConfigurado : 3;

            using var doc = PdfDocument.Open(pdfBytes);

            foreach (var page in doc.GetPages())
            {
                // Recolectar todas las imágenes de la página en una lista
                var imagenes = page.GetImages().ToList();

                // Validar que el índice existe
                if (indiceImagen < 0 || indiceImagen >= imagenes.Count)
                {
                    continue; // Pasar a la siguiente página si el índice no existe
                }

                var (bytes, extension) = ExtraerBytesImagen(imagenes[indiceImagen]);
                if (bytes != null)
                    return (bytes, extension);
            }
        }
        catch
        {
            // Si no se puede extraer foto, se omite sin lanzar excepción
        }

        return (null, ".jpg");
    }

    /// <summary>
    /// Lista todas las imágenes encontradas en el PDF, página por página, con el índice
    /// que le correspondería dentro de esa página. Sirve para calibrar visualmente el
    /// valor de configuración "indice_foto" cuando cambia el formato de la ficha oficial.
    /// </summary>
    public Task<List<ImagenPdfPreviewResponse>> ListarImagenesAsync(byte[] pdfBytes)
    {
        var resultado = new List<ImagenPdfPreviewResponse>();
        using var doc = PdfDocument.Open(pdfBytes);

        var numeroPagina = 0;
        foreach (var page in doc.GetPages())
        {
            numeroPagina++;
            var imagenes = page.GetImages().ToList();

            for (var indice = 0; indice < imagenes.Count; indice++)
            {
                var (bytes, extension) = ExtraerBytesImagen(imagenes[indice]);
                if (bytes == null) continue;

                resultado.Add(new ImagenPdfPreviewResponse
                {
                    Pagina = numeroPagina,
                    Indice = indice,
                    TipoContenido = extension == ".png" ? "image/png" : "image/jpeg",
                    ImagenBase64 = Convert.ToBase64String(bytes),
                    TamanoBytes = bytes.Length
                });
            }
        }

        return Task.FromResult(resultado);
    }

    private static (byte[]? Bytes, string Extension) ExtraerBytesImagen(IPdfImage imagen)
    {
        // Intentar extraer como PNG
        if (imagen.TryGetPng(out var png) && png.Length > 0)
            return (png, ".png");

        // Si no es PNG, intentar obtener los bytes crudos
        var raw = imagen.RawBytes.ToArray();
        return raw.Length > 1000 ? (raw, ".jpg") : (null, ".jpg");
    }
}
