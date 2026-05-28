# CLAUDE.md — Arquitectura API Ibero (Sistemas)

Guía de arquitectura y convenciones para asistir a Claude Code en proyectos del área de Sistemas Ibero.

---

## Stack tecnológico

| Capa | Tecnología |
|------|-----------|
| Framework | ASP.NET Core 10 (Web API) |
| ORM | Entity Framework Core 10 (SQL Server) |
| Mapeo de objetos | Mapster 7 |
| Validación | FluentValidation 12 |
| Autenticación | JWT Bearer (clave simétrica) |
| Documentación API | Scalar |
| Nullable | Habilitado (`<Nullable>enable</Nullable>`) |
| Implicit usings | Habilitado |

> ❌ **AutoMapper está prohibido.** No instalar ni referenciar en ningún proyecto. Usar exclusivamente Mapster.

---

## Arquitectura en capas (N-tier)

La solución tiene **4 proyectos** separados como Class Libraries, más el proyecto Web API:

```
API/          ← Web API (presentación)
Business/     ← Lógica de negocio (servicios)
Data/         ← Acceso a datos (repositorios + DbContext)
Entity/       ← DTOs de Request y Response + wrappers
```

El flujo de dependencias es estrictamente unidireccional:

```
Controller → IService → IRepository → DbContext
```

- `Entity` no tiene referencias a ningún otro proyecto.
- `Data` solo referencia `Entity`.
- `Business` referencia `Data` y `Entity`.
- `API` referencia los cuatro.

---

## Estructura de carpetas por proyecto

### `API/` (Web API)
```
Controllers/
Filters/
  FilterValidation.cs
Middleware/
  ErrorHandlingMiddleware.cs
Validators/
  {Dominio}/
    Create{Dominio}Validator.cs
  FieldMessageValidatorUtility.cs
Program.cs
appsettings.json
appsettings.Development.json
```

### `Business/`
```
Service/
  {Dominio}s/
    I{Dominio}Service.cs
    Impl/
      {Dominio}Service.cs
MappingConfigs/
  {Dominio}Mapping.cs        ← Configuración Mapster por entidad
BaseService.cs
```

### `Data/`
```
Persistence/
  {NombreContexto}/
    {NombreContexto}Context.cs
    {NombreContexto}Context.Custom.cs
Repository/
  {Dominio}s/
    I{Dominio}Repository.cs
    Impl/
      {Dominio}Repository.cs
```

### `Entity/`
```
Request/
  {Dominio}/
    {Dominio}Request.cs
Response/
  {Dominio}/
    {Dominio}Response.cs
  Result/
    Result.cs
    CommonResponse.cs
```

---

## Convenciones de nombres

| Elemento | Convención | Ejemplo |
|----------|-----------|---------|
| Clases | PascalCase | `PersonaDesaparecidaService` |
| Interfaces | `I` + PascalCase | `IPersonaDesaparecidaService` |
| Métodos | PascalCase + Async suffix | `GetAllAsync()`, `AddAsync()` |
| Propiedades | PascalCase | `NombreCompleto`, `FechaCreacion` |
| Parámetros / variables locales | camelCase | `persona`, `request` |
| Carpetas de dominio en Business/Data | Plural | `PersonaDesaparecidas/` |
| Subcarpeta de implementaciones | `Impl/` | `PersonaDesaparecidas/Impl/` |
| DTOs de entrada | `{Dominio}Request` | `PersonaDesaparecidaRequest` |
| DTOs de salida | `{Dominio}Response` | `PersonaDesaparecidaResponse` |
| Configuración Mapster | `{Dominio}Mapping` | `PersonaDesaparecidaMapping` |
| Validadores | `Create{Dominio}Validator` | `CreatePersonaDesaparecidaValidator` |
| Controllers | `{Dominio}Controller` | `PersonaDesaparecidaController` |
| Namespaces | Refleja la jerarquía de carpetas | `Business.Service.PersonaDesaparecidas.Impl` |

---

## Patrones de implementación

### Controller

```csharp
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class {Dominio}Controller : ControllerBase
{
    private readonly I{Dominio}Service _service;

    public {Dominio}Controller(I{Dominio}Service service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] {Dominio}Request request)
    {
        var result = await _service.AddAsync(request);
        return Created(string.Empty, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] {Dominio}Request request)
    {
        var result = await _service.UpdateAsync(id, request);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await _service.DeleteAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
```

