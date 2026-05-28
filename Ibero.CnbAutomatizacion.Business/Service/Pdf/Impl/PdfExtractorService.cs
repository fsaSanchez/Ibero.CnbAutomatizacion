using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace Ibero.CnbAutomatizacion.Business.Service.Pdf.Impl;

public class PdfExtractorService : IPdfExtractorService
{
    public Task<string> ExtraerTextoAsync(byte[] pdfBytes)
    {
        var sb = new StringBuilder();
        using var doc = PdfDocument.Open(pdfBytes);
        foreach (var page in doc.GetPages())
        {
            foreach (var word in page.GetWords())
                sb.Append(word.Text).Append(' ');
            sb.AppendLine();
        }
        return Task.FromResult(sb.ToString());
    }

    public Task<(byte[]? Bytes, string Extension)> ExtraerFotoAsync(byte[] pdfBytes)
    {
        try
        {
            using var doc = PdfDocument.Open(pdfBytes);
            foreach (var page in doc.GetPages())
            {
                foreach (var imagen in page.GetImages())
                {
                    if (imagen.TryGetPng(out var png) && png.Length > 0)
                        return Task.FromResult<(byte[]?, string)>((png, ".png"));

                    var raw = imagen.RawBytes.ToArray();
                    if (raw.Length > 1000)
                        return Task.FromResult<(byte[]?, string)>((raw, ".jpg"));
                }
            }
        }
        catch
        {
            // Si no se puede extraer foto, se omite sin lanzar excepción
        }

        return Task.FromResult<(byte[]?, string)>((null, ".jpg"));
    }
}
