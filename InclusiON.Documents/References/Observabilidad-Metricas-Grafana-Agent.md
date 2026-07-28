# Observabilidad y Métricas con Grafana Agent

## Tabla de Contenidos

1. [Introducción a Observabilidad](#1-introducción-a-observabilidad)
2. [Stack de Observabilidad](#2-stack-de-observabilidad)
3. [Métricas Custom a Implementar](#3-métricas-custom-a-implementar)
4. [Configuración Paso a Paso](#4-configuración-paso-a-paso)
5. [Guía de Migración a Prometheus](#5-guía-de-migración-a-prometheus)
6. [Referencia Rápida](#6-referencia-rápida)

---

## 1. Introducción a Observabilidad

### ¿Qué es Observabilidad?

La **observabilidad** es la capacidad de entender el estado interno de un sistema a partir de sus salidas externas. En el contexto de desarrollo de software, permite responder preguntas como:

- ¿Está funcionando correctamente mi aplicación?
- ¿Hay errores? ¿Dónde y cuándo ocurren?
- ¿Cuál es el rendimiento? ¿Hay cuellos de botella?
- ¿Qué está pasando en producción?

### Los 3 Pilares de la Observabilidad

| Pilar | Descripción | Ejemplo |
|-------|-------------|---------|
| **Métricas (Metrics)** | Datos numéricos agregados | CPU usage, request count, latency |
| **Logs** | Registros de eventos discretos | "User login failed", "Connection timeout" |
| **Traces (Trazabilidad)** | Seguimiento de una petición a través del sistema | Ver cómo una solicitud atraviesa todos los servicios |

### ¿Por qué es importante?

- **Detección temprana** de problemas antes de que afecten usuarios
- **Debugging** rápido cuando algo falla
- **Toma de decisiones** basada en datos (no intuición)
- **SLA/SLO** compliance y reportes

---

## 2. Stack de Observabilidad

### Arquitectura Elegida

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                            ENTORNO DE DESARROLLO                             │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│    ┌──────────────┐         ┌───────────────┐         ┌─────────────────┐  │
│    │  InclusiON   │         │ Grafana Agent │         │  Grafana Cloud  │  │
│    │  .NET API    │         │               │         │                 │  │
│    │              │         │               │         │   Dashboards    │  │
│    │ :5000        │  scrape │ :12345        │  push   │   Explore       │  │
│    │ /metrics     │ ──────► │ /metrics      │ ──────► │   Alerts        │  │
│    │ /health      │         │               │         │                 │  │
│    └──────────────┘         └───────────────┘         └─────────────────┘  │
│                                                                              │
│                                    │                                         │
│                                    ▼                                         │
│                           ┌─────────────────┐                               │
│                           │  wlkmirko.grafana.net                          │
│                           └─────────────────┘                               │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Componentes

#### 1. InclusiON .NET API (Productor)
- Genera métricas en formato Prometheus
- Expone endpoint `/metrics`
- Incluye health checks

#### 2. Grafana Agent (Recolector)
- Hace scrape del endpoint `/metrics` cada 15 segundos
- Envía (push) las métricas a Grafana Cloud via `remote_write`
- Consumo de recursos: ~50 MB RAM, muy bajo CPU
- Configuración simple via archivo YAML

#### 3. Grafana Cloud (Visualización)
- Almacena métricas (retention 14 días en Free tier)
- Permite crear dashboards
- Explore para consultas ad-hoc
- Alerts para notificaciones

### Comparación: Grafana Agent vs Prometheus

| Característica | Grafana Agent | Prometheus |
|----------------|---------------|------------|
| RAM | 50-100 MB | 200-500 MB |
| CPU | Muy bajo | Bajo-Medio |
| UI Web | No | Sí |
| scrape (polling) | Sí | Sí |
| remote_write | Sí | Sí (v2.x+) |
| PromQL | Limitado | Completo |
| Ideal para | VPS pequeños, Edge | Servidores con recursos |

### Próximo: Migración a Prometheus (Futuro)

Cuando el sistema crezca o se tenga un VPS con más recursos, se puede migrar fácilmente a Prometheus. Ver [Sección 5](#5-guía-de-migración-a-prometheus) para más detalles.

---

## 3. Métricas Custom a Implementar

Se implementarán 4 métricas custom específicas para InclusiON:

### 3.1 `auth_login_total` - Contador de Intentos de Login

**Descripción:** Cuenta cada intento de autenticación.

**Labels:**
- `status`: `success` | `failed`
- `institution_id`: ID de la institución (ej: `1`, `2`, `default`)

**Query PromQL - Login attempts por status:**
```promql
sum by (status) (rate(auth_login_total[5m]))
```

**Query PromQL - Login attempts por institución:**
```promql
sum by (institution_id) (rate(auth_login_total[5m]))
```

**Query PromQL - Tasa de fallo (útil para alertas):**
```promql
sum(rate(auth_login_total{status="failed"}[5m])) / sum(rate(auth_login_total[5m])) * 100
```

---

### 3.2 `auth_token_generated_total` - Contador de Tokens Generados

**Descripción:** Cuenta cada token generado (access, refresh).

**Labels:**
- `token_type`: `access` | `refresh`

**Query PromQL - Tokens por tipo:**
```promql
sum by (token_type) (rate(auth_token_generated_total[5m]))
```

**Query PromQL - Ratio refresh/access tokens (indica uso de refresh tokens):**
```promql
sum(rate(auth_token_generated_total{token_type="refresh"}[5m])) / 
sum(rate(auth_token_generated_total{token_type="access"}[5m]))
```

---

### 3.3 `db_query_duration_seconds` - Duración de Consultas SQL

**Descripción:** Histograma de duración de queries a la base de datos.

**Labels:**
- `operation`: `select` | `insert` | `update` | `delete`
- `entity`: Nombre de la entidad (ej: `Users`, `Institutions`, `Surveys`)

**Query PromQL - Latencia promedio (p50):**
```promql
histogram_quantile(0.5, sum by (le, operation, entity) (rate(db_query_duration_seconds_bucket[5m])))
```

**Query PromQL - Latencia p95 (para detectar outliers):**
```promql
histogram_quantile(0.95, sum by (le, operation, entity) (rate(db_query_duration_seconds_bucket[5m])))
```

**Query PromQL - Latencia p99 (para SLA):**
```promql
histogram_quantile(0.99, sum by (le, operation, entity) (rate(db_query_duration_seconds_bucket[5m])))
```

**Query PromQL - Queries lentas (> 1 segundo):**
```promql
sum by (entity) (rate(db_query_duration_seconds_bucket{le="+Inf"}[5m])) 
> 0.001
```

---

### 3.4 `error_total` - Contador de Errores

**Descripción:** Cuenta todos los errores HTTP en la aplicación.

**Labels:**
- `error_type`: `client_error` (4xx) | `server_error` (5xx) | `unauthorized` (401) | `not_found` (404)
- `status_code`: Código HTTP específico (ej: `400`, `500`, `503`)
- `endpoint`: Ruta del endpoint (ej: `/api/auth/login`, `/api/users`)

**Query PromQL - Errores totales por tipo:**
```promql
sum by (error_type) (rate(error_total[5m]))
```

**Query PromQL - Errores 5xx por endpoint (errores de servidor):**
```promql
sum by (endpoint) (rate(error_total{error_type="server_error"}[5m]))
```

**Query PromQL - Tasa de errores global:**
```promql
sum(rate(error_total[5m])) / sum(rate(http_requests_total[5m])) * 100
```

**Query PromQL - Endpoints con más errores 4xx:**
```promql
topk(10, sum by (endpoint) (rate(error_total{error_type=~"client_error|not_found"}[5m])))
```

---

## 4. Configuración Paso a Paso

### 4.1 Obtener Credenciales de Grafana Cloud

1. **Iniciar sesión** en [grafana.com](https://grafana.com)

2. **Ir a Connections > Prometheus** o buscar en el dashboard principal

3. **Copiar las siguientes credenciales:**
   - **Remote Write URL**: `https://prometheus-us-central1.grafana.net/api/v1/push`
   - **User ID**: Visible en la misma página de configuración
   - **API Key**: 
     - Ir a Security > API Keys
     - Create new key
     - Rol: **MetricsPublisher**
     - Copiar la key (solo se muestra una vez)

### 4.2 Crear Docker Compose para Grafana Agent

**Ubicación:** `C:\Users\mirko\Code\docker\monitoring\docker-compose.yml`

```yaml
version: "3.8"

services:
  grafana-agent:
    image: grafana/agent:latest
    container_name: grafana-agent
    restart: unless-stopped
    ports:
      - "12345:12345"  # métricas del agente
    volumes:
      - ./config.yaml:/etc/agent/agent.yaml:ro
      - ./data:/data
    command: -config.file=/etc/agent/agent.yaml
```

### 4.3 Crear Configuración de Grafana Agent

**Ubicación:** `C:\Users\mirko\Code\docker\monitoring\config.yaml`

```yaml
server:
  log_level: info

metrics:
  global:
    scrape_interval: 15s
    remote_write:
      - url: https://prometheus-us-central1.grafana.net/api/v1/push
        basic_auth:
          username: TU_USER_ID_AQUI
          password: TU_API_KEY_AQUI

integrations:
  prometheus:
    enabled: true
    scrape_configs:
      - job_name: "inclusion-api"
        static_configs:
          - targets: ["host.docker.internal:5000"]
        metrics_path: /metrics
        scrape_interval: 15s
```

> **Nota:** `host.docker.internal:5000` permite al contenedor acceder al host donde corre la app .NET.

### 4.4 Configurar la App .NET

#### 4.4.1 Agregar NuGet Packages

```bash
dotnet add package OpenTelemetry.Extensions.Hosting
dotnet add package OpenTelemetry.Instrumentation.AspNetCore
dotnet add package OpenTelemetry.Instrumentation.Http
dotnet add package OpenTelemetry.Instrumentation.EntityFrameworkCore
dotnet add package OpenTelemetry.Exporter.Prometheus.AspNetCore
```

#### 4.4.2 Agregar en appsettings.json

```json
{
  "Telemetry": {
    "ServiceName": "InclusiON-API",
    "ServiceVersion": "1.0.0",
    "Enabled": true
  }
}
```

#### 4.4.3 Modificar Program.cs

```csharp
// Agregar después de var builder = WebApplication.CreateBuilder(args);

// Telemetry con OpenTelemetry + Prometheus
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddPrometheusExporter();
        
        // Métricas custom se registran en TelemetryService
    });

// Health Checks
builder.Services.AddHealthChecks();

// Singleton para TelemetryService (métricas custom)
builder.Services.AddSingleton<TelemetryService>();

var app = builder.Build();

// Agregar endpoints
app.MapPrometheusScrapingEndpoint("/metrics");
app.MapHealthChecks("/health");
```

### 4.5 Iniciar el Sistema

1. **Iniciar Grafana Agent:**
```bash
cd C:\Users\mirko\Code\docker\monitoring
docker compose up -d
```

2. **Verificar que el contenedor está corriendo:**
```bash
docker ps
```

3. **Iniciar la API .NET:**
```bash
dotnet run --project InclusiON.Api
```

4. **Verificar métricas en la app:**
- Abrir浏览器: `http://localhost:5000/metrics`
- Deberías ver las métricas en formato Prometheus

### 4.6 Verificar en Grafana Cloud

1. **Ir a** `https://wlkmirko.grafana.net/explore`

2. **Seleccionar Prometheus** como data source

3. **Ejecutar queries de prueba:**
```promql
# Métricas disponibles
{job="inclusion-api"}

# Contar todas las métricas
count({job="inclusion-api"})
```

---

## 5. Guía de Migración a Prometheus

### Cuándo Migrar

Considera migrar a Prometheus cuando:

| Señal | Indicador |
|-------|-----------|
| Memoria | Grafana Agent consume >100 MB |
| Complejidad | Necesitas UI para debugging |
| Escalabilidad | >50k series de métricas |
| Flexibilidad | Necesitas PromQL completo |

### Diferencias Clave

| Aspecto | Grafana Agent | Prometheus |
|---------|---------------|------------|
| Configuración | `config.yaml` (formato Agent) | `prometheus.yml` (formato Prometheus) |
| remote_write | Integrado | Integrado en v2.x+ |
| Storage | No almacena localmente | Almacena TSDB localmente |
| Retención | Solo en Cloud | Local + Cloud |

### Pasos de Migración

#### 1. Instalar Prometheus

**Docker Compose:**
```yaml
version: "3.8"

services:
  prometheus:
    image: prom/prometheus:latest
    container_name: prometheus
    restart: unless-stopped
    ports:
      - "9090:9090"  # UI de Prometheus
      - "9091:9091"  # remote_write endpoint
    volumes:
      - ./prometheus.yml:/etc/prometheus/prometheus.yml:ro
      - ./data:/prometheus
    command:
      - '--config.file=/etc/prometheus/prometheus.yml'
      - '--storage.tsdb.path=/prometheus'
      - '--web.enable-remote-write-receiver'
```

#### 2. Crear prometheus.yml

**Ubicación:** `C:\Users\mirko\Code\docker\monitoring\prometheus.yml`

```yaml
global:
  scrape_interval: 15s
  evaluation_interval: 15s

remote_write:
  - url: https://prometheus-us-central1.grafana.net/api/v1/push
    basic_auth:
      username: TU_USER_ID
      password: TU_API_KEY

scrape_configs:
  - job_name: "inclusion-api"
    static_configs:
      - targets: ["host.docker.internal:5000"]
    metrics_path: /metrics
    scrape_interval: 15s
```

#### 3. Cambiar target en la app .NET (opcional)

Si quieres que Prometheus haga el scrape en vez del Agent, solo necesitas:
1. Detener Grafana Agent
2. Iniciar Prometheus
3. No hay cambios en la app .NET

#### 4. Verificar en Prometheus

1. Abrir `http://localhost:9090`
2. Ir a **Status > Targets**
3. Verificar que `inclusion-api` esté **UP**

### Comandos de Referencia

```bash
# Iniciar Prometheus
docker compose -f docker-compose-prometheus.yml up -d

# Ver logs
docker logs prometheus -f

# Ver targets
curl http://localhost:9090/api/v1/targets

# Detener y limpiar
docker compose -f docker-compose-prometheus.yml down
```

---

## 6. Referencia Rápida

### Endpoints de la App

| Endpoint | Descripción |
|----------|-------------|
| `GET /metrics` | Métricas en formato Prometheus |
| `GET /health` | Health check general |
| `GET /health/ready` | Health check con PostgreSQL y SMTP |
| `GET /health/live` | Liveness probe simple |

### Comandos Docker

```bash
# Iniciar
cd C:\Users\mirko\Code\docker\monitoring
docker compose up -d

# Ver logs
docker logs grafana-agent -f

# Ver estado
docker ps

# Detener
docker compose down

# Reiniciar
docker compose restart
```

### Métricas Auto-generadas por OpenTelemetry

Además de las métricas custom, OpenTelemetry genera automáticamente:

| Métrica | Tipo | Descripción |
|---------|------|-------------|
| `http_server_duration_seconds` | Histogram | Latencia de requests HTTP |
| `http_server_request_duration_seconds` | Histogram | Duración de requests |
| `http_client_duration_seconds` | Histogram | Latencia de llamadas HTTP salientes |
| `db_client_connections_usage_seconds` | Gauge | Conexiones de BD activas |
| `process_runtime_dotnet_*` | Varios | Métricas del runtime .NET |

### Troubleshooting

#### Grafana Agent no puede alcanzar la app

```bash
# Verificar que la app está corriendo
curl http://localhost:5000/metrics

# Ver logs del agente
docker logs grafana-agent --tail=50
```

#### No veo métricas en Grafana Cloud

1. Verificar credenciales en `config.yaml`
2. Verificar que `remote_write` esté correctamente configurado
3. En Grafana Cloud, ir a **Connections > Prometheus > Status**

#### Métricas custom no aparecen

1. Verificar que `TelemetryService` esté registrado como singleton
2. Verificar que el namespace sea correcto: `InclusiON.Infrastructure.Telemetry`

### Links Útiles

- [Documentación Grafana Agent](https://grafana.com/docs/grafana-cloud/agent/)
- [Documentación Prometheus](https://prometheus.io/docs/)
- [PromQL Reference](https://prometheus.io/docs/prometheus/latest/querying/basics/)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/instrumentation/net/)

---

## Resumen de Arquitectura Final

```
┌─────────────────────────────────────────────────────────────────┐
│                     DESARROLLO LOCAL                            │
│                                                                 │
│   InclusiON .NET ──── scrape (15s) ────► Grafana Agent         │
│   :5000                                    :12345               │
│   /metrics                                 config.yaml          │
│                                               │                 │
│                                               ▼                 │
│                                       remote_write push         │
│                                               │                 │
└───────────────────────────────────────────────┼─────────────────┘
                                                │
                                                ▼
                                    ┌───────────────────────┐
                                    │   Grafana Cloud       │
                                    │                       │
                                    │   wlkmirko.grafana.net│
                                    │                       │
                                    │   - Dashboards        │
                                    │   - Explore           │
                                    │   - Alerts            │
                                    └───────────────────────┘
```

---

*Documento creado: Configuración de Observabilidad para InclusiON*
*Stack: .NET 10, OpenTelemetry, Grafana Agent, Grafana Cloud*
