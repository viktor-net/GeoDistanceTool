<#
.SYNOPSIS
    Устанавливает GeoDistanceTool.exe в систему
#>
[CmdletBinding()]
param(
    [switch]$Force
)

$ErrorActionPreference = "Stop"

$AppName = "GeoDistanceTool"
$InstallDir = Join-Path $env:LOCALAPPDATA "Programs\$AppName"
$ExeName = "$AppName.exe"
$SourceExe = Join-Path $PSScriptRoot $ExeName

function Write-Info    { param([string]$m) Write-Host "ℹ️  $m" -ForegroundColor Cyan }
function Write-Ok      { param([string]$m) Write-Host "✅ $m" -ForegroundColor Green }
function Write-Fail    { param([string]$m) Write-Host "❌ $m" -ForegroundColor Red }

# Защита от запуска вставкой кода в консоль
if (-not $PSScriptRoot) {
    Write-Fail "Скрипт запущен неверно."
    Write-Info "Сохраните код в файл install.ps1 и запустите: .\install.ps1"
    exit 1
}

if (-not (Test-Path $SourceExe)) {
    Write-Fail "Не найден файл: $SourceExe"
    Write-Info "Убедитесь, что $ExeName лежит в одной папке со скриптом."
    exit 1
}

try {
    # 1. Создаём директорию
    if (-not (Test-Path $InstallDir)) {
        New-Item -ItemType Directory -Path $InstallDir -Force | Out-Null
        Write-Info "Создана папка: $InstallDir"
    }

    # 2. Копируем исполняемый файл
    $DestExe = Join-Path $InstallDir $ExeName
    Copy-Item -Path $SourceExe -Destination $DestExe -Force
    Write-Ok "Файл скопирован: $ExeName"

    # 3. Обновляем PATH пользователя
    $UserPath = [Environment]::GetEnvironmentVariable("Path", "User")
    if ($UserPath -notlike "*$InstallDir*") {
        $NewPath = if ($UserPath.Trim().Length -eq 0) { $InstallDir } else { "$UserPath;$InstallDir" }
        [Environment]::SetEnvironmentVariable("Path", $NewPath, "User")
        Write-Ok "Добавлено в системный PATH (HKCU)"
    } else {
        Write-Info "Путь уже присутствует в PATH"
    }

    # 4. Мгновенно обновляем PATH в текущей сессии
    $MachinePath = [Environment]::GetEnvironmentVariable("Path", "Machine")
    $FreshUserPath = [Environment]::GetEnvironmentVariable("Path", "User")
    $Combined = "$MachinePath;$FreshUserPath" -split ';' | 
                Where-Object { $_.Trim().Length -gt 0 } | 
                Select-Object -Unique
    $env:Path = $Combined -join ';'
    Write-Info "PATH обновлён в текущей сессии"

    # 5. Создаём ярлык на рабочем столе (если нет)
    $Desktop = [Environment]::GetFolderPath("Desktop")
    $LnkPath = Join-Path $Desktop "$AppName.lnk"
    if (-not (Test-Path $LnkPath)) {
        $Wsh = New-Object -ComObject WScript.Shell
        $Lnk = $Wsh.CreateShortcut($LnkPath)
        $Lnk.TargetPath = $DestExe
        $Lnk.WorkingDirectory = $InstallDir
        $Lnk.Description = "$AppName — калькулятор расстояний"
        $Lnk.Save()
        Write-Ok "Создан ярлык на рабочем столе"
    }

    Write-Host "`n🎉 Установка завершена! Теперь из любого терминала работает:" -ForegroundColor Green
    Write-Host "   $AppName --help" -ForegroundColor White
    Write-Host "   $AppName --lat1 55.75 --lon1 37.62 --lat2 59.93 --lon2 30.33" -ForegroundColor White
    exit 0
}
catch {
    Write-Fail "Ошибка: $($_.Exception.Message)"
    exit 1
}