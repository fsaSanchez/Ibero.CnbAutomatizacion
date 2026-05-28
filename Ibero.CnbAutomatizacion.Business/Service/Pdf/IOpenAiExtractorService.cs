using Ibero.CnbAutomatizacion.Business.Models;

namespace Ibero.CnbAutomatizacion.Business.Service.Pdf;

public interface IOpenAiExtractorService
{
    Task<FichaExtraidaDto> ExtraerAsync(string textoPdf, FichaExtraidaDto parcial);
}
