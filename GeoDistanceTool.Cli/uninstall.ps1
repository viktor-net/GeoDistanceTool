<#
.SYNOPSIS
    Полностью удаляет GeoDistanceTool из системы
#>
[CmdletBinding()]
param(
    [switch]$Force
)

$ErrorActionPreference = "Stop"

$AppName = "GeoDistanceTool"
$InstallDir = Join-Path $env:LOCALAPPDATA "Programs\$AppName"

function Write-Info  { param([string]$m) Write-Host "ℹ️  $m" -ForegroundColor Cyan }
function Write-Ok    { param([string]$m) Write-Host "✅ $m" -ForegroundColor Green }
function Write-Fail  { param([string]$m) Write-Host "❌ $m" -ForegroundColor Red }

if (-not (Test-Path $InstallDir) -and -not $Force) {
    Write-Info "$AppName не установлен. Ничего удалять не нужно."
    exit 0
}

try {
    # 1. Удаляем из PATH пользователя
    $UserPath = [Environment]::GetEnvironmentVariable("Path", "User")
    if ($UserPath -like "*$InstallDir*") {
        $Cleaned = ($UserPath -split ';' | Where-Object { $_.Trim() -ne $InstallDir }) -join ';'
        [Environment]::SetEnvironmentVariable("Path", $Cleaned, "User")
        Write-Ok "Удалено из PATH пользователя"
    }

    # Обновляем текущую сессию
    $MachinePath = [Environment]::GetEnvironmentVariable("Path", "Machine")
    $FreshUserPath = [Environment]::GetEnvironmentVariable("Path", "User")
    $Combined = "$MachinePath;$FreshUserPath" -split ';' | 
                Where-Object { $_.Trim().Length -gt 0 } | 
                Select-Object -Unique
    $env:Path = $Combined -join ';'

    # 2. Удаляем ярлык
    $Desktop = [Environment]::GetFolderPath("Desktop")
    $LnkPath = Join-Path $Desktop "$AppName.lnk"
    if (Test-Path $LnkPath) {
        Remove-Item -Path $LnkPath -Force
        Write-Ok "Удалён ярлык с рабочего стола"
    }

    # 3. Удаляем папку установки
    if (Test-Path $InstallDir) {
        Remove-Item -Path $InstallDir -Recurse -Force
        Write-Ok "Удалена папка: $InstallDir"
    }

    Write-Host "`n🗑️ $AppName полностью удалён." -ForegroundColor Green
    Write-Info "Для полной очистки закройте и откройте терминал заново."
    exit 0
}
catch {
    Write-Fail "Ошибка удаления: $($_.Exception.Message)"
    exit 1
}