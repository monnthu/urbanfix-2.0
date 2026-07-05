-- Ejecutar en Supabase -> SQL Editor DESPUES de aplicar migraciones EF
-- Vincula profiles con auth.users y crea perfil automatico al registrarse

alter table public.profiles
    drop constraint if exists profiles_id_fkey;

alter table public.profiles
    add constraint profiles_id_fkey
    foreign key (id) references auth.users(id) on delete cascade;

create or replace function public.handle_new_user()
returns trigger
language plpgsql
security definer
set search_path = public
as $$
begin
    insert into public.profiles (id, role, full_name, national_id, created_at)
    values (
        new.id,
        'civilian',
        new.raw_user_meta_data->>'full_name',
        new.raw_user_meta_data->>'national_id',
        now()
    )
    on conflict (id) do nothing;

    return new;
end;
$$;

drop trigger if exists on_auth_user_created on auth.users;

create trigger on_auth_user_created
    after insert on auth.users
    for each row execute function public.handle_new_user();

-- Perfil para usuario de prueba si ya existia antes del trigger
insert into public.profiles (id, role, full_name, national_id, created_at)
select
    id,
    'civilian',
    raw_user_meta_data->>'full_name',
    raw_user_meta_data->>'national_id',
    now()
from auth.users
where email = 'ciudadano@urbanfix.test'
on conflict (id) do nothing;

-- Crear administrador manualmente (NO usar registro web):
-- 1) Crea el usuario en Supabase Auth (Authentication -> Users -> Add user)
-- 2) Ejecuta:
-- update public.profiles set role = 'admin' where id = '<uuid-del-usuario>';
