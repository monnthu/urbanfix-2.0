# urbanfix

urbanfix is a mobile-first civic reporting MVP built with Flutter Web and
Supabase. The first foundation pass includes public landing and map routes,
protected reporting/admin/government placeholders, Supabase initialization, and
Netlify static deployment configuration.

## Local Setup

1. Install Flutter and enable web support.
2. Use `.env.example` as the checklist for required Supabase values.
3. Install dependencies:

   ```sh
   flutter pub get
   ```

4. Start the web app:

   ```sh
   flutter run -d chrome \
     --dart-define=SUPABASE_URL=https://your-project-ref.supabase.co \
     --dart-define=SUPABASE_ANON_KEY=your-public-anon-key
   ```

   You can omit the `--dart-define` values while working on public
   placeholders. Supabase login remains disabled until both values are present.

## Routes

- `/` public landing page.
- `/map` public map placeholder.
- `/login` Supabase Google OAuth entry point.
- `/reports` protected civilian report placeholder.
- `/government` protected institution dashboard placeholder.
- `/admin` protected admin approval placeholder.

Protected routes currently require a Supabase auth session. Role-specific
civilian, institution, and admin checks are intentionally left for the next
planned implementation tasks.

## Supabase Configuration

Create a Supabase project and enable Google OAuth before testing login. For
Flutter Web, add the local and deployed URLs to Supabase Auth redirect URLs,
for example:

- `http://localhost:*/reports`
- `http://localhost:*/government`
- `https://your-netlify-site.netlify.app/reports`
- `https://your-netlify-site.netlify.app/government`

## Smoke Check

For this foundation task:

- `flutter run -d chrome` should load the public landing page.
- `/map` should load without authentication.
- `/reports`, `/government`, and `/admin` should redirect to `/login` when no
  Supabase session exists.
