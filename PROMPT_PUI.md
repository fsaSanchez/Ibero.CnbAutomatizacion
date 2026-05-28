# PROMPT — Sistema de Alertas de Personas Desaparecidas (PUI)
## Universidad Iberoamericana — Área de Sistemas

---

## Rol

Eres un arquitecto y desarrollador de software senior especializado en .NET 10, Microsoft 365 (Graph API), SQL Server y automatización de procesos backend. Trabajas para el área de Sistemas de la Universidad Iberoamericana y debes seguir estrictamente la arquitectura definida en `Arquitectura_CLAUDE.md`.

---

## Objetivo

Construir un sistema automatizado MVP listo para producción inicial que gestione alertas de personas desaparecidas recibidas vía correo institucional.

---

## Contexto del sistema

- Los correos llegan al buzón `pui@ibero.mx` (Microsoft 365)
- Cada correo contiene una **Ficha de Búsqueda de Persona Desaparecida** en formato PDF adjunto (documento oficial del gobierno mexicano)
- Los datos del PDF se extraen y almacenan en SQL Server
- Los registros se muestran en un portal web React
- Diariamente se publica una persona en Facebook de forma automática o manual

---

## Stack tecnológico

| Capa | Tecnología |
|------|-----------|
| Framework | ASP.NET Core 10 (Web API) |
| ORM | Entity Framework Core 10 |
| Base de datos | SQL Server (on-premise) |
| Mapeo de objetos | Mapster 7 |
| Validación | FluentValidation 12 |
| Scheduler | Hangfire (on-premise, IIS) |
| Correo | Microsoft Graph API |
| Extracción PDF | PdfPig + Regex / Fallback OpenAI GPT-4o-mini |
| Frontend | React |
| Red social | Meta Graph API (Facebook) |
| Documentación API | Scalar |
| Servidor | Windows Server on-premise + IIS |

> ❌ No usar AutoMapper. Usar exclusivamente Mapster.
> ❌ No usar microservicios ni arquitecturas distribuidas.
> ❌ No inventar endpoints de Meta API → verificar en documentación oficial.

---

## Arquitectura

Seguir estrictamente lo definido en `Arquitectura_CLAUDE.md`:
- 4 capas: `API`, `Business`, `Data`, `Entity`
- Flujo: `Controller → IService → IRepository → DbContext`
- Soft delete obligatorio (`Activo = false`)
- Timestamps UTC en el servicio
- `AsNoTracking()` en todas las lecturas
- `CommonResponse` como wrapper de respuesta

---

## Base de datos

- El `DbContext` ya está scaffoldeado desde SQL Server
- Seguir convenciones DBA: snake_case, minúsculas, español, sin acentos
- Esquemas: `operacion`, `bitacoras`, `catalogos`

### Tablas principales

| Tabla | Esquema | Descripción |
|-------|---------|-------------|
| `correo_raw` | operacion | Correos sin procesar |
| `archivo_correo` | operacion | PDFs descargados |
| `persona_desaparecida` | operacion | Datos procesados de la ficha |
| `foto_persona` | operacion | Referencia a fotos en disco |
| `bitacora_publicacion` | bitacoras | Log de publicaciones Facebook |
| `bitacora_general` | bitacoras | Log de todas las acciones |
| `configuracion_sistema` | catalogos | Configuraciones editables |

---

## Módulos a construir

### 1. INGESTA DE CORREOS
**Trigger:** Hangfire job cada 5-10 minutos (configurable en `configuracion_sistema`)

**Flujo:**
1. Autenticarse con Microsoft Graph API usando `ClientId`, `TenantId`, `ClientSecret`
2. Leer correos no procesados de `pui@ibero.mx`
3. Verificar si el correo ya existe en `correo_raw` por `id_externo` (deduplicación)
4. Guardar correo en `correo_raw` con `estado_procesamiento = pendiente`
5. Detectar adjuntos PDF
6. Descargar PDF en Base64 → decodificar → guardar en disco (`ruta_almacenamiento_pdfs`)
7. Guardar referencia en `archivo_correo`
8. Registrar en `bitacora_general`
9. Marcar correo como leído en Outlook

**Configuración en appsettings.json:**
```json
"MicrosoftGraph": {
  "TenantId": "",
  "ClientId": "",
  "ClientSecret": "",
  "Buzon": "pui@ibero.mx"
}
```

---

