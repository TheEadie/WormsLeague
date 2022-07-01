CREATE SEQUENCE IF NOT EXISTS public.games_id_seq
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 2147483647
    CACHE 1;

ALTER SEQUENCE public.games_id_seq
    OWNER TO "spawn_admin_eZiH";

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

ALTER SEQUENCE public.games_id_seq
    OWNED BY games.id;