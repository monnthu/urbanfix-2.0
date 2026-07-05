# Ejecuta supabase-auth-trigger.sql usando la connection string de appsettings.Development.json
$ErrorActionPreference = "Stop"
$projectRoot = Split-Path $PSScriptRoot -Parent
$settings = Get-Content (Join-Path $projectRoot "appsettings.Development.json") -Raw | ConvertFrom-Json
$connectionString = $settings.ConnectionStrings.DefaultConnection
$sqlFile = Join-Path $PSScriptRoot "supabase-auth-trigger.sql"
$sql = Get-Content $sqlFile -Raw

Add-Type -Path (Get-ChildItem "$env:USERPROFILE\.nuget\packages\npgsql\*\lib\net*\Npgsql.dll" | Sort-Object FullName -Descending | Select-Object -First 1 -ExpandProperty FullName)

$connection = New-Object Npgsql.NpgsqlConnection($connectionString)
$connection.Open()
try {
    $command = $connection.CreateCommand()
    $command.CommandText = $sql
    $command.ExecuteNonQuery() | Out-Null
    Write-Host "Trigger de Auth aplicado correctamente." -ForegroundColor Green
}
finally {
    $connection.Close()
}
