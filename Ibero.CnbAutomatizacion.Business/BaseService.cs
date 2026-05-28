using Ibero.CnbAutomatizacion.Entity.Response.Result;

namespace Ibero.CnbAutomatizacion.Business;

public abstract class BaseService
{
    protected CommonResponse CreateResponseOk(string message = "Operación exitosa", int code = 200, object? data = null)
        => new() { Success = true, Code = code, Message = message, Data = data };

    protected CommonResponse CreateResponseFail(string message, int code = 400)
        => new() { Success = false, Code = code, Message = message };
}
