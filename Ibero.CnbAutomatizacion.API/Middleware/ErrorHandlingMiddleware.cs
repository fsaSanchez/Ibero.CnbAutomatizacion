using Ibero.CnbAutomatizacion.Entity.Response.Result;
using System.Net;
using System.Text.Json;

namespace Ibero.CnbAutomatizacion.API.Middleware;

public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    public async Task Invoke(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error no controlado");
            ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            ctx.Response.ContentType = "application/json";
            var response = new CommonResponse
            {
                Success = false,
                Code = 500,
                Message = "Ocurrió un error interno. Intente más tarde."
            };
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
