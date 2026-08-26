using Hangfire;
using Ibero.CnbAutomatizacion.Data.Repository.ConfiguracionSistemas;

namespace Ibero.CnbAutomatizacion.API.Jobs;

/// <summary>
/// Actualiza los schedules de Hangfire con los valores de configuracion_sistema.
/// Se ejecuta en el arranque de la app y diariamente a medianoche.
/// </summary>
public class ActualizarJobsScheduleJob(
    IConfiguracionSistemaRepository configRepo,
    IRecurringJobManager jobManager,
    IConfiguration configuration)
{
    public async Task Ejecutar()
    {
        var intervaloIngesta = await ObtenerEntero(
            "intervalo_revision_correos_minutos",
            configuration.GetValue<int>("Hangfire:IngestaIntervalMinutos", 50));

        var intervaloReintento = await ObtenerEntero(
            "intervalo_reintento_datos_incompletos_horas",
            configuration.GetValue<int>("Hangfire:ReintentoDatosIncompletosHoras", 4));

        var cronPublicacion = await ObtenerCronPublicacion();

        jobManager.AddOrUpdate<IngestaCorreosJob>(
            "ingesta-correos", j => j.Ejecutar(), Cron.MinuteInterval(intervaloIngesta));

        jobManager.AddOrUpdate<ReintentoDatosIncompletosJob>(
            "reintento-datos-incompletos", j => j.Ejecutar(), Cron.HourInterval(intervaloReintento));

        jobManager.AddOrUpdate<PublicacionDiariaJob>(
            "publicacion-diaria-facebook", j => j.Ejecutar(), cronPublicacion);
    }

    private async Task<int> ObtenerEntero(string clave, int valorDefault)
    {
        var valor = await configRepo.ObtenerValorAsync(clave);
        return (valor != null && int.TryParse(valor, out var v)) ? v : valorDefault;
    }

    private async Task<string> ObtenerCronPublicacion()
    {
        var horaStr = await configRepo.ObtenerValorAsync("hora_publicacion_facebook");

        // Acepta "08:00:00" (TimeSpan) o "14" (entero)
        if (horaStr != null && TimeSpan.TryParse(horaStr, out var ts))
            return Cron.Daily(ts.Hours, ts.Minutes);

        var horaDefault = configuration.GetValue<int>("Hangfire:PublicacionHoraUtc", 14);
        return Cron.Daily(horaDefault);
    }
}
