param(
    [string]$OutputDir = 'publish\production',
    [string]$ZipName = 'quiz-system-production.zip',
    [switch]$SkipZip
)

$ErrorActionPreference = 'Stop'

$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$publishDir = [System.IO.Path]::GetFullPath((Join-Path $repoRoot $OutputDir))

if (-not $publishDir.StartsWith($repoRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Output directory must stay inside the repository."
}

if (Test-Path -LiteralPath $publishDir) {
    Remove-Item -LiteralPath $publishDir -Recurse -Force
}

New-Item -ItemType Directory -Path $publishDir | Out-Null

Push-Location $repoRoot
try {
    dotnet publish API\API.csproj -c Release -o $publishDir

    if (-not $SkipZip) {
        $zipPath = [System.IO.Path]::GetFullPath((Join-Path $repoRoot "publish\$ZipName"))
        if (-not $zipPath.StartsWith($repoRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
            throw "Zip output must stay inside the repository."
        }

        if (Test-Path -LiteralPath $zipPath) {
            Remove-Item -LiteralPath $zipPath -Force
        }

        Compress-Archive -Path (Join-Path $publishDir '*') -DestinationPath $zipPath
        Write-Host "Production zip created at: $zipPath"
    }

    Write-Host "Production publish created at: $publishDir"
}
finally {
    Pop-Location
}
