ALTER TABLE public.replay_placements
    ADD COLUMN elo_delta integer NULL,
    ADD COLUMN elo_after integer NULL;
