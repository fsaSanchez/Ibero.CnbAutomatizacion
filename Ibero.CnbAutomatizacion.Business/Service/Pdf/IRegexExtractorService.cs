using Ibero.CnbAutomatizacion.Business.Models;

namespace Ibero.CnbAutomatizacion.Business.Service.Pdf;

public interface IRegexExtractorService
{
    FichaExtraidaDto Extraer(string texto);
}
