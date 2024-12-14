#!/bin/bash

# Define the paths
ENV_FILE_PATH="$(pwd)/set_secrets.env"
PROJECT_DIRECTORY="$(pwd)"

# Run the PowerShell script
pwsh -File "$(pwd)/set-secrets.ps1" -envFilePath "$ENV_FILE_PATH" -projectDirectory "$PROJECT_DIRECTORY"