### 2. PROCESAMIENTO DE PDF
**Trigger:** Hangfire job inmediato después de ingesta + reintento cada 4 horas para incompletos

**Flujo:**
1. Leer registros de `correo_raw` con `estado_procesamiento = pendiente`
2. Localizar PDF en disco desde `archivo_correo`
3. Extraer texto del PDF con **PdfPig**
4. Intentar extracción con **Regex** (Capa 1)
5. Si Regex falla o campos incompletos → **Fallback OpenAI** (Capa 2)
6. Guardar datos en `persona_desaparecida`
7. Descargar y guardar foto en disco (`ruta_almacenamiento_fotos`)
8. Guardar referencia en `foto_persona`
9. Marcar `estado_procesamiento = completo` o `incompleto`
10. Registrar en `bitacora_general`

**Campos a extraer (todos):**
- Folio Único de Identificación
- Nombre completo
- Edad actual / edad al momento de desaparición
- Sexo / Género
- Nacionalidad
- Lugar de nacimiento
- Lugar de los hechos
- Fecha de hechos / Fecha de percate
- Características físicas
- Señas particulares
- Prendas de vestir
- Autoridades competentes
- Carpeta de investigación
- Idioma / Discapacidad
- Foto (referencia en disco)

**Estrategia de extracción:**
```
PDF → PdfPig extrae texto
     → Regex busca campos conocidos
       ✅ Todos encontrados → estado = completo
       ❌ Algún campo faltó → OpenAI como fallback
         → OpenAI devuelve JSON estructurado
         → Guardar lo que llegó
         → Si aún incompleto → estado = incompleto, registrar campos_incompletos
```

**Configuración en appsettings.json:**
```json
"OpenAI": {
  "ApiKey": "",
  "Model": "gpt-4o-mini"
}
```

**Manejo de errores:**
- Si PDF no se puede leer → `estado_procesamiento = error`, registrar en `bitacora_general`
- Si llega incompleto → guardar lo que llegó, marcar `estado_procesamiento = incompleto`
- Reintentos automáticos cada 4 horas para registros incompletos (máximo configurable)

---

### 3. API REST
**Base URL:** `/api`

#### Endpoints públicos (sin autenticación)
```
GET  /api/personas                  → Listar personas activas con filtros
GET  /api/personas/{id}             → Detalle de persona
```

#### Endpoints admin (con autenticación JWT)
```
DELETE /api/personas/{id}           → Borrado lógico
POST   /api/personas/{id}/publicar  → Publicar manualmente en Facebook
GET    /api/configuracion           → Obtener configuraciones del sistema
PUT    /api/configuracion/{clave}   → Actualizar configuración
GET    /api/bitacora/publicaciones  → Log de publicaciones Facebook
GET    /api/bitacora/general        → Log general del sistema
```

#### Filtros en GET /api/personas
```
?folio=               → Filtrar por FUI
?nombre=              → Filtrar por nombre (búsqueda parcial)
?estado=              → Filtrar por estado (completo, incompleto)
?fechaHechos=         → Filtrar por fecha de hechos
?busqueda=            → Búsqueda libre en todos los campos
?pagina=              → Paginación
?tamanioPagina=       → Registros por página
```

---

### 4. PUBLICACIÓN EN FACEBOOK
**Trigger:** Hangfire job diario a hora configurable desde `configuracion_sistema`

**Flujo automático:**
1. Verificar si ya se publicó hoy (`bitacora_publicacion`)
2. Si ya se publicó manualmente → omitir publicación automática
3. Seleccionar 1 persona al azar de las llegadas ese día con `flag_publicado_facebook = false`
4. Generar texto automático con datos básicos de la persona
5. Publicar foto + texto en Facebook Page vía Meta Graph API
6. Marcar `flag_publicado_facebook = true` en `persona_desaparecida`
7. Registrar en `bitacora_publicacion` (tipo: automatica)

**Flujo manual (desde admin):**
1. Admin presiona "Publicar en Facebook" en el portal
2. Llama a `POST /api/personas/{id}/publicar`
3. Si ya fue publicada → retornar error indicándolo
4. Publicar y registrar en `bitacora_publicacion` (tipo: manual)
5. La publicación automática del día ya no ejecuta si hubo publicación manual

**Texto generado automáticamente:**
```
🔴 ALERTA DE PERSONA DESAPARECIDA

Nombre: {nombre}
Edad: {edad_actual} años
Lugar: {lugar_hechos}
Fecha: {fecha_hechos}
Carpeta: {carpeta_investigacion}

{informacion_contacto}  ← Desde configuracion_sistema
```

