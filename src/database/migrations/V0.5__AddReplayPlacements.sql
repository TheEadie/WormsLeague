CREATE TABLE IF NOT EXISTS public.replay_placements (
    replay_id   integer NOT NULL REFERENCES public.replays (id),
    machine     text    NOT NULL,
    team_name   text    NOT NULL,
    position    integer NOT NULL,
    PRIMARY KEY (replay_id, machine, team_name)
);
