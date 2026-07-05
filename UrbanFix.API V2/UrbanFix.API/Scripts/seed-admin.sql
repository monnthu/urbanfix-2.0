-- Crear un administrador (NO disponible desde la plataforma web)
--
-- 1. En Supabase Dashboard -> Authentication -> Users -> Add user
--    Registra el correo y contrasena del administrador.
--
-- 2. Espera unos segundos a que el trigger handle_new_user cree el perfil (role = civilian).
--
-- 3. Ejecuta este SQL reemplazando el correo.

update public.profiles
set role = 'admin'
where id = (
    select id from auth.users where email = 'admin@urbanfix.test'
);

-- Verifica:
-- select p.id, u.email, p.role, p.full_name from public.profiles p
-- join auth.users u on u.id = p.id where p.role = 'admin';
