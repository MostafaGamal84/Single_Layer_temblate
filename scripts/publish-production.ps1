param(
    [string]$OutputDir = "artifacts\\publish-production"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($root)) {
    $root = (Get-Location).Path
}

Write-Host "Project root: $root"

Push-Location $root
try {
    Write-Host "1) Building Angular production bundle..."
    Push-Location (Join-Path $root "front")
    try {
        cmd /c "npm.cmd run build"
    }
    finally {
        Pop-Location
    }

    $browserDist = Join-Path $root "front\\dist\\front\\browser"
    if (!(Test-Path $browserDist)) {
        throw "Angular production output not found at: $browserDist"
    }

    $webRoot = Join-Path $root "API\\wwwroot"
    if (!(Test-Path $webRoot)) {
        New-Item -ItemType Directory -Path $webRoot | Out-Null
    }

    Write-Host "2) Copying Angular files into API/wwwroot (preserving uploads)..."
    Get-ChildItem $webRoot -Force |
        Where-Object { $_.Name -ne "uploads" } |
        Remove-Item -Recurse -Force

    Copy-Item (Join-Path $browserDist "*") $webRoot -Recurse -Force

    Write-Host "3) Publishing ASP.NET Core Release build..."
    $publishPath = Join-Path $root $OutputDir
    if (Test-Path $publishPath) {
        Remove-Item $publishPath -Recurse -Force
    }

    dotnet publish "API\\API.csproj" -c Release -o $publishPath

    Write-Host ""
    Write-Host "Production package ready at: $publishPath"
    Write-Host "Upload all files from this folder to your hosting root."
}
finally {
    Pop-Location
}
