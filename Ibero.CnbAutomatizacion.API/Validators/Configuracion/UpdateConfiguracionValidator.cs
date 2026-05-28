using FluentValidation;
using Ibero.CnbAutomatizacion.Entity.Request.Configuracion;

namespace Ibero.CnbAutomatizacion.API.Validators.Configuracion;

public class UpdateConfiguracionValidator : AbstractValidator<ConfiguracionUpdateRequest>
{
    public UpdateConfiguracionValidator()
    {
        RuleFor(x => x.Valor)
            .NotEmpty()
            .WithMessage(FieldMessageValidatorUtility.Required("valor"));
    }
}
