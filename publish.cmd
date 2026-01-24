@echo off
setlocal enabledelayedexpansion

:: Build script for AddPdfEnvelope - creates self-contained single-file executables
:: for Windows, macOS, and Linux (both x64 and ARM64)

set PUBLISH_DIR=publish
set PROJECT_NAME=AddPdfEnvelope
set PROJECT_FILE=%PROJECT_NAME%.csproj

:: Runtime identifiers for all target platforms
set RIDS=win-x64 win-arm64 osx-x64 osx-arm64 linux-x64 linux-arm64

:: Clean up previous publish folder
echo Cleaning up previous publish folder...
if exist "%PUBLISH_DIR%" rmdir /s /q "%PUBLISH_DIR%"
mkdir "%PUBLISH_DIR%"

:: Build and publish for each platform
for %%R in (%RIDS%) do (
    echo.
    echo ==========================================
    echo Building for %%R...
    echo ==========================================
    
    set OUTPUT_DIR=%PUBLISH_DIR%\%%R
    
    dotnet publish "%PROJECT_FILE%" -c Release -r %%R --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "!OUTPUT_DIR!"
    
    if errorlevel 1 (
        echo ERROR: Build failed for %%R
        exit /b 1
    )
    
    :: Clean up unnecessary files
    echo Cleaning up %%R output...
    if exist "!OUTPUT_DIR!\*.pdb" del /q "!OUTPUT_DIR!\*.pdb"
    if exist "!OUTPUT_DIR!\*.deps.json" del /q "!OUTPUT_DIR!\*.deps.json"
    if exist "!OUTPUT_DIR!\*.runtimeconfig.json" del /q "!OUTPUT_DIR!\*.runtimeconfig.json"
    
    :: Create ZIP file
    echo Creating ZIP for %%R...
    set ZIP_NAME=%PROJECT_NAME%-%%R.zip
    
    :: Use PowerShell to create ZIP (available on Windows 10+)
    powershell -NoProfile -Command "Compress-Archive -Path '!OUTPUT_DIR!\*' -DestinationPath '%PUBLISH_DIR%\!ZIP_NAME!' -Force"
    
    if errorlevel 1 (
        echo WARNING: Failed to create ZIP for %%R
    ) else (
        echo Created: %PUBLISH_DIR%\!ZIP_NAME!
    )
)

echo.
echo ==========================================
echo Build complete! Output in %PUBLISH_DIR%\
echo ==========================================
dir /b "%PUBLISH_DIR%\*.zip"

endlocal
