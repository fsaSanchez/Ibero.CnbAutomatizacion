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
            FolioUnicoIdentificacion = Buscar(FuiRegex(), texto, "folio")?.Trim(),
            Nombre = Buscar(NombreRegex(), texto, "nombre")?.Trim(),
            Sexo = Buscar(SexoRegex(), texto, "sexo")?.Trim(),
            Genero = Buscar(GeneroRegex(), texto, "genero")?.Trim(),
            EdadActual = ParseInt(Buscar(EdadActualRegex(), texto, "edad_actual")),
            EdadMomentoDesaparicion = ParseInt(Buscar(EdadDesaparicionRegex(), texto, "edad_desaparicion")),
            Nacionalidad = Buscar(NacionalidadRegex(), texto, "nacionalidad")?.Trim(),
            LugarNacimiento = Buscar(LugarNacimientoRegex(), texto, "lugar_nacimiento")?.Trim(),
            LugarHechos = Buscar(LugarHechosRegex(), texto, "lugar_hechos")?.Trim(),
            FechaHechos = ParseFecha(Buscar(FechaHechosRegex(), texto, "fecha_hechos")),
            FechaPercate = ParseFecha(Buscar(FechaPercateRegex(), texto, "fecha_percate")),
            CaracteristicasFisicas = Buscar(CaracteristicasRegex(), texto, "caracteristicas")?.Trim(),
            SenasParticulares = Buscar(SenasRegex(), texto, "senas")?.Trim(),
            PrendasVestir = Buscar(PrendasRegex(), texto, "prendas")?.Trim(),
            AutoridadesCompetentes = Buscar(AutoridadesRegex(), texto, "autoridades")?.Trim(),
            CarpetaInvestigacion = Buscar(CarpetaRegex(), texto, "carpeta")?.Trim(),
            Idioma = Buscar(IdiomaRegex(), texto, "idioma")?.Trim(),
            Discapacidad = Buscar(DiscapacidadRegex(), texto, "discapacidad")?.Trim()
        };

        ficha.CamposIncompletos = CalcularIncompletos(ficha);
        return ficha;
    }

    private static string? Buscar(Regex regex, string texto, string campo)
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

        void Validar(string campo, string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                incompletos.Add(campo);
        }

        if (string.IsNullOrWhiteSpace(f.FolioUnicoIdentificacion)) incompletos.Add("folio_unico_identificacion");
        if (string.IsNullOrWhiteSpace(f.Nombre)) incompletos.Add("nombre");
        if (f.EdadActual == null) incompletos.Add("edad_actual");

        Validar("sexo", f.Sexo);
        Validar("lugar_hechos", f.LugarHechos);
        Validar("carpeta_investigacion", f.CarpetaInvestigacion);
        Validar("prendas_vestir", f.PrendasVestir);

        if (f.FechaHechos == null) incompletos.Add("fecha_hechos");

        return incompletos;
    }

    // ── Regex corregidos ─────────────────────────────────────────

    [GeneratedRegex(@"(?i)folio[:\s]+([A-Z0-9\-/]+)", RegexOptions.Multiline)]
    private static partial Regex FuiRegex();

    [GeneratedRegex(@"DESAPARECIDA\s+([A-ZÁÉÍÓÚÜÑ\s]{5,100})\s+Folio", RegexOptions.Singleline)]
    private static partial Regex NombreRegex();

    [GeneratedRegex(@"(?i)sexo[:\s]+([A-ZÁÉÍÓÚÜÑ]+)", RegexOptions.Multiline)]
    private static partial Regex SexoRegex();

    [GeneratedRegex(@"(?i)g[eé]nero[:\s]+([A-ZÁÉÍÓÚÜÑ]+)", RegexOptions.Multiline)]
    private static partial Regex GeneroRegex();

    [GeneratedRegex(@"(?i)edad\s+actual[:\s]+(\d+)", RegexOptions.Multiline)]
    private static partial Regex EdadActualRegex();

    [GeneratedRegex(@"(?i)edad\s+al\s+momento\s+de\s+la\s+desaparici[oó]n[:\s]+(\d+)", RegexOptions.Multiline)]
    private static partial Regex EdadDesaparicionRegex();

    [GeneratedRegex(@"(?i)nacionalidad[:\s]+([A-ZÁÉÍÓÚÜÑ]+)", RegexOptions.Multiline)]
    private static partial Regex NacionalidadRegex();

    [GeneratedRegex(@"(?i)lugar\s+de\s+nacimiento[:\s]+(.+?)(?:\n|$)", RegexOptions.Multiline)]
    private static partial Regex LugarNacimientoRegex();

    [GeneratedRegex(@"(?i)lugar\s+de\s+los\s+hechos[:\s]+(.+?)(?:\n|$)", RegexOptions.Multiline)]
    private static partial Regex LugarHechosRegex();

    [GeneratedRegex(@"(?i)fecha\s+de\s+(?:los\s+)?hechos[:\s]+(\d{1,2}/\d{1,2}/\d{4})", RegexOptions.Multiline)]
    private static partial Regex FechaHechosRegex();

    [GeneratedRegex(@"(?i)fecha\s+de\s+percato[:\s]+(\d{1,2}/\d{1,2}/\d{4})", RegexOptions.Multiline)]
    private static partial Regex FechaPercateRegex();

    [GeneratedRegex(@"(?i)caracter[ií]sticas\s+f[ií]sicas[:\s]+(.+?)(?:\n|$)", RegexOptions.Multiline)]
    private static partial Regex CaracteristicasRegex();

    [GeneratedRegex(@"(?i)se[ñn]as\s+particulares[:\s]+(.+?)(?:\n|$)", RegexOptions.Multiline)]
    private static partial Regex SenasRegex();

    [GeneratedRegex(@"(?i)prendas\s+de\s+vestir:\s*(.+)", RegexOptions.Multiline)]
    private static partial Regex PrendasRegex();

    [GeneratedRegex(@"(?i)autoridades\s+competentes[:\s]+(.+?)(?:\n|$)", RegexOptions.Multiline)]
    private static partial Regex AutoridadesRegex();

    [GeneratedRegex(@"(?i)carpeta\s+de\s+investigaci[oó]n[:\s]+(.+?)(?:\n|$)", RegexOptions.Multiline)]
    private static partial Regex CarpetaRegex();

    [GeneratedRegex(@"(?i)idioma\s+o\s+lengua\s+ind[ií]gena[:\s]+(.+?)(?:\n|$)", RegexOptions.Multiline)]
    private static partial Regex IdiomaRegex();

    [GeneratedRegex(@"(?i)discapacidad[:\s]+(.+?)(?:\n|$)", RegexOptions.Multiline)]
    private static partial Regex DiscapacidadRegex();
}