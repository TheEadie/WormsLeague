TRUNCATE public.teams, public.players RESTART IDENTITY CASCADE;
TRUNCATE public.replays RESTART IDENTITY CASCADE;

-- 1: Processed, 2 teams, with placements (Alpha wins)
INSERT INTO public.replays (name, status, filename, league_id, date, winner, teams, fulllog)
VALUES (
    '2024-03-10',
    'Processed',
    'seed_replay1.WAgame',
    'redgate',
    '2024-03-10 19:00:00',
    'Team Alpha',
    ARRAY['Team Alpha', 'Team Beta'],
    $$Game Started at 2024-03-10 19:00:00 GMT
Red: "machine-alpha" as "Team Alpha"
Blue: "machine-beta" as "Team Beta"
[00:00:05.00] ••• Team Alpha (machine-alpha) starts turn
[00:00:12.00] ••• Team Alpha (machine-alpha) fires Bazooka
[00:00:22.00] ••• Damage dealt: 52 to Team Beta (machine-beta)
[00:00:28.00] ••• Team Alpha (machine-alpha) ends turn; time used: 23.00 sec turn, 3.00 sec retreat
[00:00:33.00] ••• Team Beta (machine-beta) starts turn
[00:00:41.00] ••• Team Beta (machine-beta) fires Shotgun
[00:00:55.00] ••• Damage dealt: 33 to Team Alpha (machine-alpha)
[00:01:00.00] ••• Team Beta (machine-beta) ends turn; time used: 27.00 sec turn, 3.00 sec retreat
[00:01:05.00] ••• Team Alpha (machine-alpha) starts turn
[00:01:11.00] ••• Team Alpha (machine-alpha) fires Banana Bomb (5 sec)
[00:01:28.00] ••• Damage dealt: 74 (1 kill) to Team Beta (machine-beta)
[00:01:35.00] ••• Team Alpha (machine-alpha) ends turn; time used: 30.00 sec turn, 3.00 sec retreat
Team Alpha wins the match!
$$
);

-- 2: Processed, 3 teams, with placements (Gamma wins)
INSERT INTO public.replays (name, status, filename, league_id, date, winner, teams, fulllog)
VALUES (
    '2024-04-07',
    'Processed',
    'seed_replay2.WAgame',
    'redgate',
    '2024-04-07 19:45:00',
    'Team Gamma',
    ARRAY['Team Alpha', 'Team Beta', 'Team Gamma'],
    $$Game Started at 2024-04-07 19:45:00 GMT
Red: "machine-alpha" as "Team Alpha"
Blue: "machine-beta" as "Team Beta"
Green: "machine-gamma" as "Team Gamma"
[00:00:06.00] ••• Team Alpha (machine-alpha) starts turn
[00:00:14.00] ••• Team Alpha (machine-alpha) fires Grenade (3 sec, min bounce)
[00:00:29.00] ••• Damage dealt: 45 to Team Beta (machine-beta)
[00:00:33.00] ••• Team Alpha (machine-alpha) ends turn; time used: 27.00 sec turn, 3.00 sec retreat
[00:00:38.00] ••• Team Beta (machine-beta) starts turn
[00:00:46.00] ••• Team Beta (machine-beta) fires Bazooka
[00:01:02.00] ••• Damage dealt: 38 to Team Gamma (machine-gamma)
[00:01:07.00] ••• Team Beta (machine-beta) ends turn; time used: 29.00 sec turn, 3.00 sec retreat
[00:01:12.00] ••• Team Gamma (machine-gamma) starts turn
[00:01:19.00] ••• Team Gamma (machine-gamma) fires Shotgun
[00:01:33.00] ••• Damage dealt: 67 (1 kill) to Team Alpha (machine-alpha)
[00:01:38.00] ••• Team Gamma (machine-gamma) ends turn; time used: 26.00 sec turn, 3.00 sec retreat
[00:01:43.00] ••• Team Beta (machine-beta) starts turn
[00:01:51.00] ••• Team Beta (machine-beta) fires Homing Missile
[00:02:05.00] ••• Damage dealt: 41 to Team Gamma (machine-gamma)
[00:02:10.00] ••• Team Beta (machine-beta) ends turn; time used: 27.00 sec turn, 3.00 sec retreat
[00:02:15.00] ••• Team Gamma (machine-gamma) starts turn
[00:02:21.00] ••• Team Gamma (machine-gamma) fires Banana Bomb (5 sec)
[00:02:38.00] ••• Damage dealt: 78 (1 kill) to Team Beta (machine-beta)
[00:02:45.00] ••• Team Gamma (machine-gamma) ends turn; time used: 30.00 sec turn, 3.00 sec retreat
Team Gamma wins the match!
$$
);

