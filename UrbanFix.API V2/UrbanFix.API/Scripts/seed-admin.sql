-- Crear un administrador (NO disponible desde la plataforma web)
--
-- 1. En Supabase Dashboard -> Authentication -> Users -> Add user
--    Registra el correo y contrasena del administrador.
--
-- 2. Espera unos segundos a que el trigger handle_new_user cree el perfil (role = civilian).
--
-- 3. Ejecuta este SQL reemplazando el correo.
--    El usuario debe existir en Supabase Auth con email_confirm = true
--    (los administradores no requieren verificacion por correo en la API).

update public.profiles
set role = 'admin'
where id = (
    select id from auth.users where email = 'admin@urbanfix.test'
);

-- Confirmar correo del administrador en Auth (si aun no lo esta):
-- update auth.users
-- set email_confirmed_at = now()
-- where email = 'admin@urbanfix.test' and email_confirmed_at is null;

-- Verifica:
-- select p.id, u.email, p.role, p.full_name from public.profiles p
-- join auth.users u on u.id = p.id where p.role = 'admin';
