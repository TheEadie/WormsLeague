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

INSERT INTO public.replays (name, status, filename, league_id, date, winner, teams, fulllog)
VALUES (
    '2024-06-01',
    'Processed',
    'seed_replay_withlog.WAgame',
    'redgate',
    '2024-06-01 19:00:00',
    'Team Alpha',
    ARRAY['Team Alpha', 'Team Beta'],
    $$Game Started at 2024-06-01 19:00:00 GMT
Red: "player1" as "Team Alpha"
Blue: "player2" as "Team Beta"
[00:00:05.00] ••• Team Alpha (player1) starts turn
[00:00:08.00] ••• Team Alpha (player1) fires Shotgun
[00:00:25.00] ••• Damage dealt: 45 to Team Beta (player2)
[00:00:27.00] ••• Team Alpha (player1) ends turn; time used: 22.00 sec turn, 3.00 sec retreat
[00:00:35.00] ••• Team Beta (player2) starts turn
[00:00:40.00] ••• Team Beta (player2) fires Grenade (3 sec, min bounce)
[00:00:58.00] ••• Damage dealt: 30 to Team Alpha (player1)
[00:01:00.00] ••• Team Beta (player2) ends turn; time used: 25.00 sec turn, 3.00 sec retreat
[00:01:10.00] ••• Team Alpha (player1) starts turn
[00:01:15.00] ••• Team Alpha (player1) fires Ninja Rope
[00:01:20.00] ••• Team Alpha (player1) fires Banana Bomb (5 sec)
[00:01:35.00] ••• Damage dealt: 80 (1 kill) to Team Beta (player2)
[00:01:37.00] ••• Team Alpha (player1) ends turn; time used: 27.00 sec turn, 3.00 sec retreat
[00:01:45.00] ••• Team Beta (player2) starts turn
[00:01:50.00] ••• Team Beta (player2) fires Bazooka
[00:02:05.00] ••• Damage dealt: 50 to Team Alpha (player1)
[00:02:07.00] ••• Team Beta (player2) ends turn; time used: 22.00 sec turn, 3.00 sec retreat
Team Alpha wins the match!
$$
);
