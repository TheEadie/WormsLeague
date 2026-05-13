ALTER TABLE public.replays
    ADD COLUMN IF NOT EXISTS league_id text,
    ADD COLUMN IF NOT EXISTS date      timestamp,
    ADD COLUMN IF NOT EXISTS winner    text,
    ADD COLUMN IF NOT EXISTS teams     text[];

ALTER TABLE public.replays
    ADD CONSTRAINT replays_leagues_fk
    FOREIGN KEY (league_id) REFERENCES public.leagues (id);