-- 3: Processed, 4 teams, with placements (Delta wins)
INSERT INTO public.replays (name, status, filename, league_id, date, winner, teams, fulllog)
VALUES (
    '2024-04-21',
    'Processed',
    'seed_replay3.WAgame',
    'redgate',
    '2024-04-21 20:00:00',
    'Team Delta',
    ARRAY['Team Alpha', 'Team Beta', 'Team Gamma', 'Team Delta'],
    $$Game Started at 2024-04-21 20:00:00 GMT
Red: "machine-alpha" as "Team Alpha"
Blue: "machine-beta" as "Team Beta"
Green: "machine-gamma" as "Team Gamma"
Yellow: "machine-delta" as "Team Delta"
[00:00:04.00] ••• Team Alpha (machine-alpha) starts turn
[00:00:12.00] ••• Team Alpha (machine-alpha) fires Bazooka
[00:00:26.00] ••• Damage dealt: 44 to Team Beta (machine-beta)
[00:00:31.00] ••• Team Alpha (machine-alpha) ends turn; time used: 27.00 sec turn, 3.00 sec retreat
[00:00:36.00] ••• Team Beta (machine-beta) starts turn
[00:00:44.00] ••• Team Beta (machine-beta) fires Grenade (3 sec)
[00:00:58.00] ••• Damage dealt: 51 to Team Gamma (machine-gamma)
[00:01:03.00] ••• Team Beta (machine-beta) ends turn; time used: 27.00 sec turn, 3.00 sec retreat
[00:01:08.00] ••• Team Gamma (machine-gamma) starts turn
[00:01:16.00] ••• Team Gamma (machine-gamma) fires Shotgun
[00:01:28.00] ••• Damage dealt: 37 to Team Delta (machine-delta)
[00:01:33.00] ••• Team Gamma (machine-gamma) ends turn; time used: 25.00 sec turn, 3.00 sec retreat
[00:01:38.00] ••• Team Delta (machine-delta) starts turn
[00:01:45.00] ••• Team Delta (machine-delta) fires Holy Hand Grenade
[00:02:00.00] ••• Damage dealt: 86 (1 kill) to Team Alpha (machine-alpha)
[00:02:07.00] ••• Team Delta (machine-delta) ends turn; time used: 29.00 sec turn, 3.00 sec retreat
[00:02:12.00] ••• Team Beta (machine-beta) starts turn
[00:02:19.00] ••• Team Beta (machine-beta) fires Bazooka
[00:02:33.00] ••• Damage dealt: 55 (1 kill) to Team Gamma (machine-gamma)
[00:02:38.00] ••• Team Beta (machine-beta) ends turn; time used: 26.00 sec turn, 3.00 sec retreat
[00:02:43.00] ••• Team Delta (machine-delta) starts turn
[00:02:50.00] ••• Team Delta (machine-delta) fires Banana Bomb (5 sec)
[00:03:06.00] ••• Damage dealt: 71 (1 kill) to Team Beta (machine-beta)
[00:03:12.00] ••• Team Delta (machine-delta) ends turn; time used: 29.00 sec turn, 3.00 sec retreat
[00:03:17.00] ••• Team Delta (machine-delta) starts turn
[00:03:24.00] ••• Team Delta (machine-delta) fires Cluster Bomb
[00:03:38.00] ••• Damage dealt: 63 (1 kill) to Team Gamma (machine-gamma)
[00:03:44.00] ••• Team Delta (machine-delta) ends turn; time used: 27.00 sec turn, 3.00 sec retreat
Team Delta wins the match!
$$
);

-- 4: Processed, 2 teams, draw — no winner, null positions
INSERT INTO public.replays (name, status, filename, league_id, date, winner, teams, fulllog)
VALUES (
    '2024-05-05',
    'Processed',
    'seed_replay4.WAgame',
    'redgate',
    '2024-05-05 19:30:00',
    NULL,
    ARRAY['Team Alpha', 'Team Beta'],
    $$Game Started at 2024-05-05 19:30:00 GMT
Red: "machine-alpha" as "Team Alpha"
Blue: "machine-beta" as "Team Beta"
[00:00:07.00] ••• Team Alpha (machine-alpha) starts turn
[00:00:15.00] ••• Team Alpha (machine-alpha) fires Bazooka
[00:00:28.00] ••• Damage dealt: 35 to Team Beta (machine-beta)
[00:00:33.00] ••• Team Alpha (machine-alpha) ends turn; time used: 26.00 sec turn, 3.00 sec retreat
[00:00:38.00] ••• Team Beta (machine-beta) starts turn
[00:00:46.00] ••• Team Beta (machine-beta) fires Shotgun
[00:00:59.00] ••• Damage dealt: 42 to Team Alpha (machine-alpha)
[00:01:04.00] ••• Team Beta (machine-beta) ends turn; time used: 26.00 sec turn, 3.00 sec retreat
[00:01:09.00] ••• Team Alpha (machine-alpha) starts turn
[00:01:17.00] ••• Team Alpha (machine-alpha) fires Grenade (3 sec, min bounce)
[00:01:32.00] ••• Damage dealt: 28 to Team Beta (machine-beta)
[00:01:37.00] ••• Team Alpha (machine-alpha) ends turn; time used: 28.00 sec turn, 3.00 sec retreat
[00:01:42.00] ••• Team Beta (machine-beta) starts turn
[00:01:50.00] ••• Team Beta (machine-beta) fires Homing Missile
[00:02:04.00] ••• Damage dealt: 33 to Team Alpha (machine-alpha)
[00:02:09.00] ••• Team Beta (machine-beta) ends turn; time used: 27.00 sec turn, 3.00 sec retreat
The round was drawn.
$$
);

