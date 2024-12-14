# set-secrets.ps1
param (
    [string]$envFilePath,
    [string]$projectDirectory
)

# Check if the .env file exists
if (-Not (Test-Path $envFilePath)) {
    Write-Host "The '$($envFilePath)' file does not exist at the specified path."
    exit 1
}

# Check if the project directory is a valid .NET project
if (-Not (Test-Path "$projectDirectory\*.csproj")) {
    Write-Host "The specified project directory does not contain a .csproj file."
    exit 1
}

# Check if the project directory is a valid .NET project
if (-Not (Test-Path "$projectDirectory\*.csproj")) {
    Write-Host "The specified project directory does not contain a .csproj file."
    exit 1
}

# Get the User Secrets ID from the project file
$csprojFile = Get-ChildItem -Path $projectDirectory -Filter *.csproj | Select-Object -First 1
if ($csprojFile -eq $null) {
    Write-Host "No .csproj file found in the specified directory."
    exit 1
}

[xml]$csprojContent = Get-Content $csprojFile.FullName
$userSecretsId = $csprojContent.Project.PropertyGroup.UserSecretsId
Write-Host "Type of userSecretsId: $($userSecretsId.GetType().FullName)"

# Initialize user secrets if not already initialized
if (-Not $userSecretsId -or ($userSecretsId -is [System.Object[]] -and $userSecretsId.Length -eq 0)) {
    Write-Host "User secrets are not initialized. Initializing now..."
    dotnet user-secrets init --project $projectDirectory
} else {
    Write-Host "User secrets are already initialized. $($userSecretsId)"
}

# Read the .env file
$envContent = Get-Content $envFilePath

# Loop through each line in the .env file
foreach ($line in $envContent) {
    # Skip empty lines and comments
    if ($line -match '^\s*$' -or $line -match '^\s*#') {
        continue
    }

    # Split the line into key and value
    $keyValue = $line -split '=', 2
    if ($keyValue.Length -eq 2) {
        $key = $keyValue[0].Trim()
        $value = $keyValue[1].Trim(" ", "`"")

        # Set the secret using dotnet user-secrets
        dotnet user-secrets set "$key" "$value" --project $projectDirectory
        Write-Host "Set secret: $key. Value: $value"
    } else {
        Write-Host "Invalid line in .env file: $line"
    }
}