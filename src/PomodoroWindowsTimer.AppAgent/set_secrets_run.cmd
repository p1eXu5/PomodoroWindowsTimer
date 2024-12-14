@echo off
setlocal

:: Get the current directory as the project directory
set "PROJECT_DIRECTORY=%cd%"

:: Define the path to the .env file
set "ENV_FILE_PATH=%PROJECT_DIRECTORY%\secrets.env"  :: Assuming .env is in the current directory

:: Run the PowerShell script
powershell -ExecutionPolicy Bypass -File "%PROJECT_DIRECTORY%\set_secrets.ps1" -envFilePath "%ENV_FILE_PATH%" -projectDirectory "%PROJECT_DIRECTORY%"

endlocal
