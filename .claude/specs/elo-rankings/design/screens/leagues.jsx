// League Hub — shows all leagues the signed-in user belongs to

const {
  Box, Typography, Card, CardActionArea, CardContent,
  Paper, Chip, Avatar, AvatarGroup, Stack, Divider,
  Button, Icon, Grid,
} = window.MaterialUI;

function LeagueHub({ onEnter }) {
  const leagues = Object.values(window.LEAGUE_DEFS);

  return (
    <Box className="fade-in" sx={{ p: { xs: 2, md: 4 }, maxWidth: 1300, mx: "auto" }}>
      {/* Page header */}
      <Box sx={{ mb: 4 }}>
        <Typography variant="overline" color="text.secondary">Welcome back, Eadie</Typography>
        <Typography variant="h4" fontWeight={700} mt={0.5}>Your Leagues</Typography>
        <Typography variant="body2" color="text.secondary" mt={0.75}>
          {leagues.length} active leagues · {window.GAMES.length} total matches archived
        </Typography>
      </Box>

      {/* League cards */}
      <Box sx={{ display: "grid", gap: 2.5, gridTemplateColumns: { xs: "1fr", md: "repeat(3, 1fr)" } }}>
        {leagues.map(league => <LeagueCard key={league.id} league={league} onEnter={onEnter}/>)}
      </Box>

      {/* Recent activity across all leagues */}
      <Box sx={{ mt: 5 }}>
        <Typography variant="h6" fontWeight={700} mb={2}>Recent activity</Typography>
        <Paper variant="outlined">
          {window.GAMES.slice(0, 8).map((g, i) => {
            const league = window.LEAGUE_DEFS[g.leagueId];
            const winner = window.PLAYERS[g.winner];
            return (
              <React.Fragment key={g.id}>
                <Box
                  sx={{
                    display: "grid",
                    gridTemplateColumns: "100px 140px 1fr auto",
                    gap: 2, alignItems: "center",
                    px: 2.5, py: 1.5,
                    cursor: "pointer",
                    "&:hover": { bgcolor: "action.hover" },
                  }}
                  onClick={() => onEnter(g.leagueId, g.id)}
                >
                  {/* League badge */}
                  <Chip
                    label={league.short}
                    size="small"
                    sx={{
                      bgcolor: league.color + "22",
                      color: league.color,
                      border: `1px solid ${league.color}55`,
                      fontWeight: 700,
                      fontFamily: "JetBrains Mono",
                      fontSize: 11,
                    }}
                  />

                  {/* Date */}
                  <Box>
                    <Typography sx={{ fontFamily: "JetBrains Mono", fontSize: 13 }}>
                      {window.fmtDate(g.when)}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      {window.fmtTime(g.when)} · {g.map}
                    </Typography>
                  </Box>

                  {/* Players */}
                  <Stack direction="row" spacing={1} alignItems="center" flexWrap="wrap" useFlexGap>
                    <Icon sx={{ fontSize: 14, color: "warning.main" }}>workspace_premium</Icon>
                    <Typography variant="body2" fontWeight={700} color="warning.main">{winner.name}</Typography>
                    <Typography variant="caption" color="text.secondary">defeated</Typography>
                    {g.players.filter(p => p !== g.winner).map(pid => (
                      <Typography key={pid} variant="body2" color="text.secondary">
                        {window.PLAYERS[pid].name}
                      </Typography>
                    ))}
                  </Stack>

                  <Icon sx={{ color: "text.disabled" }}>chevron_right</Icon>
                </Box>
                {i < 7 && <Divider/>}
              </React.Fragment>
            );
          })}
        </Paper>
      </Box>
    </Box>
  );
}

