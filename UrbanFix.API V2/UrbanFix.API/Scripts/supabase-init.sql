-- UrbanFix: esquema inicial para Supabase (PostgreSQL)
-- Ejecutar en Supabase -> SQL Editor

create table if not exists public.profiles (
    id uuid primary key,
    role text not null default 'civilian',
    full_name text,
    national_id varchar(15),
    created_at timestamptz not null default now()
);

create unique index if not exists ix_profiles_national_id on public.profiles(national_id);

create table if not exists public.institutions (
    id uuid primary key default gen_random_uuid(),
    profile_id uuid not null references public.profiles(id) on delete cascade,
    name varchar(200) not null,
    official_domain varchar(200) not null,
    category varchar(100),
    zone varchar(100),
    status text not null default 'pending',
    created_at timestamptz not null default now(),
    reviewed_at timestamptz
);

create table if not exists public.reports (
    id uuid primary key,
    title varchar(200) not null,
    description varchar(2000) not null,
    category integer not null,
    priority integer,
    latitude numeric(9, 6) not null,
    longitude numeric(9, 6) not null,
    civilian_user_id uuid not null references public.profiles(id),
    institution_id uuid references public.institutions(id) on delete set null,
    status integer not null default 0,
    ai_category text,
    ai_priority text,
    ai_confidence numeric(4, 3),
    created_at timestamptz not null default now(),
    updated_at timestamptz
);

create table if not exists public.report_images (
    id uuid primary key,
    report_id uuid not null references public.reports(id) on delete cascade,
    storage_path varchar(500) not null,
    thumbnail_path varchar(500),
    content_type varchar(50) not null,
    file_size_bytes bigint not null,
    sort_order smallint not null default 0,
    created_at timestamptz not null default now()
);

create index if not exists idx_reports_civilian_user_id on public.reports(civilian_user_id);
create index if not exists idx_reports_created_at on public.reports(created_at desc);
create index if not exists idx_report_images_report_id on public.report_images(report_id);

-- Vincular profiles con auth.users (Supabase Auth)
alter table public.profiles
    drop constraint if exists profiles_id_fkey;

alter table public.profiles
    add constraint profiles_id_fkey
    foreign key (id) references auth.users(id) on delete cascade;

-- Crear perfil automaticamente al registrarse
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

-- Tabla de historial de migraciones EF (si usas dotnet ef database update)
create table if not exists public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) not null,
    "ProductVersion" character varying(32) not null,
    constraint "PK___EFMigrationsHistory" primary key ("MigrationId")
);

-- Storage: crear bucket "report-images" como publico desde el panel de Supabase Storage
