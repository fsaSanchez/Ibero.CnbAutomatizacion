using Ibero.CnbAutomatizacion.Entity.Response.Pdf;

namespace Ibero.CnbAutomatizacion.Business.Service.Pdf;

public interface IPdfExtractorService
{
    Task<string> ExtraerTextoAsync(byte[] pdfBytes);
    Task<(byte[]? Bytes, string Extension)> ExtraerFotoAsync(byte[] pdfBytes);
    Task<List<ImagenPdfPreviewResponse>> ListarImagenesAsync(byte[] pdfBytes);
}