function LeagueCard({ league, onEnter }) {
  const games   = window.gamesForLeague(league.id);
  const players = league.playerIds.map(id => window.PLAYERS[id]).filter(Boolean);
  const { standings } = window.computeLeagueElo(league.id);
  const top3 = standings.slice(0, 3);

  return (
    <Card
      variant="outlined"
      sx={{
        borderTop: `3px solid ${league.color}`,
        display: "flex", flexDirection: "column",
        transition: "box-shadow 0.15s, border-color 0.15s",
        "&:hover": { boxShadow: `0 0 0 1px ${league.color}66`, borderColor: league.color },
      }}
    >
      <CardActionArea onClick={() => onEnter(league.id, null)} sx={{ flex: 1, alignItems: "flex-start", display: "flex", flexDirection: "column" }}>
        <CardContent sx={{ width: "100%", p: 2.5, "&:last-child": { pb: 2.5 } }}>
          {/* Header */}
          <Box sx={{ display: "flex", alignItems: "flex-start", justifyContent: "space-between", mb: 2 }}>
            <Box>
              <Box sx={{ display: "flex", alignItems: "center", gap: 1, mb: 0.5 }}>
                <Box sx={{
                  width: 8, height: 8, borderRadius: "50%",
                  bgcolor: league.color, boxShadow: `0 0 6px ${league.color}`,
                }}/>
                <Typography variant="caption" sx={{ color: league.color, fontWeight: 700, letterSpacing: "0.1em", textTransform: "uppercase" }}>
                  {league.short}
                </Typography>
              </Box>
              <Typography variant="h6" fontWeight={700} lineHeight={1.2}>{league.name}</Typography>
              <Typography variant="caption" color="text.secondary">{league.season} · {league.week}</Typography>
            </Box>
            <Chip label={`${games.length} matches`} size="small" variant="outlined" sx={{ flexShrink: 0, mt: 0.5 }}/>
          </Box>

          <Typography variant="body2" color="text.secondary" mb={2} sx={{ lineHeight: 1.6 }}>
            {league.description}
          </Typography>

          <Divider sx={{ mb: 2 }}/>

          {/* Players */}
          <Box sx={{ mb: 2 }}>
            <Typography variant="caption" color="text.secondary" sx={{ textTransform: "uppercase", letterSpacing: "0.12em" }}>
              Players
            </Typography>
            <Box sx={{ display: "flex", alignItems: "center", gap: 1.5, mt: 0.75 }}>
              <AvatarGroup max={5} sx={{ "& .MuiAvatar-root": { width: 28, height: 28, fontSize: 12, fontWeight: 700 } }}>
                {players.map(p => (
                  <Avatar key={p.id} sx={{ bgcolor: TEAM_COLORS[p.color], color: "#000" }}>{p.avatar}</Avatar>
                ))}
              </AvatarGroup>
              <Typography variant="caption" color="text.secondary">{players.length} players</Typography>
            </Box>
          </Box>

          {/* Stats row — ELO top 3 */}
          <Paper variant="outlined" sx={{ p: 1.25, bgcolor: 'transparent' }}>
            <Box sx={{ display: 'flex', alignItems: 'baseline', justifyContent: 'space-between', mb: 1 }}>
              <Typography variant="caption" color="text.secondary" sx={{ textTransform: "uppercase", letterSpacing: "0.1em" }}>
                ELO leaderboard
              </Typography>
              <Typography sx={{ fontFamily: 'JetBrains Mono', fontSize: 10, color: 'text.disabled' }}>
                top 3 of {standings.length}
              </Typography>
            </Box>
            <Stack spacing={0.5}>
              {top3.map((s, i) => {
                const p = window.PLAYERS[s.playerId];
                const place = i + 1;
                const medalColor = ['#ffca28', '#bdbdbd', '#cd7f32'][i];
                return (
                  <Box key={s.playerId} sx={{
                    display: 'grid',
                    gridTemplateColumns: '18px 22px 1fr auto auto',
                    gap: 1, alignItems: 'center',
                  }}>
                    <Typography sx={{
                      fontFamily: 'JetBrains Mono', fontSize: 11, fontWeight: 700,
                      color: medalColor, textAlign: 'center',
                    }}>
                      {place}
                    </Typography>
                    <Avatar sx={{
                      width: 22, height: 22, fontSize: 11, fontWeight: 700,
                      bgcolor: TEAM_COLORS[p.color], color: '#000',
                    }}>{p.avatar}</Avatar>
                    <Typography variant="body2" fontWeight={place === 1 ? 700 : 500} noWrap>
                      {p.name}
                    </Typography>
                    <Typography sx={{
                      fontFamily: 'JetBrains Mono', fontSize: 10, color: 'text.disabled',
                    }}>
                      {s.wins}W
                    </Typography>
                    <Typography sx={{
                      fontFamily: 'JetBrains Mono', fontSize: 12, fontWeight: 700,
                      color: place === 1 ? league.color : 'text.primary',
                      minWidth: 36, textAlign: 'right',
                    }}>
                      {s.rating}
                    </Typography>
                  </Box>
                );
              })}
            </Stack>
          </Paper>
        </CardContent>
      </CardActionArea>
    </Card>
  );
}

window.LeagueHub = LeagueHub;
