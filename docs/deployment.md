# Flutter Web Deployment

This MVP deploys as a static Flutter Web build on Netlify.

## Build Command

```sh
flutter build web --release \
  --dart-define=SUPABASE_URL=$SUPABASE_URL \
  --dart-define=SUPABASE_ANON_KEY=$SUPABASE_ANON_KEY
```

Netlify publishes the generated `build/web` directory. The repository includes
`netlify.toml` with an SPA redirect so direct links such as `/reports` and
`/government` are served by Flutter routing.

## Environment Values

Set these Netlify environment variables:

- `SUPABASE_URL`
- `SUPABASE_ANON_KEY`

The Netlify build command passes those values to Flutter as `--dart-define`
values. You can use the same pattern for one-off local builds.

## Supabase Redirect URLs

Add the deployed Netlify URL to Supabase Auth redirect URLs before testing
Google OAuth. Include each protected route that can be used as a post-login
destination:

- `https://your-netlify-site.netlify.app/reports`
- `https://your-netlify-site.netlify.app/government`
- `https://your-netlify-site.netlify.app/admin`
