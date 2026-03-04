param(
    [Parameter(Position=0)]
    [string]$Command,

    [Parameter(Position=1)]
    [string]$Param1,

    [Parameter(Position=2)]
    [string]$Param2
)

$Project = "InclusiON.Data"
$StartupProject = "InclusiON.Api"

function Show-Help {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "   EF Core Migration Helper" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Comandos disponibles:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  .\ef migrate [name]      - Aplica migraciones (todas o hasta una especifica)"
    Write-Host "  .\ef update [name]       - Alias de migrate"
    Write-Host "  .\ef add <name>          - Crea una nueva migracion"
    Write-Host "  .\ef remove              - Elimina la ultima migracion"
    Write-Host "  .\ef list                - Lista todas las migraciones"
    Write-Host "  .\ef script [from] [to]  - Genera script SQL de migraciones"
    Write-Host "  .\ef help                - Muestra esta ayuda"
    Write-Host ""
    Write-Host "Ejemplos:" -ForegroundColor Yellow
    Write-Host "  .\ef migrate                          # Aplica todas las migraciones"
    Write-Host "  .\ef migrate InitialCreate            # Migra hasta InitialCreate"
    Write-Host "  .\ef add AddLoginMethods              # Crea nueva migracion"
    Write-Host "  .\ef list                             # Lista migraciones"
    Write-Host "  .\ef script                           # Script de todas las migraciones"
    Write-Host "  .\ef script 0 InitialCreate           # Script desde inicio hasta InitialCreate"
    Write-Host "  .\ef script InitialCreate AddUsers    # Script de InitialCreate a AddUsers"
    Write-Host ""
}

switch ($Command) {
    "migrate" {
        if ($Param1) {
            Write-Host "Migrando hasta: $Param1" -ForegroundColor Cyan
            dotnet ef database update $Param1 --project $Project --startup-project $StartupProject
        } else {
            Write-Host "Aplicando todas las migraciones pendientes..." -ForegroundColor Cyan
            dotnet ef database update --project $Project --startup-project $StartupProject
        }
    }
    "update" {
        if ($Param1) {
            Write-Host "Migrando hasta: $Param1" -ForegroundColor Cyan
            dotnet ef database update $Param1 --project $Project --startup-project $StartupProject
        } else {
            Write-Host "Aplicando todas las migraciones pendientes..." -ForegroundColor Cyan
            dotnet ef database update --project $Project --startup-project $StartupProject
        }
    }
    "add" {
        if (-not $Param1) {
            Write-Host "Error: Debes especificar el nombre de la migracion" -ForegroundColor Red
            Write-Host "Uso: .\ef add NombreMigracion" -ForegroundColor Yellow
            return
        }
        Write-Host "Creando migracion: $Param1" -ForegroundColor Cyan
        dotnet ef migrations add $Param1 --project $Project --startup-project $StartupProject
    }
    "remove" {
        Write-Host "Eliminando ultima migracion..." -ForegroundColor Cyan
        dotnet ef migrations remove --project $Project --startup-project $StartupProject
    }
    "list" {
        Write-Host "Listando migraciones..." -ForegroundColor Cyan
        dotnet ef migrations list --project $Project --startup-project $StartupProject
    }
    "script" {
        $outputFile = "migration_$(Get-Date -Format 'yyyyMMdd_HHmmss').sql"

        if ($Param1 -and $Param2) {
            Write-Host "Generando script SQL desde '$Param1' hasta '$Param2'..." -ForegroundColor Cyan
            dotnet ef migrations script $Param1 $Param2 --project $Project --startup-project $StartupProject -o $outputFile
        } elseif ($Param1) {
            Write-Host "Generando script SQL desde inicio hasta '$Param1'..." -ForegroundColor Cyan
            dotnet ef migrations script 0 $Param1 --project $Project --startup-project $StartupProject -o $outputFile
        } else {
            Write-Host "Generando script SQL de todas las migraciones..." -ForegroundColor Cyan
            dotnet ef migrations script --project $Project --startup-project $StartupProject -o $outputFile
        }

        if (Test-Path $outputFile) {
            Write-Host "Script generado: $outputFile" -ForegroundColor Green
        }
    }
    "help" {
        Show-Help
    }
    default {
        Show-Help
    }
}
