CREATE TABLE IF NOT EXISTS public.data_fixes (
    name text PRIMARY KEY,
    applied_at timestamptz NOT NULL DEFAULT now()
);
