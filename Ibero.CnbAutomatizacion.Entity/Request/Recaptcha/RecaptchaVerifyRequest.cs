namespace Ibero.CnbAutomatizacion.Entity.Request.Recaptcha;

public class RecaptchaVerifyRequest
{
    public string Token { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
}
