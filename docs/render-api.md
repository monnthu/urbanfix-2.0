# Render API Deployment

Deploy only the ASP.NET API project to Render:

`UrbanFix.API V2/UrbanFix.API`

The repository includes a Dockerfile for the API and a `render.yaml` blueprint.

## Render Setup

1. Create a new Render Blueprint from this GitHub repository, or create a Web
   Service with Docker.
2. If creating the service manually, use:
   - Dockerfile path: `./UrbanFix.API V2/UrbanFix.API/Dockerfile`
   - Docker context: `.`
   - Health check path: `/api/health`
3. Add the environment variables below in Render.

## Required Environment Variables

```text
ConnectionStrings__DefaultConnection
Supabase__Url
Supabase__ServiceRoleKey
Supabase__JwtSecret
Supabase__StorageBucket=report-images
Cors__AllowedOrigins__0
```

Use the Supabase direct Postgres connection string for
`ConnectionStrings__DefaultConnection`. Do not commit any Supabase keys or
database passwords to the repository.

Set `Cors__AllowedOrigins__0` later to the deployed web app origin. While the
frontend is not deployed, this value can be left blank or set to the local web
origin for testing.

## After Deploy

Open:

```text
https://YOUR_RENDER_SERVICE.onrender.com/api/health
```

The API is healthy when the response status is `healthy` and the database check
is `connected`.
