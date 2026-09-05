@echo off
:: Solicitar permisos de Administrador automáticamente si no los tiene
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo Solicitando permisos de Administrador...
    powershell -Command "Start-Process '%~f0' -Verb RunAs"
    exit /b
)

echo ===============================================================
echo  InclusiON - Redirigir localhost:5432 a 192.168.0.17:5433
echo ===============================================================

:: Limpiar reglas previas en el puerto 5432
netsh interface portproxy delete v4tov4 listenport=5432 listenaddress=127.0.0.1 >nul 2>&1
netsh interface portproxy delete v4tov4 listenport=5432 listenaddress=0.0.0.0 >nul 2>&1

:: Crear el reenvío de puertos para 127.0.0.1 y 0.0.0.0
netsh interface portproxy add v4tov4 listenport=5432 listenaddress=127.0.0.1 connectport=5433 connectaddress=192.168.0.17
netsh interface portproxy add v4tov4 listenport=5432 listenaddress=0.0.0.0 connectport=5433 connectaddress=192.168.0.17

echo.
echo Estado de la redireccion:
netsh interface portproxy show all

echo.
echo ===============================================================
echo  LISTO! Ahora cualquier peticion a localhost:5432 sera
echo  redirigida automaticamente al PostgreSQL en 192.168.0.17:5433
echo ===============================================================
echo.
pause
