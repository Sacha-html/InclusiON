# InclusiON — Infraestructura y Docker

Instrucciones para levantar la infraestructura de **PostgreSQL 17 con pgvector** usando Docker y conectarlo a la API y Frontend en cualquier entorno (local o compartido).

---

## 📋 Requisitos Previos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) instalado y en ejecución
- .NET 10 SDK (si corres el backend localmente)
- Node.js 20+ y Angular CLI (si corres el frontend localmente)
- (Opcional) [DBeaver](https://dbeaver.io/) u otro cliente SQL

---

## 🚀 Modos de Ejecución

### Modo 1: Base de Datos en Docker + Apps Locales (Recomendado para Desarrollo)
Este es el modo más rápido y habitual de desarrollo diario: Docker solo gestiona la base de datos, mientras que la API y Angular se ejecutan desde tu IDE (Rider, Visual Studio, VS Code) o terminal.

#### 1. Levantar el contenedor de Base de Datos
Desde la carpeta `InclusiON.Documents/Infra/`, ejecutar:

```powershell
docker compose up -d postgres
```

O si prefieres ejecutarlo con un comando directo de Docker:
```powershell
docker run -d --name postgres-vector -p 5432:5432 -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=inclusion_dev -v inclusion_pgdata:/var/lib/postgresql/data pgvector/pgvector:pg17
```

> **Nota sobre el script de inicialización:**  
> Al crearse el volumen por primera vez, PostgreSQL ejecuta automáticamente `InclusiON.Server/InclusiON.Data/Scripts/db-users-setup.sql`, creando:
> - Usuarios: `inclusion_dev_app`, `inclusion_uat_app`, `inclusion_prod_app`
> - Bases de datos: `inclusion_dev`, `inclusion_test`
> - Extensión `vector` habilitada para Inteligencia Artificial y búsqueda semántica.

#### 2. Configurar la Conexión en `appsettings.Development.json`
Ubica el archivo `InclusiON.Server/InclusiON.Api/appsettings.Development.json`:

* **Si Docker corre en tu misma computadora (Local):**
  ```json
  "ConnectionStrings": {
    "PostgreSqlConn": "Host=localhost;Port=5432;Database=inclusion_dev;Username=inclusion_dev_app;Password=Inclusion_Dev_2025_!"
  }
  ```
  *(O si usas el superusuario: `Username=postgres;Password=postgres`)*

* **Si Docker corre en otra máquina de la red local (por ejemplo la PC de un compañero):**
  ```json
  "ConnectionStrings": {
    "PostgreSqlConn": "Host=192.168.0.X;Port=5432;Database=inclusion_dev;Username=postgres;Password=postgres"
  }
  ```
  *(Reemplazar `192.168.0.X` por la IP de la máquina donde está corriendo Docker).*

#### 3. Iniciar Backend y Frontend
```bash
# Terminal 1 - Backend:
cd InclusiON.Server/InclusiON.Api
dotnet run

# Terminal 2 - Frontend:
cd InclusiON.Client
npm start
```
*La API aplica migraciones y seed de datos automáticamente al iniciar.*

---

### Modo 2: Stack Completo en Docker (`docker compose up`)
Levanta todos los servicios en contenedores independientes (Postgres + Backend + Frontend):

```powershell
# Desde InclusiON.Documents/Infra/:
docker compose up -d
```

Servicios incluidos en `docker-compose.yml`:

| Servicio | Contenedor | Puerto Host | Descripción |
| :--- | :--- | :--- | :--- |
| **`postgres`** | `postgres-vector` | `5432` | PostgreSQL 17 con extensión `pgvector` |
| **`api`** | `inclusion_api` | `5000` | Backend .NET 10 Web API |
| **`frontend`** | `inclusion_frontend` | `4200` | Frontend Angular servido con Nginx |

> 🧠 **Nota de Arquitectura:** El procesamiento de embeddings y búsqueda semántica por IA corre internamente en .NET mediante modelos locales ONNX (`InclusiON.Infrastructure`), eliminando la necesidad de un microservicio Python externo.

---

## 🔍 Conexión desde DBeaver / Clientes SQL

| Parámetro | Valor Local | Valor Red Compartida |
| :--- | :--- | :--- |
| **Host** | `localhost` | `192.168.0.X` (IP del equipo con Docker) |
| **Puerto** | `5432` (o `5433` si se modificó) | `5432` (o `5433`) |
| **Base de Datos** | `inclusion_dev` | `inclusion_dev` |
| **Usuario** | `inclusion_dev_app` (o `postgres`) | `postgres` |
| **Contraseña** | `Inclusion_Dev_2025_!` (o `postgres`) | `postgres` |

---

## 🛠️ Comandos de Mantenimiento

```powershell
# Ver logs del contenedor de base de datos
docker logs postgres-vector

# Reiniciar base de datos
docker compose restart postgres

# Detener contenedores manteniendo datos
docker compose stop

# Detener y eliminar contenedores (los datos se conservan en el volumen)
docker compose down

# REINICIO TOTAL (elimina la base de datos y recrea volumen desde cero)
docker compose down -v
docker compose up -d postgres
```

---

## ⚠️ Troubleshooting (Problemas Comunes)

1. **Error: Puerto 5432 ya ocupado**  
   Si tienes una instancia local de PostgreSQL instalada en Windows, puedes cambiar el puerto de Docker en `docker-compose.yml` (por ejemplo `"5433:5432"`) o detener el servicio local de Windows:
   ```powershell
   net stop postgresql-x64-17
   ```

2. **Error de conexión desde otra máquina en la red local:**
   - Asegúrate de que el firewall de Windows en la máquina host permita conexiones entrantes al puerto `5432` o `5433`.
   - Verifica que el IP de la máquina host sea estático o no haya cambiado (`ipconfig`).
