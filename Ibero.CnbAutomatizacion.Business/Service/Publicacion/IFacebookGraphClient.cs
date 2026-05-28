namespace Ibero.CnbAutomatizacion.Business.Service.Publicacion;

public interface IFacebookGraphClient
{
    Task<string?> PublicarFotoAsync(string pageId, string accessToken, byte[] fotoBytes, string caption);
    Task<string?> PublicarTextoAsync(string pageId, string accessToken, string message);
}
