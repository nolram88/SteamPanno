@echo off
cls
echo =====================
echo    MACOS EXPORTER
echo =====================

set GODOT_PATH="C:\Program Files\Godot_v4.5-stable_mono_win64\Godot_v4.5-stable_mono_win64.exe"
set PROJECT_PATH=%~dp0..\src\SteamPanno
set EXPORT_PATH=%~dp0..\dist\macos-arm64

echo Exporting...

mkdir "%EXPORT_PATH%" 2> NUL
%GODOT_PATH% --verbose --headless --path "%PROJECT_PATH%" --export-release "macOS" "%EXPORT_PATH%\SteamPanno.exe"
if %errorlevel% equ 0 (
    echo Export successful!
) else (
    echo Export failed!
)

echo Copying files...

mkdir "%EXPORT_PATH%\assets\translations" 2> NUL
copy "%PROJECT_PATH%\assets\translations\*.*" "%EXPORT_PATH%\assets\translations\"
if %errorlevel% equ 0 (
    echo Copy successful!
) else (
    echo Copy failed!
)

echo Export process completed!
pause