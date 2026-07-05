# UrbanFix API V2 - Configuracion Supabase

Copia estos valores en `appsettings.Development.json` o en User Secrets:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=db.XXXX.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=TU_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
dotnet user-secrets set "Supabase:Url" "https://XXXX.supabase.co"
dotnet user-secrets set "Supabase:JwtSecret" "TU_JWT_SECRET"
dotnet user-secrets set "Supabase:ServiceRoleKey" "TU_SERVICE_ROLE_KEY"
dotnet user-secrets set "Supabase:StorageBucket" "report-images"
```

Antes de levantar la API:

1. Ejecuta `Scripts/supabase-init.sql` en Supabase SQL Editor (o deja que EF aplique migraciones en Development).
2. Crea el bucket `report-images` (publico) en Supabase Storage.
3. Crea al menos un usuario en Supabase Auth.

Endpoints utiles:

- GET `/api/health` - estado de BD y Supabase
- GET `/api/Reports` - listar reportes
- GET `/api/Profiles/me` - perfil del JWT
- POST `/api/Reports` - crear reporte (JWT requerido)
