using FluentValidation;
using Ibero.CnbAutomatizacion.Entity.Response.Result;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Ibero.CnbAutomatizacion.API.Filters;

public class FilterValidation<T>(IValidator<T> validator) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var model = context.ActionArguments.Values.OfType<T>().FirstOrDefault();
        if (model is null)
        {
            await next();
            return;
        }

        var result = await validator.ValidateAsync(model);
        if (!result.IsValid)
        {
            var errors = result.Errors.Select(e => e.ErrorMessage).ToList();
            var response = new CommonResponse
            {
                Success = false,
                Code = 400,
                Message = errors.First(),
                Data = errors
            };
            context.Result = new OkObjectResult(response);
            return;
        }

        await next();
    }
}
