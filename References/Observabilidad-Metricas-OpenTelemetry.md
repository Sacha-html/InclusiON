# Métricas de Observabilidad - InclusiON API

## Quick Reference - Métricas Principales

| Métrica | Tipo | Para qué sirve |
|---------|------|----------------|
| `http_server_request_duration_seconds` | Histogram | Latencia de requests (p50, p95, p99) |
| `http_server_request_duration_seconds_count` | Counter | Total de requests |
| `aspnetcore_authorization_attempts_total` | Counter | Intentos de autorización (success/failure) |
| `aspnetcore_authentication_challenges_total` | Counter | Veces que se pidió login (401) |
| `aspnetcore_identity_sign_in_check_password_attempts_total` | Counter | Login exitoso |
| `kestrel_active_connections` | Gauge | Conexiones activas |
| `kestrel_tls_handshake_duration_seconds_count` | Counter | Errores de TLS |
| `aspnetcore_memory_pool_allocated_bytes_total` | Counter | Uso de memoria |
| `http_client_request_duration_seconds` | Histogram | Verificar que métricas se envían a Grafana |

---

## Stack de Observabilidad

```
InclusiON .NET API → OTLP Push → Grafana Cloud
```

- Endpoint metrics: `/metrics` (disponible para debug)
- Health checks: `/health`, `/health/ready`

---

## Queries Útiles para Dashboard

### Requests y Latencia
```promql
-- Requests por segundo
sum(rate(http_server_request_duration_seconds_count[1m]))

-- Latencia p50
histogram_quantile(0.5, sum by (le) (rate(http_server_request_duration_seconds_bucket[5m])))

-- Latencia p95
histogram_quantile(0.95, sum by (le) (rate(http_server_request_duration_seconds_bucket[5m])))
```

### Errores
```promql
-- Errores 5xx por endpoint
sum by (http_route) (rate(http_server_request_duration_seconds_bucket{le="+Inf",http_response_status_code=~"5.."}[5m]))

-- Errores 4xx por endpoint
sum by (http_route) (rate(http_server_request_duration_seconds_bucket{le="+Inf",http_response_status_code=~"4.."}[5m]))
```

### Autenticación
```promql
-- Login exitosos
rate(aspnetcore_identity_sign_in_check_password_attempts_total{aspnetcore_identity_sign_in_result="success"}[5m])

-- Intentos de autorización fallidos
rate(aspnetcore_authorization_attempts_total{aspnetcore_authorization_result="failure"}[5m])
```

### Conexiones
```promql
-- Conexiones activas
sum(kestrel_active_connections)

-- Errores TLS handshake
sum(kestrel_tls_handshake_duration_seconds_count{error_type!=""})
```

### Verificar envío a Grafana
```promql
-- Debe mostrar 200 (éxito)
http_client_request_duration_seconds{service.name="InclusiON.Api"}
```

---

## Labels Importantes

| Label | Ejemplo | Uso |
|-------|---------|-----|
| `http_route` | `api/Auth/login`, `api/Users/me` | Filtrar por endpoint |
| `http_response_status_code` | `200`, `401`, `500` | Filtrar por código HTTP |
| `http_request_method` | `GET`, `POST`, `PUT` | Filtrar por método |
| `otel_scope_name` | `Microsoft.AspNetCore.Hosting` | Filtrar por componente |

---

## Próximos Pasos (Métricas Custom)

Se pueden agregar métricas custom para más visibilidad:

1. **auth_login_total** - Login exitosos/fallidos con institution_id
2. **db_query_duration_seconds** - Latencia de queries SQL por entidad
3. **error_total** - Errores de aplicación por tipo y endpoint
4. **business_activities_executed** - Actividades ejecutadas

---

## Referencias

- [OpenTelemetry .NET Metrics](https://opentelemetry.io/docs/languages/dotnet/metrics/)
- [Grafana Cloud Explore](https://grafana.com/docs/grafana-cloud/explore/explore-metrics/)