**Configuración en appsettings.json:**
```json
"Facebook": {
  "PageId": "",
  "AccessToken": ""
}
```

**Manejo de errores:**
- Si falla la publicación → `estado_publicacion = fallida` en `bitacora_publicacion`
- Reintentos automáticos (máximo configurable en `configuracion_sistema`)
- Alerta visual en portal admin si falla

---

### 5. PORTAL WEB REACT

#### Vista pública
- Listado de personas desaparecidas
- Filtros: FUI, nombre, estado, fecha de hechos, búsqueda libre
- Tarjeta por persona: foto, nombre, edad, lugar
- Botón "Ver más" → detalle completo
- Mensaje de contacto desde `configuracion_sistema`

#### Vista administrador (con login)
- Misma vista pública + indicador visual de estado (completo/incompleto)
- Botón "Eliminar" → borrado lógico (con confirmación)
- Botón "Publicar en Facebook" → llama al endpoint manual
- Indicador si ya fue publicada
- Sección de configuración: editar hora de publicación, mensaje de contacto
- Sección de bitácora: log de publicaciones y acciones

---

### 6. HANGFIRE (Scheduler)

**Jobs registrados:**
| Job | Frecuencia | Descripción |
|-----|-----------|-------------|
| `IngestaCorreosJob` | Cada X minutos (configurable) | Revisa nuevos correos |
| `ProcesamientoPdfJob` | Inmediato tras ingesta | Procesa PDFs pendientes |
| `ReintentoDatosIncompletosJob` | Cada X horas (configurable) | Reintenta incompletos |
| `PublicacionFacebookJob` | Diario a hora configurable | Publica en Facebook |

**Configuración:**
- Dashboard Hangfire disponible en `/hangfire` (solo admin)
- Usar SQL Server como storage de Hangfire
- Reintentos automáticos en caso de falla

---

## Almacenamiento en disco

```
C:\Datos\PUI\
  PDFs\     ← Archivos PDF descargados de correos
  Fotos\    ← Fotos extraídas de fichas
```

Nombre de archivos: `{folio_unico_identificacion}.pdf` / `{folio_unico_identificacion}.jpg`

---

## Configuraciones del sistema (editables desde admin)

| Clave | Valor default | Descripción |
|-------|--------------|-------------|
| `informacion_contacto` | Mensaje oficial CNB | Texto de contacto para publicaciones |
| `hora_publicacion_facebook` | `08:00:00` | Hora diaria de publicación |
| `habilitar_publicacion_automatica` | `true` | Habilitar/deshabilitar automatismo |
| `ruta_almacenamiento_fotos` | `C:\Datos\PUI\Fotos\` | Ruta de fotos |
| `ruta_almacenamiento_pdfs` | `C:\Datos\PUI\PDFs\` | Ruta de PDFs |
| `reintentos_maximos_publicacion` | `3` | Reintentos Facebook |
| `intervalo_revision_correos_minutos` | `5` | Frecuencia de ingesta |
| `intervalo_reintento_datos_incompletos_horas` | `4` | Frecuencia de reintentos |

---

## Riesgos y mitigaciones

### Riesgo 1: Extracción incorrecta del PDF
- PDFs con variaciones de formato o campos en distinto orden
- **Mitigación:** Regex como primera capa + OpenAI como fallback + estado `incompleto` visible en admin

### Riesgo 2: Fallo en publicación Facebook
- Token expirado, API caída, límites de rate
- **Mitigación:** Reintentos automáticos + log detallado + alerta visual en admin

### Riesgo 3: Correos duplicados o reprocesamiento
- El mismo correo procesado más de una vez
- **Mitigación:** Campo único `id_externo` en `correo_raw` + verificación antes de insertar

---

## Restricciones

- ❌ No usar soluciones exclusivas de Azure sin alternativa on-premise
- ❌ No diseñar microservicios
- ❌ No usar AutoMapper (usar Mapster)
- ❌ No inventar endpoints de Meta API
- ❌ No eliminar registros físicamente (siempre soft delete)
- ✅ Seguir arquitectura de `Arquitectura_CLAUDE.md` en todo momento
- ✅ Seguir convenciones DBA en cualquier consulta o migración
- ✅ Responder y comentar código en español
