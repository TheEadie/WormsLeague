-- Set league for all existing rows
UPDATE public.replays SET league_id = 'redgate';

-- Backfill date from full_log for rows that have it
UPDATE public.replays
SET
    date = (
        regexp_match(fulllog, 'Game Started at (\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}) GMT')
    )[1]::timestamp
WHERE fulllog IS NOT NULL;

-- Backfill winner from full_log
UPDATE public.replays
SET
    winner = CASE
        WHEN fulllog ~ 'The round was drawn\.' THEN 'Draw'
        ELSE (regexp_match(fulllog, '(.+) wins the (?:match!|round\.)'))[1]
    END
WHERE fulllog IS NOT NULL;

-- Backfill teams from full_log
-- Group 1: player/team name; group 2 (optional): team name when online format ("player" as "team")
-- COALESCE(m[2], m[1]) returns the team name for both online and offline formats
UPDATE public.replays
SET
    teams = ARRAY(
        SELECT DISTINCT COALESCE(m[2], m[1])
        FROM regexp_matches(
            fulllog,
            'Colour: "([^"]+)"(?: as "([^"]+)")?',
            'g'
        ) AS m
        WHERE COALESCE(m[2], m[1]) IS NOT NULL
    )
WHERE fulllog IS NOT NULL;
