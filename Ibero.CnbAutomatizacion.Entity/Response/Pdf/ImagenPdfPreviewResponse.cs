namespace Ibero.CnbAutomatizacion.Entity.Response.Pdf;

public class ImagenPdfPreviewResponse
{
    public int Pagina { get; set; }
    public int Indice { get; set; }
    public string TipoContenido { get; set; } = string.Empty;
    public string ImagenBase64 { get; set; } = string.Empty;
    public int TamanoBytes { get; set; }
}
