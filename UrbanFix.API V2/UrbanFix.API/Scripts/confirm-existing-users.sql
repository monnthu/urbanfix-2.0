-- Confirmar correos de usuarios existentes (sin verificacion por email).
-- Ejecutar en Supabase -> SQL Editor si hay cuentas creadas antes del registro via API.

update auth.users
set email_confirmed_at = coalesce(email_confirmed_at, now())
where email_confirmed_at is null;
