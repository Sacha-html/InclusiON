# Genera el reporte de cobertura y lo abre en el browser.
# Uso: .\coverage.ps1

dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
reportgenerator -reports:"coverage\**\coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html
Start-Process "coverage-report\index.html"
