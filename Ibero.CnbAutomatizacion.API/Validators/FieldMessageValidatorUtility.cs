namespace Ibero.CnbAutomatizacion.API.Validators;

public static class FieldMessageValidatorUtility
{
    public static string Required(string fieldName) => $"El campo '{fieldName}' es obligatorio.";
    public static string MaxLength(string fieldName, int max) => $"El campo '{fieldName}' no debe superar {max} caracteres.";
    public static string InvalidFormat(string fieldName) => $"El campo '{fieldName}' tiene un formato inválido.";
}
