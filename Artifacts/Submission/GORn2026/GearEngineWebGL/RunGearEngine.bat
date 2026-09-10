@echo off
where py >nul 2>nul
if %errorlevel% equ 0 (
  py -3 ServeGearEngine.py
  exit /b %errorlevel%
)

where python >nul 2>nul
if %errorlevel% equ 0 (
  python ServeGearEngine.py
  exit /b %errorlevel%
)

echo Python 3 is required to start the local Gear Engine WebGL server.
echo Install Python 3 or serve this folder with another static HTTP server.
pause
exit /b 1
