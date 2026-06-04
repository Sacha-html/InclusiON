# InclusiON — Infraestructura Local

Instrucciones para levantar PostgreSQL con pgvector usando Docker y conectarlo a la API.

---

## Requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) instalado y corriendo
- .NET 10 SDK
- (Opcional) [DBeaver](https://dbeaver.io/) u otro cliente para inspeccionar la base de datos

---

## Setup completo desde cero

### 1. Levantar PostgreSQL

Desde esta carpeta (`Infra/`), ejecutar:

```powershell
docker compose up -d postgres
```

Postgres inicializa el volumen y ejecuta automáticamente `db-users-setup.sql`, que crea:
- Usuarios: `inclusion_dev_app`, `inclusion_uat_app`, `inclusion_prod_app`
- Bases: `inclusion_dev`, `inclusion_test`
- Extensión `vector` en ambas bases

| Parámetro | Valor |
|-----------|-------|
| Imagen | `pgvector/pgvector:pg17` |
| Contenedor | `postgres` |
| Usuario superusuario | `postgres` |
| Puerto | `5432` |

Los datos persisten en el volumen Docker `inclusion_pgdata` — sobreviven reinicios. El script de init solo corre cuando el volumen está vacío (primera vez o tras `docker compose down -v`).

---

### 2. Correr migraciones y seed

La API aplica migraciones y corre el seeder automáticamente al iniciar. No es necesario correr `dotnet ef database update` manualmente.

```bash
cd InclusiON.Server/InclusiON.Api
dotnet run
```

O desde Rider: seleccionar el launch profile `Development`.

---

## Recrear bases desde cero

Si las bases están corruptas o se quiere empezar limpio:

```powershell
# 1. Detener y eliminar contenedor + volumen
docker compose down -v

# 2. Volver a levantar (init corre automáticamente: usuarios, bases, extensión vector)
docker compose up -d postgres

# 3. Levantar la API (aplica migraciones y seed automáticamente)
cd InclusiON.Server/InclusiON.Api && dotnet run
```

---

## Ambientes y usuarios de base de datos

| Ambiente | Base de datos | Usuario de app | Contraseña |
|----------|--------------|----------------|------------|
| Development | `inclusion_dev` | `inclusion_dev_app` | `Inclusion_Dev_2025_!` |
| Testing (E2E) | `inclusion_test` | `inclusion_dev_app` | `Inclusion_Dev_2025_!` |
| UAT | `inclusion_uat` | `inclusion_uat_app` | (ver appsettings.UAT.json) |
| Production | `inclusion_prod` | `inclusion_prod_app` | (var de entorno) |

> Los usuarios de app tienen permisos DML (SELECT/INSERT/UPDATE/DELETE). Son **owners** de sus bases para que EF Core pueda ejecutar migraciones (ALTER TABLE, etc.).

---

## Stack completo (todos los servicios)

Para levantar API + frontend + agente Python además de la DB:

```powershell
docker compose up -d
```

Servicios incluidos en `docker-compose.yml`:

| Servicio | Puerto | Descripción |
|----------|--------|-------------|
| `postgres` | 5432 | PostgreSQL 17 con pgvector |
| `inclusion_api` | 5000 | Backend .NET 10 |
| `inclusion_agent` | 5001 | Agente Python (embeddings) |
| `inclusion_frontend` | 4200 | Frontend Angular |

---

## Verificar conexión

```powershell
# Conectarse a la base de desarrollo
docker exec -it postgres psql -U inclusion_dev_app -d inclusion_dev

# Listar tablas (después de correr migraciones)
\dt

# Salir
\q
```

Desde DBeaver:

| Campo | Valor |
|-------|-------|
| Host | `localhost` |
| Port | `5432` |
| Database | `inclusion_dev` |
| Username | `inclusion_dev_app` |
| Password | `Inclusion_Dev_2025_!` |

---

## Comandos útiles

```powershell
# Ver logs del contenedor
docker logs postgres

# Detener sin borrar datos
docker compose stop

# Detener y eliminar contenedor (el volumen queda)
docker compose down

# Eliminar todo, incluyendo datos
docker compose down -v

# Reiniciar
docker compose restart postgres
```

---

## Migraciones EF Core

Las migraciones se crean desde CLI (desde `InclusiON.Server/`):

```bash
dotnet ef migrations add NombreMigracion --project InclusiON.Data --startup-project InclusiON.Api
dotnet ef database update --project InclusiON.Data --startup-project InclusiON.Api
```

---

## Troubleshooting

**Puerto 5432 ocupado**
```powershell
netstat -ano | findstr :5432
# Si hay un proceso PostgreSQL local corriendo:
net stop postgresql-x64-17
```

**Error `must be owner of table`**
El usuario de la app no es owner de la tabla. Ocurre si las migraciones se corrieron con un usuario distinto. Solución: recrear la base siguiendo los pasos de [Recrear bases desde cero](#recrear-bases-desde-cero).

**Error `permission denied to create extension "vector"`**
La extensión requiere superusuario. Correr el paso 5 del setup con `-U postgres`.

**Contenedor no inicia**
```powershell
docker logs postgres
```
