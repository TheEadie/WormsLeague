CREATE TABLE IF NOT EXISTS public.games
(
    id integer NOT NULL DEFAULT nextval('games_id_seq'::regclass),
    status text COLLATE pg_catalog."default",
    hostmachine text COLLATE pg_catalog."default",
    CONSTRAINT games_pkey PRIMARY KEY (id)
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.games
    OWNER to "spawn_admin_eZiH";