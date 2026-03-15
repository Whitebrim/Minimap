@echo off
title RML Mod Build Script
setlocal EnableDelayedExpansion

:: Theme
set "R=[0m"
set "B=[1m"
set "DIM=[90m"
set "GRN=[32m"
set "RED=[31m"
set "CYN=[36m"
set "WHT=[97m"
set "BGGRN=[42m"
set "BGRED=[41m"

:: Resolve project name
for %%* in (.) do set "PROJECT=%%~n*"

:: Header
echo.
echo %DIM%-----------------------------------------------%R%
echo  %B%%WHT%RML Mod Build Script%R%  %DIM%v1.0%R%
echo %DIM%-----------------------------------------------%R%
echo.
echo  %DIM%^>%R% Project:  %CYN%%PROJECT%%R%
echo.

:: Step 1: Prepare
call :step "Preparing build directory"
if exist "build" ( rmdir /s /q "build" >nul 2>&1 )
mkdir "build" >nul 2>&1
if errorlevel 1 ( call :fail "Could not create build directory" & goto :end )
call :ok

:: Step 2: Copy sources
call :step "Copying sources"
robocopy "%PROJECT%" "build" /E /XF *.csproj *.rmod /XD bin obj >nul 2>&1
if errorlevel 8 ( call :fail "Robocopy failed" & goto :end )
call :ok

:: Step 3: Package
call :step "Packaging %PROJECT%.rmod"
if exist "%PROJECT%.rmod" ( del "%PROJECT%.rmod" >nul 2>&1 )
powershell.exe -NoProfile -Command "Add-Type -AssemblyName System.IO.Compression.FileSystem; [System.IO.Compression.ZipFile]::CreateFromDirectory('build','%PROJECT%.rmod',[System.IO.Compression.CompressionLevel]::Optimal,$false)" >nul 2>&1
if errorlevel 1 ( call :fail "Packaging failed" & goto :end )
call :ok

:: Step 4: Cleanup
call :step "Cleaning up"
rmdir /s /q "build" >nul 2>&1
call :ok

:: Footer: Success
echo.
echo %DIM%-----------------------------------------------%R%
echo  %B%%BGGRN%%WHT% BUILD SUCCESSFUL %R%
echo %DIM%-----------------------------------------------%R%
echo.
echo  %DIM%Output:%R%  %GRN%%PROJECT%.rmod%R%
echo.
goto :end

:step
<nul set /p =  %DIM%^>%R% %~1 %DIM%...%R%
exit /b

:ok
echo  %GRN%OK%R%
exit /b

:fail
echo  %RED%FAIL%R%
echo.
echo %DIM%-----------------------------------------------%R%
echo  %B%%BGRED%%WHT% BUILD FAILED %R%
echo %DIM%-----------------------------------------------%R%
echo.
echo  %RED%%~1%R%
echo.
exit /b

:end
endlocal
pause
