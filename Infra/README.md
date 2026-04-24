# InclusiON — Infraestructura Local

Instrucciones para levantar PostgreSQL con Docker y conectarlo a la API de InclusiON.

---

## Requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) instalado y corriendo
- .NET 10 SDK
- (Opcional) [DBeaver](https://dbeaver.io/) u otro cliente para inspeccionar la base de datos

---

## 1. Levantar PostgreSQL

Desde esta carpeta (`Infra/`), ejecutar:

```bash
docker compose up -d
```

Esto levanta un contenedor con:

| Parámetro | Valor |
|-----------|-------|
| Imagen | `postgres:16` |
| Contenedor | `inclusion_db` |
| Base de datos | `inclusion_db` |
| Usuario | `inclusion_user` |
| Contraseña | `inclusion_pass` |
| Puerto | `5432` |

Los datos persisten en un volumen Docker (`inclusion_pgdata`) — sobreviven reinicios del contenedor.

---

## 2. Configurar la conexión en la API

La API usa EF Core con Npgsql. La connection string va en `InclusiON.Api/appsettings.json` (o preferiblemente en User Secrets para no commitear credenciales).

### appsettings.Development.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=inclusion_db;Username=inclusion_user;Password=inclusion_pass"
  }
}
```

### Con User Secrets (recomendado)

```bash
cd InclusiON.Server/InclusiON.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=inclusion_db;Username=inclusion_user;Password=inclusion_pass"
```

---

## 3. Correr la API

La API aplica las migraciones y corre el seeder automáticamente al iniciar:

```bash
cd InclusiON.Server
dotnet run --project InclusiON.Api
```

No es necesario correr `dotnet ef database update` manualmente — el `Program.cs` lo hace en startup.

---

## 4. Verificar la conexión

### Desde Docker

```bash
docker exec -it inclusion_db psql -U inclusion_user -d inclusion_db
```

```sql
-- Listar tablas creadas por EF Core
\dt

-- Salir
\q
```

### Desde DBeaver

| Campo | Valor |
|-------|-------|
| Host | `localhost` |
| Port | `5432` |
| Database | `inclusion_db` |
| Username | `inclusion_user` |
| Password | `inclusion_pass` |

---

## 5. Comandos útiles

```bash
# Ver logs del contenedor
docker logs inclusion_db

# Detener el contenedor (sin borrar datos)
docker compose stop

# Detener y eliminar el contenedor (sin borrar datos del volumen)
docker compose down

# Eliminar todo, incluyendo los datos
docker compose down -v

# Reiniciar
docker compose restart
```

---

## 6. Migraciones EF Core

Las migraciones se crean desde Package Manager Console en Visual Studio, con el proyecto `InclusiON.Data` como target:

```powershell
Add-Migration <NombreMigración> -Project InclusiON.Data -StartupProject InclusiON.Api
```

O desde CLI:

```bash
cd InclusiON.Server
dotnet ef migrations add <NombreMigración> --project InclusiON.Data --startup-project InclusiON.Api
```

---

## Troubleshooting

**Puerto 5432 ocupado**
Verificar si hay otra instancia de PostgreSQL corriendo localmente:
```bash
# Windows
netstat -ano | findstr :5432

# Detener el servicio local si existe
net stop postgresql-x64-16
```

**Contenedor no inicia**
```bash
docker logs inclusion_db
```

**La API no conecta**
- Verificar que el contenedor esté corriendo: `docker ps`
- Confirmar que la connection string en `appsettings.Development.json` o User Secrets coincide exactamente con los valores del `docker-compose.yml`
- El puerto del contenedor debe ser `5432` (host) → `5432` (container)
