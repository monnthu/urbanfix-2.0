# UrbanFixWEB2 — deploy en Render (gratis)

Copia de UrbanfixWEB preparada para hosting en Render, conectada a tu API externa.

## Variables de entorno (Render Dashboard)

| Variable | Descripción |
|----------|-------------|
| `ApiSettings__BaseUrl` | URL pública de tu API (ej. `https://tu-api.onrender.com`) |
| `ApiSettings__SupabaseUrl` | `https://xxx.supabase.co` |
| `ApiSettings__SupabaseAnonKey` | Anon key de Supabase |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

Opcional para desarrollo local: copia `Urbanfix/appsettings.Development.json.example` a `appsettings.Development.json` y completa los valores.

## Deploy manual en Render

1. Conecta el repo de GitHub en [render.com](https://render.com).
2. **New → Web Service** → selecciona el repositorio.
3. **Root Directory:** `UrbanFixWEB2`
4. **Runtime:** Docker
5. **Instance type:** Free
6. Agrega las variables de entorno de la tabla anterior.
7. Deploy.

## Supabase

En Authentication → URL Configuration:

- **Site URL:** `https://tu-app.onrender.com`
- **Redirect URLs:** rutas que uses tras login (`/Reportar`, `/Admin`, etc.)

## Desarrollo local

```powershell
cd UrbanFixWEB2/Urbanfix
dotnet run
```

## Docker local

```powershell
cd UrbanFixWEB2
docker build -t urbanfix-web .
docker run -p 8080:10000 -e PORT=10000 -e ApiSettings__BaseUrl=https://tu-api.com urbanfix-web
```