- Usar `[AllowAnonymous]` solo en endpoints que no requieran autenticación.
- Validación de DTOs: aplicar `[ServiceFilter(typeof(FilterValidation<{Dominio}Request>))]` cuando exista validador registrado.

### Service

```csharp
public class {Dominio}Service : BaseService, I{Dominio}Service
{
    private readonly I{Dominio}Repository _repository;

    public {Dominio}Service(I{Dominio}Repository repository)
    {
        _repository = repository;
    }

    public async Task<CommonResponse> GetAllAsync()
    {
        var response = CreateResponseOk();
        response.Data = (await _repository.GetAllAsync()).Adapt<List<{Dominio}Response>>();
        return response;
    }

    public async Task<CommonResponse> GetByIdAsync(long id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return CreateResponseFail("Registro no encontrado");
        var response = CreateResponseOk();
        response.Data = entity.Adapt<{Dominio}Response>();
        return response;
    }

    public async Task<CommonResponse> AddAsync({Dominio}Request request)
    {
        var entity = request.Adapt<{Entidad}>();
        entity.FechaCreacion = DateTime.UtcNow;
        entity.FechaActualizacion = DateTime.UtcNow;
        await _repository.AddAsync(entity);
        var response = CreateResponseOk();
        response.Data = entity.Adapt<{Dominio}Response>();
        return response;
    }

    public async Task<CommonResponse> UpdateAsync(long id, {Dominio}Request request)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return CreateResponseFail("Registro no encontrado");
        request.Adapt(entity);
        entity.FechaActualizacion = DateTime.UtcNow;
        await _repository.UpdateAsync(entity);
        var response = CreateResponseOk();
        response.Data = entity.Adapt<{Dominio}Response>();
        return response;
    }

    public async Task<CommonResponse> DeleteAsync(long id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return CreateResponseFail("Registro no encontrado");
        await _repository.DeleteAsync(id);
        return CreateResponseOk();
    }
}
```

> ✅ **Mapster:** Usar `.Adapt<T>()` para mapeos simples. Configuraciones complejas en `MappingConfigs/{Dominio}Mapping.cs`.

### Configuración Mapster (solo cuando se necesita mapeo personalizado)

```csharp
public static class {Dominio}Mapping
{
    public static void Register(TypeAdapterConfig config)
    {
        config.NewConfig<{Entidad}, {Dominio}Response>()
            .Map(dest => dest.Campo, src => src.NavProperty.Nombre);

        config.NewConfig<{Dominio}Request, {Entidad}>();
    }
}
```

Registro global en `Program.cs`:

```csharp
var config = TypeAdapterConfig.GlobalSettings;
config.Scan(Assembly.GetAssembly(typeof({Dominio}Mapping))!);
```

### Repository

```csharp
public class {Dominio}Repository : I{Dominio}Repository
{
    private readonly {NombreContexto}Context _context;

    public {Dominio}Repository({NombreContexto}Context context) => _context = context;

    public async Task<List<{Entidad}>> GetAllAsync()
        => await _context.{Entidades}
            .Where(x => x.Activo == true)
            .AsNoTracking()
            .ToListAsync();

    public async Task<{Entidad}?> GetByIdAsync(long id)
        => await _context.{Entidades}
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task AddAsync({Entidad} entity)
    {
        _context.{Entidades}.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync({Entidad} entity)
    {
        _context.{Entidades}.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _context.{Entidades}.FindAsync(id);
        if (entity != null)
        {
            entity.Activo = false;
            _context.{Entidades}.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
```

---

## Wrapper de respuesta

**Todos los servicios retornan `CommonResponse`.** Los controllers devuelven `IActionResult`.

```csharp
public class CommonResponse
{
    public bool Success { get; set; }
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
}
```

Métodos helper en `BaseService`:

```csharp
CreateResponseOk(message: "Operación exitosa", code: 200, data: null)
CreateResponseFail(message: "...", code: 400)
```

Formato JSON que recibe el cliente:

```json
{
  "success": true,
  "code": 200,
  "message": "Operación exitosa",
  "data": {}
}
```

---

## Reglas de negocio

