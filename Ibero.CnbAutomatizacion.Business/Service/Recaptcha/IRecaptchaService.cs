using Ibero.CnbAutomatizacion.Entity.Request.Recaptcha;
using Ibero.CnbAutomatizacion.Entity.Response.Result;

namespace Ibero.CnbAutomatizacion.Business.Service.Recaptcha;

public interface IRecaptchaService
{
    Task<CommonResponse> VerificarAsync(RecaptchaVerifyRequest request);
}
