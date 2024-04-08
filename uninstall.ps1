[CmdletBinding()]
param([switch]$Force)

$AppName = "GeoDistanceTool"
$InstallDir = "$env:LOCALAPPDATA\Programs\$AppName"

if (!(Test-Path $InstallDir) -and !$Force) {
    Write-Host "$AppName не установлен." -ForegroundColor Yellow
    exit 0
}

# Удаляем из PATH
$RegPath = "HKCU:\Environment"
$CurrentPath = (Get-ItemProperty -Path $RegPath -Name Path -ErrorAction SilentlyContinue).Path
if ($CurrentPath -like "*$InstallDir*") {
    $NewPath = ($CurrentPath -split ';' | Where-Object { $_ -ne $InstallDir }) -join ';'
    Set-ItemProperty -Path $RegPath -Name Path -Value $NewPath
    $env:Path = ($env:Path -split ';' | Where-Object { $_ -ne $InstallDir }) -join ';'
    Write-Host "✓ Удалено из PATH" -ForegroundColor Green
}

# Удаляем файлы
if (Test-Path $InstallDir) {
    Remove-Item $InstallDir -Recurse -Force
    Write-Host "✓ Удалена папка: $InstallDir" -ForegroundColor Green
}

# Удаляем ярлык
$Desktop = [Environment]::GetFolderPath("Desktop")
$Shortcut = Join-Path $Desktop "$AppName.lnk"
if (Test-Path $Shortcut) {
    Remove-Item $Shortcut -Force
    Write-Host "✓ Удалён ярлык с рабочего стола" -ForegroundColor Green
}

Write-Host "`n$AppName удалён. Перезапустите терминал для применения изменений." -ForegroundColor Cyan