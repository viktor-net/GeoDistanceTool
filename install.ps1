<#
.SYNOPSIS
    Устанавливает GeoDistanceTool в систему (добавляет в PATH пользователя)
#>
[CmdletBinding()]
param(
    [switch]$Force,
    [switch]$Quiet
)

$AppName = "GeoDistanceTool"
$AppVersion = "1.0.0"
$InstallDir = "$env:LOCALAPPDATA\Programs\$AppName"
$ExeName = "$AppName.exe"

# Цветной вывод (если не тихий режим)
function Write-Status { param([string]$Msg, [string]$Color = "Cyan") if (!$Quiet) { $c = $Host.UI.RawUI.ForegroundColor; $Host.UI.RawUI.ForegroundColor = $Color; Write-Host "→ $Msg"; $Host.UI.RawUI.ForegroundColor = $c } }
function Write-Error { param([string]$Msg) if (!$Quiet) { $c = $Host.UI.RawUI.ForegroundColor; $Host.UI.RawUI.ForegroundColor = "Red"; Write-Host "✗ $Msg"; $Host.UI.RawUI.ForegroundColor = $c } else { Write-Host "ERROR: $Msg" -ForegroundColor Red } }
function Write-Success { param([string]$Msg) if (!$Quiet) { $c = $Host.UI.RawUI.ForegroundColor; $Host.UI.RawUI.ForegroundColor = "Green"; Write-Host "✓ $Msg"; $Host.UI.RawUI.ForegroundColor = $c } }

# Проверка прав (для записи в реестр не нужны админ-права — пишем в HKCU)
if (!$Force -and ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Status "Запуск от администратора не требуется. Установка будет выполнена для текущего пользователя."
}

# 1. Создаём папку установки
if (!(Test-Path $InstallDir)) {
    New-Item -ItemType Directory -Path $InstallDir -Force | Out-Null
    Write-Status "Создана папка: $InstallDir"
}

# 2. Копируем исполняемый файл
$SourceExe = Join-Path $PSScriptRoot $ExeName
$DestExe = Join-Path $InstallDir $ExeName
if (!(Test-Path $SourceExe)) {
    Write-Error "Не найден файл $SourceExe"
    exit 1
}
Copy-Item $SourceExe $DestExe -Force
Write-Status "Скопирован $ExeName → $InstallDir"

# 3. Добавляем папку в PATH пользователя (через реестр — надёжнее, чем [Environment]::SetEnvironmentVariable)
$RegPath = "HKCU:\Environment"
$CurrentPath = (Get-ItemProperty -Path $RegPath -Name Path -ErrorAction SilentlyContinue).Path
if ($CurrentPath -notlike "*$InstallDir*") {
    $NewPath = "$CurrentPath;$InstallDir"
    Set-ItemProperty -Path $RegPath -Name Path -Value $NewPath.TrimStart(';')
    Write-Status "Добавлено в PATH пользователя: $InstallDir"
    
    # Обновляем PATH в текущей сессии
    $env:Path += ";$InstallDir"
} else {
    Write-Status "Путь уже присутствует в PATH" "Gray"
}

# 4. Создаём ярлык на рабочем столе (опционально)
$Desktop = [Environment]::GetFolderPath("Desktop")
$ShortcutPath = Join-Path $Desktop "$AppName.lnk"
if (!(Test-Path $ShortcutPath)) {
    $WScript = New-Object -ComObject WScript.Shell
    $Shortcut = $WScript.CreateShortcut($ShortcutPath)
    $Shortcut.TargetPath = $DestExe
    $Shortcut.WorkingDirectory = $InstallDir
    $Shortcut.Description = "$AppName v$AppVersion — калькулятор расстояний"
    $Shortcut.Save()
    Write-Status "Создан ярлык на рабочем столе"
}

# 5. Финал
Write-Success "Готово! $AppName установлен."
Write-Host ""
if (!$Quiet) {
    Write-Host "Теперь вы можете запускать:" -ForegroundColor White
    Write-Host "  $AppName --help" -ForegroundColor Green
    Write-Host "  $AppName --lat1 55.75 --lon1 37.62 --lat2 59.93 --lon2 30.33" -ForegroundColor Green
    Write-Host ""
    Write-Host "Если команда не распознаётся — перезапустите терминал." -ForegroundColor Yellow
}
exit 0