using Hangfire;
using Hangfire.SqlServer;
using Ibero.CnbAutomatizacion.API.Filters;
using Ibero.CnbAutomatizacion.API.Jobs;
using Ibero.CnbAutomatizacion.API.Middleware;
using Ibero.CnbAutomatizacion.Business;
using Ibero.CnbAutomatizacion.Business.Service.Bitacoras;
using Ibero.CnbAutomatizacion.Business.Service.Bitacoras.Impl;
using Ibero.CnbAutomatizacion.Business.Service.Configuraciones;
using Ibero.CnbAutomatizacion.Business.Service.Configuraciones.Impl;
using Ibero.CnbAutomatizacion.Business.Service.Correos;
using Ibero.CnbAutomatizacion.Business.Service.Correos.Impl;
using Ibero.CnbAutomatizacion.Business.Service.Graph;
using Ibero.CnbAutomatizacion.Business.Service.Graph.Impl;
using Ibero.CnbAutomatizacion.Business.Service.Pdf;
using Ibero.CnbAutomatizacion.Business.Service.Pdf.Impl;
using Ibero.CnbAutomatizacion.Business.Service.Personas;
using Ibero.CnbAutomatizacion.Business.Service.Personas.Impl;
using Ibero.CnbAutomatizacion.Business.Service.Procesamiento;
using Ibero.CnbAutomatizacion.Business.Service.Procesamiento.Impl;
using Ibero.CnbAutomatizacion.Business.Service.Publicacion;
using Ibero.CnbAutomatizacion.Business.Service.Publicacion.Impl;
using Ibero.CnbAutomatizacion.Data.Persistence.CNB_Ibero;
using Ibero.CnbAutomatizacion.Data.Repository.ArchivoCorreos;
using Ibero.CnbAutomatizacion.Data.Repository.ArchivoCorreos.Impl;
using Ibero.CnbAutomatizacion.Data.Repository.BitacoraGenerals;
using Ibero.CnbAutomatizacion.Data.Repository.BitacoraGenerals.Impl;
using Ibero.CnbAutomatizacion.Data.Repository.BitacoraPublicaciones;
using Ibero.CnbAutomatizacion.Data.Repository.BitacoraPublicaciones.Impl;
using Ibero.CnbAutomatizacion.Data.Repository.ConfiguracionSistemas;
using Ibero.CnbAutomatizacion.Data.Repository.ConfiguracionSistemas.Impl;
using Ibero.CnbAutomatizacion.Data.Repository.CorreoRaws;
using Ibero.CnbAutomatizacion.Data.Repository.CorreoRaws.Impl;
using Ibero.CnbAutomatizacion.Data.Repository.FotoPersonas;
using Ibero.CnbAutomatizacion.Data.Repository.FotoPersonas.Impl;
using Ibero.CnbAutomatizacion.Data.Repository.PersonaDesaparecidas;
using Ibero.CnbAutomatizacion.Data.Repository.PersonaDesaparecidas.Impl;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── DbContext ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<CNB_IberoContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Repositorios ───────────────────────────────────────────────────────────────
builder.Services.AddScoped<ICorreoRawRepository, CorreoRawRepository>();
builder.Services.AddScoped<IArchivoCorreoRepository, ArchivoCorreoRepository>();
builder.Services.AddScoped<IBitacoraGeneralRepository, BitacoraGeneralRepository>();
builder.Services.AddScoped<IBitacoraPublicacionRepository, BitacoraPublicacionRepository>();
builder.Services.AddScoped<IConfiguracionSistemaRepository, ConfiguracionSistemaRepository>();
builder.Services.AddScoped<IPersonaDesaparecidaRepository, PersonaDesaparecidaRepository>();
builder.Services.AddScoped<IFotoPersonaRepository, FotoPersonaRepository>();

// ── Servicios ─────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IGraphMailService, GraphMailService>();
builder.Services.AddScoped<ICorreoIngestaService, CorreoIngestaService>();
builder.Services.AddScoped<IPdfExtractorService, PdfExtractorService>();
builder.Services.AddScoped<IRegexExtractorService, RegexExtractorService>();
builder.Services.AddScoped<IOpenAiExtractorService, OpenAiExtractorService>();
builder.Services.AddScoped<IProcesamientoPdfService, ProcesamientoPdfService>();
builder.Services.AddScoped<IPersonaDesaparecidaService, PersonaDesaparecidaService>();
builder.Services.AddScoped<IConfiguracionSistemaService, ConfiguracionSistemaService>();
builder.Services.AddScoped<IBitacoraService, BitacoraService>();
builder.Services.AddScoped<IPublicacionFacebookService, PublicacionFacebookService>();

// ── Filtros genéricos ──────────────────────────────────────────────────────────
builder.Services.AddScoped(typeof(FilterValidation<>));

// ── Mapster ───────────────────────────────────────────────────────────────────
var mapsterConfig = Mapster.TypeAdapterConfig.GlobalSettings;
mapsterConfig.Scan(Assembly.GetAssembly(typeof(BaseService))!);
builder.Services.AddSingleton(mapsterConfig);

// ── FluentValidation ───────────────────────────────────────────────────────────
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// ── Hangfire ───────────────────────────────────────────────────────────────────
var connStr = builder.Configuration.GetConnectionString("DefaultConnection")!;
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(connStr, new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));
builder.Services.AddHangfireServer();

// ── JWT Authentication ─────────────────────────────────────────────────────────
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

// ── CORS ───────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("IberoPolicy", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// ── Controllers y OpenAPI ──────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// ── Pipeline ───────────────────────────────────────────────────────────────────
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "Ibero CNB Automatización API";
        options.Theme = ScalarTheme.Purple;
        options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();
app.UseCors("IberoPolicy");
app.UseAuthentication();
app.UseAuthorization();

// ── Hangfire Dashboard y Jobs recurrentes ──────────────────────────────────────
app.UseHangfireDashboard("/hangfire");

var intervalo = builder.Configuration.GetValue<int>("Hangfire:IngestaIntervalMinutos", 5);
RecurringJob.AddOrUpdate<IngestaCorreosJob>(
    "ingesta-correos",
    job => job.Ejecutar(),
    Cron.MinuteInterval(intervalo));

var intervaloReintento = builder.Configuration.GetValue<int>("Hangfire:ReintentoDatosIncompletosHoras", 4);
RecurringJob.AddOrUpdate<ReintentoDatosIncompletosJob>(
    "reintento-datos-incompletos",
    job => job.Ejecutar(),
    Cron.HourInterval(intervaloReintento));

app.MapControllers();

app.Run();