-- 5: Processed, 2 teams, with placements and full log (Alpha wins)
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
Red: "machine-alpha" as "Team Alpha"
Blue: "machine-beta" as "Team Beta"
[00:00:05.00] ••• Team Alpha (machine-alpha) starts turn
[00:00:12.00] ••• Team Alpha (machine-alpha) fires Shotgun
[00:00:21.00] ••• Damage dealt: 38 to Team Beta (machine-beta)
[00:00:30.00] ••• Team Alpha (machine-alpha) ends turn; time used: 25.00 sec turn, 3.00 sec retreat
[00:00:35.00] ••• Team Beta (machine-beta) starts turn
[00:00:43.00] ••• Team Beta (machine-beta) fires Bazooka
[00:00:58.00] ••• Damage dealt: 47 to Team Alpha (machine-alpha)
[00:01:02.00] ••• Team Beta (machine-beta) ends turn; time used: 27.00 sec turn, 3.00 sec retreat
[00:01:07.00] ••• Team Alpha (machine-alpha) starts turn
[00:01:14.00] ••• Team Alpha (machine-alpha) fires Banana Bomb (5 sec)
[00:01:28.00] ••• Damage dealt: 65 to Team Beta (machine-beta)
[00:01:35.00] ••• Team Alpha (machine-alpha) ends turn; time used: 28.00 sec turn, 3.00 sec retreat
[00:01:40.00] ••• Team Beta (machine-beta) starts turn
[00:01:48.00] ••• Team Beta (machine-beta) fires Grenade (3 sec, min bounce)
[00:02:03.00] ••• Damage dealt: 29 to Team Alpha (machine-alpha)
[00:02:08.00] ••• Team Beta (machine-beta) ends turn; time used: 28.00 sec turn, 3.00 sec retreat
[00:02:13.00] ••• Team Alpha (machine-alpha) starts turn
[00:02:20.00] ••• Team Alpha (machine-alpha) fires Holy Hand Grenade
[00:02:38.00] ••• Damage dealt: 91 (1 kill) to Team Beta (machine-beta)
[00:02:45.00] ••• Team Alpha (machine-alpha) ends turn; time used: 32.00 sec turn, 3.00 sec retreat
Team Alpha wins the match!
$$
);

-- 6: Pending — awaiting processing (name used for sort; appears first in UI)
INSERT INTO public.replays (name, status, filename, league_id, date, winner, teams)
VALUES (
    '2024-06-15',
    'Pending',
    'seed_replay5.WAgame',
    'redgate',
    NULL,
    NULL,
    NULL
);

-- All teams
INSERT INTO public.teams (machine, team_name) VALUES ('machine-alpha', 'Team Alpha');
INSERT INTO public.teams (machine, team_name) VALUES ('machine-beta', 'Team Beta');
INSERT INTO public.teams (machine, team_name) VALUES ('machine-gamma', 'Team Gamma');
INSERT INTO public.teams (machine, team_name) VALUES ('machine-delta', 'Team Delta');

-- Pre-claim Team Beta to another player to demonstrate the "already claimed" state
INSERT INTO public.players (auth_subject, display_name) VALUES ('google-oauth2|100000000000000000001', 'Other Player');
UPDATE public.teams SET player_auth_subject = 'google-oauth2|100000000000000000001' WHERE machine = 'machine-beta' AND team_name = 'Team Beta';

-- Claim Team Alpha so that at least two players are claimed (required for ELO rankings to appear)
INSERT INTO public.players (auth_subject, display_name) VALUES ('google-oauth2|100000000000000000002', 'Alpha Player');
UPDATE public.teams SET player_auth_subject = 'google-oauth2|100000000000000000002' WHERE machine = 'machine-alpha' AND team_name = 'Team Alpha';
