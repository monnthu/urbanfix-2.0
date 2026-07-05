# Configura Supabase para UrbanFix (ejecutar una sola vez desde esta carpeta Scripts)
$ErrorActionPreference = "Stop"

$projectRoot = Split-Path $PSScriptRoot -Parent
$settingsPath = Join-Path $projectRoot "appsettings.Development.json"
if (-not (Test-Path $settingsPath)) {
    throw "No se encontro $settingsPath"
}

$settings = Get-Content $settingsPath -Raw | ConvertFrom-Json
$supabaseUrl = $settings.Supabase.Url
$serviceRoleKey = $settings.Supabase.ServiceRoleKey
$connectionString = $settings.ConnectionStrings.DefaultConnection

if ([string]::IsNullOrWhiteSpace($supabaseUrl) -or [string]::IsNullOrWhiteSpace($serviceRoleKey)) {
    throw "Configura Supabase:Url y Supabase:ServiceRoleKey en appsettings.Development.json"
}

$headers = @{
    Authorization = "Bearer $serviceRoleKey"
    apikey        = $serviceRoleKey
}

Write-Host "1. Creando bucket report-images (publico)..." -ForegroundColor Cyan
try {
    Invoke-RestMethod -Method Post -Uri "$supabaseUrl/storage/v1/bucket" -Headers $headers -ContentType "application/json" -Body '{"id":"report-images","name":"report-images","public":true}' | Out-Null
    Write-Host "   Bucket creado." -ForegroundColor Green
}
catch {
    Write-Host "   $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host "2. Creando usuario de prueba ciudadano@urbanfix.test ..." -ForegroundColor Cyan
try {
    $userBody = @{
        email         = "ciudadano@urbanfix.test"
        password      = "Urbanfix123!"
        email_confirm = $true
        user_metadata = @{
            role      = "civilian"
            full_name = "Ciudadano de prueba"
        }
    } | ConvertTo-Json

    Invoke-RestMethod -Method Post -Uri "$supabaseUrl/auth/v1/admin/users" -Headers $headers -ContentType "application/json" -Body $userBody | Out-Null
    Write-Host "   Usuario creado." -ForegroundColor Green
}
catch {
    Write-Host "   $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host "3. Aplicando migraciones EF..." -ForegroundColor Cyan
$apiProject = Join-Path $projectRoot "UrbanFix.API.csproj"
dotnet ef database update --project $apiProject --connection $connectionString
if ($LASTEXITCODE -ne 0) { throw "Fallo dotnet ef database update" }

Write-Host "4. Aplicando trigger de Auth..." -ForegroundColor Cyan
dotnet run --project $apiProject --no-launch-profile -- --apply-auth-sql
if ($LASTEXITCODE -ne 0) { throw "Fallo apply-auth-sql" }

Write-Host "Listo." -ForegroundColor Green
