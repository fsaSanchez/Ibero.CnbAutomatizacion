using System.Globalization;
using System.Text.RegularExpressions;
using Ibero.CnbAutomatizacion.Business.Models;

namespace Ibero.CnbAutomatizacion.Business.Service.Pdf.Impl;

public partial class RegexExtractorService : IRegexExtractorService
{
    public FichaExtraidaDto Extraer(string texto)
    {
        var ficha = new FichaExtraidaDto
        {
            FolioUnicoIdentificacion = Buscar(FuiRegex(), texto)?.Trim(),
            Nombre                   = Buscar(NombreRegex(), texto)?.Trim(),
            Sexo                     = Buscar(SexoRegex(), texto)?.Trim(),
            Genero                   = Buscar(GeneroRegex(), texto)?.Trim(),
            EdadActual               = ParseInt(Buscar(EdadActualRegex(), texto)),
            EdadMomentoDesaparicion  = ParseInt(Buscar(EdadDesaparicionRegex(), texto)),
            Nacionalidad             = Buscar(NacionalidadRegex(), texto)?.Trim(),
            LugarNacimiento          = Buscar(LugarNacimientoRegex(), texto)?.Trim(),
            LugarHechos              = Buscar(LugarHechosRegex(), texto)?.Trim(),
            FechaHechos              = ParseFecha(Buscar(FechaHechosRegex(), texto)),
            FechaPercate             = ParseFecha(Buscar(FechaPercateRegex(), texto)),
            CaracteristicasFisicas   = Buscar(CaracteristicasRegex(), texto)?.Trim(),
            SenasParticulares        = Buscar(SenasRegex(), texto)?.Trim(),
            PrendasVestir            = Buscar(PrendasRegex(), texto)?.Trim(),
            AutoridadesCompetentes   = Buscar(AutoridadesRegex(), texto)?.Trim(),
            CarpetaInvestigacion     = Buscar(CarpetaRegex(), texto)?.Trim(),
            Idioma                   = Buscar(IdiomaRegex(), texto)?.Trim(),
            Discapacidad             = Buscar(DiscapacidadRegex(), texto)?.Trim()
        };

        ficha.CamposIncompletos = CalcularIncompletos(ficha);
        return ficha;
    }

    private static string? Buscar(Regex regex, string texto)
    {
        var match = regex.Match(texto);
        return match.Success ? match.Groups[1].Value : null;
    }

    private static int? ParseInt(string? valor)
        => int.TryParse(valor, out var n) ? n : null;

    private static DateOnly? ParseFecha(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return null;
        foreach (var fmt in new[] { "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy" })
        {
            if (DateOnly.TryParseExact(valor.Trim(), fmt, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var fecha))
                return fecha;
        }
        return null;
    }

    private static List<string> CalcularIncompletos(FichaExtraidaDto f)
    {
        var incompletos = new List<string>();
        if (string.IsNullOrWhiteSpace(f.FolioUnicoIdentificacion)) incompletos.Add("folio_unico_identificacion");
        if (string.IsNullOrWhiteSpace(f.Nombre))                   incompletos.Add("nombre");
        if (f.EdadActual == null)                                   incompletos.Add("edad_actual");
        if (string.IsNullOrWhiteSpace(f.Sexo))                     incompletos.Add("sexo");
        if (string.IsNullOrWhiteSpace(f.LugarHechos))              incompletos.Add("lugar_hechos");
        if (f.FechaHechos == null)                                  incompletos.Add("fecha_hechos");
        if (string.IsNullOrWhiteSpace(f.CarpetaInvestigacion))     incompletos.Add("carpeta_investigacion");
        return incompletos;
    }

    // ── Patrones generados como source generators para performance ────────────

    [GeneratedRegex(@"(?i)folio\s+[uú]nico\s+de\s+identificaci[oó]n[:\s]+([A-Z0-9\-/]+)", RegexOptions.Multiline)]
    private static partial Regex FuiRegex();

    [GeneratedRegex(@"(?i)nombre[:\s]+([A-ZÁÉÍÓÚÜÑ][A-ZÁÉÍÓÚÜÑ\s]{2,60})", RegexOptions.Multiline)]
    private static partial Regex NombreRegex();

    [GeneratedRegex(@"(?i)sexo[:\s]+([A-ZÁÉÍÓÚÜÑ]+)", RegexOptions.Multiline)]
    private static partial Regex SexoRegex();

    [GeneratedRegex(@"(?i)g[eé]nero[:\s]+([A-ZÁÉÍÓÚÜÑ]+)", RegexOptions.Multiline)]
    private static partial Regex GeneroRegex();

    [GeneratedRegex(@"(?i)edad\s+actual[:\s]+(\d+)", RegexOptions.Multiline)]
    private static partial Regex EdadActualRegex();

    [GeneratedRegex(@"(?i)edad\s+al\s+momento[:\s]+(\d+)", RegexOptions.Multiline)]
    private static partial Regex EdadDesaparicionRegex();

    [GeneratedRegex(@"(?i)nacionalidad[:\s]+([A-ZÁÉÍÓÚÜÑ]+)", RegexOptions.Multiline)]
    private static partial Regex NacionalidadRegex();

    [GeneratedRegex(@"(?i)lugar\s+de\s+nacimiento[:\s]+(.+)", RegexOptions.Multiline)]
    private static partial Regex LugarNacimientoRegex();

    [GeneratedRegex(@"(?i)lugar\s+de\s+los\s+hechos[:\s]+(.+)", RegexOptions.Multiline)]
    private static partial Regex LugarHechosRegex();

    [GeneratedRegex(@"(?i)fecha\s+de\s+(?:los\s+)?hechos[:\s]+(\d{1,2}/\d{1,2}/\d{4})", RegexOptions.Multiline)]
    private static partial Regex FechaHechosRegex();

    [GeneratedRegex(@"(?i)fecha\s+de\s+percate[:\s]+(\d{1,2}/\d{1,2}/\d{4})", RegexOptions.Multiline)]
    private static partial Regex FechaPercateRegex();

    [GeneratedRegex(@"(?i)caracter[ií]sticas\s+f[ií]sicas[:\s]+(.+)", RegexOptions.Multiline)]
    private static partial Regex CaracteristicasRegex();

    [GeneratedRegex(@"(?i)se[ñn]as\s+particulares[:\s]+(.+)", RegexOptions.Multiline)]
    private static partial Regex SenasRegex();

    [GeneratedRegex(@"(?i)prendas\s+de\s+vestir[:\s]+(.+)", RegexOptions.Multiline)]
    private static partial Regex PrendasRegex();

    [GeneratedRegex(@"(?i)autoridades\s+competentes[:\s]+(.+)", RegexOptions.Multiline)]
    private static partial Regex AutoridadesRegex();

    [GeneratedRegex(@"(?i)carpeta\s+de\s+investigaci[oó]n[:\s]+(.+)", RegexOptions.Multiline)]
    private static partial Regex CarpetaRegex();

    [GeneratedRegex(@"(?i)idioma[:\s]+([A-ZÁÉÍÓÚÜÑ\s]+)", RegexOptions.Multiline)]
    private static partial Regex IdiomaRegex();

    [GeneratedRegex(@"(?i)discapacidad[:\s]+(.+)", RegexOptions.Multiline)]
    private static partial Regex DiscapacidadRegex();
}
