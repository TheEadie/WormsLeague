CREATE SEQUENCE IF NOT EXISTS public.games_id_seq
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 2147483647
    CACHE 1;

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

ALTER SEQUENCE public.games_id_seq
    OWNED BY games.id;