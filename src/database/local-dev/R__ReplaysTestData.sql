DELETE FROM public.replays;

INSERT INTO public.replays (name, status, filename, league_id, date, winner, teams)
VALUES (
    '2024-03-10',
    'Processed',
    'seed_replay1.WAgame',
    'redgate',
    '2024-03-10 19:00:00',
    'Team Beta',
    ARRAY['Team Alpha', 'Team Beta']
);

INSERT INTO public.replays (name, status, filename, league_id, date, winner, teams)
VALUES (
    '2024-03-24',
    'Processed',
    'seed_replay2.WAgame',
    'redgate',
    '2024-03-24 20:15:00',
    'Team Alpha',
    ARRAY['Team Alpha', 'Team Beta']
);

INSERT INTO public.replays (name, status, filename, league_id, date, winner, teams)
VALUES (
    '2024-04-07',
    'Processed',
    'seed_replay3.WAgame',
    'redgate',
    '2024-04-07 19:45:00',
    'Team Beta',
    ARRAY['Team Alpha', 'Team Beta']
);

INSERT INTO public.replays (name, status, filename, league_id, date, winner, teams)
VALUES (
    '2024-04-21',
    'Processed',
    'seed_replay4.WAgame',
    'redgate',
    '2024-04-21 20:00:00',
    'Team Alpha',
    ARRAY['Team Alpha', 'Team Beta']
);

INSERT INTO public.replays (name, status, filename, league_id, date, winner, teams)
VALUES (
    '2024-05-05',
    'Pending',
    'seed_replay5.WAgame',
    'redgate',
    NULL,
    NULL,
    NULL
);
