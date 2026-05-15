CREATE TABLE IF NOT EXISTS public.players (
    auth_subject text NOT NULL PRIMARY KEY,
    display_name text NOT NULL
);

CREATE TABLE IF NOT EXISTS public.teams (
    id                   integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    machine              text    NOT NULL,
    team_name            text    NOT NULL,
    player_auth_subject  text    REFERENCES public.players (auth_subject),
    UNIQUE (machine, team_name)
);

INSERT INTO public.teams (machine, team_name)
SELECT DISTINCT machine, team_name
FROM public.replay_placements
ON CONFLICT DO NOTHING;
