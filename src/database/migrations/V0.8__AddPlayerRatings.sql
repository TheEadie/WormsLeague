CREATE TABLE IF NOT EXISTS public.player_ratings (
    player_auth_subject  text    NOT NULL REFERENCES public.players (auth_subject),
    league_id            text    NOT NULL,
    rating               integer NOT NULL,
    games_played         integer NOT NULL,
    PRIMARY KEY (player_auth_subject, league_id)
);