- **Soft delete obligatorio**: nunca eliminar físicamente. Marcar `Activo = false`.
- **AsNoTracking en lecturas**: todas las consultas GET usan `.AsNoTracking()`.
- **Timestamps UTC**: `FechaCreacion` y `FechaActualizacion` se asignan en el servicio con `DateTime.UtcNow`.
- **Entidades keyless**: vistas de BD se mapean con `HasNoKey()` y `ToView(null)` en el contexto custom.
- **No exponer entidades EF directamente**: siempre usar DTOs de respuesta.

---

## Registro de dependencias (Program.cs)

Siempre usar `AddScoped`. Orden estándar:

1. DbContext
2. Repositorios e interfaces por dominio (en pares)
3. Servicios e interfaces por dominio (en pares)
4. Mapster (`TypeAdapterConfig.GlobalSettings.Scan(...)`)
5. FluentValidation (`AddValidatorsFromAssemblyContaining<Program>`)
6. `FilterValidation<>` genérico
7. CORS, Autenticación JWT, Controllers, OpenAPI/Scalar

```csharp
builder.Services.AddScoped<I{Dominio}Repository, {Dominio}Repository>();
builder.Services.AddScoped<I{Dominio}Service, {Dominio}Service>();
```

---

## Middleware y filtros

### ErrorHandlingMiddleware

- Se registra primero en el pipeline.
- Captura excepciones no manejadas.
- Retorna HTTP 500 con `CommonResponse { Success=false, Code=500 }`.
- Crea scope de DI explícito para servicios Scoped.

### FilterValidation\<T\>

- Implementa `IAsyncActionFilter`.
- Retorna HTTP 200 con `Success=false` cuando la validación falla.
- No cambiar este comportamiento sin consenso del área.

---

## Autenticación

- JWT Bearer con clave simétrica ASCII desde `appsettings.json → AppConfiguration:Apikey`.
- `ValidateIssuer = false`, `ValidateAudience = false`.
- `ClockSkew = TimeSpan.Zero`.
- `RequireHttpsMetadata = false` (red interna).

---

## Documentación API

- Scalar en `/scalar`, spec en `/openapi/v1.json`.
- ❌ No usar `Swashbuckle`, `app.UseSwagger()` ni `app.UseSwaggerUI()`.

---

## Cómo agregar un nuevo dominio

1. **`Entity/Request/{Dominio}/{Dominio}Request.cs`**
2. **`Entity/Response/{Dominio}/{Dominio}Response.cs`**
3. **`Data/Repository/{Dominio}s/I{Dominio}Repository.cs`**
4. **`Data/Repository/{Dominio}s/Impl/{Dominio}Repository.cs`**
5. **`Business/MappingConfigs/{Dominio}Mapping.cs`** ← solo si hay mapeo personalizado
6. **`Business/Service/{Dominio}s/I{Dominio}Service.cs`**
7. **`Business/Service/{Dominio}s/Impl/{Dominio}Service.cs`**
8. **`API/Controllers/{Dominio}Controller.cs`**
9. **`Program.cs`** → registrar repositorio y servicio con `AddScoped`

Si requiere validación:

10. **`API/Validators/{Dominio}/Create{Dominio}Validator.cs`**
11. Agregar `[ServiceFilter(typeof(FilterValidation<{Dominio}Request>))]` en la acción `Create`.

---

## Checklist de revisión de código

- [ ] El controller devuelve `IActionResult` sin lógica de negocio.
- [ ] El servicio hereda de `BaseService` y usa `CreateResponseOk`/`CreateResponseFail`.
- [ ] El repositorio usa `AsNoTracking()` en todos los métodos de lectura.
- [ ] El delete es soft (`Activo = false`), nunca físico.
- [ ] Los timestamps se asignan en el servicio con `DateTime.UtcNow`.
- [ ] Se usa Mapster (`.Adapt<T>()`) en lugar de AutoMapper.
- [ ] El repositorio y servicio están registrados con `AddScoped` en `Program.cs`.
- [ ] `[Authorize]` / `[AllowAnonymous]` correctamente aplicados.
- [ ] No hay lógica de negocio ni consultas en el controller.
- [ ] No se exponen entidades EF directamente; siempre DTOs.
- [ ] ❌ No hay ninguna referencia a AutoMapper en el proyecto.
