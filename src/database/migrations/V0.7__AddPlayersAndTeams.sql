CREATE TABLE IF NOT EXISTS public.players (
    id            integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    auth0_subject text    NOT NULL UNIQUE,
    display_name  text    NOT NULL
);

CREATE TABLE IF NOT EXISTS public.teams (
    id          integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    machine     text    NOT NULL,
    team_name   text    NOT NULL,
    player_id   integer REFERENCES public.players (id),
    UNIQUE (machine, team_name)
);

INSERT INTO public.teams (machine, team_name)
SELECT DISTINCT machine, team_name
FROM public.replay_placements
ON CONFLICT DO NOTHING;
