using Ibero.CnbAutomatizacion.Entity.Response.Result;

namespace Ibero.CnbAutomatizacion.Business.Service.CargaPdf;

public interface ICargaPdfManualService
{
    Task<CommonResponse> CargarAsync(byte[] pdfBytes, string nombreArchivo);
}
