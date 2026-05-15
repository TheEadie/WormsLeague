const {
  Box, Typography, Card, CardActionArea, CardContent,
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
  Paper, Chip, Avatar,
  ToggleButton, ToggleButtonGroup,
  TextField, InputAdornment, Icon,
  LinearProgress, Stack, Divider, Button,
} = window.MaterialUI;

// ── helpers ────────────────────────────────────────────────────────────────

function gameTopWeapons(g) {
  if (g.topWeapons) return g.topWeapons;
  return window.summarizeFeatured().topWeapons.slice(0, 3).map(([n]) => n);
}
function gameTotalDamage(g) {
  return g.totalDamage != null ? g.totalDamage : window.summarizeFeatured().totalDamage;
}
function gameTurnCount(g) {
  return g.turnCount != null ? g.turnCount : (g.turns ? g.turns.length : 0);
}

// ── Games list screen ──────────────────────────────────────────────────────

function GamesList({ density, listLayout, league, onOpen }) {
  const [sort,  setSort]  = React.useState('newest');
  const [query, setQuery] = React.useState('');

  const leagueDef = window.LEAGUE_DEFS[league] || Object.values(window.LEAGUE_DEFS)[0];
  const allGames  = window.gamesForLeague(leagueDef.id);

  const games = allGames.filter(g => {
    if (!query) return true;
    const q = query.toLowerCase();
    return g.id.includes(q) || g.map.toLowerCase().includes(q)
      || g.players.some(pid => window.PLAYERS[pid].name.toLowerCase().includes(q));
  });

  return (
    <Box className="fade-in" sx={{ p: density === 'compact' ? 2 : 3, maxWidth: 1400, mx: 'auto' }}>
      {/* Page header */}
      <Box sx={{ display: 'flex', alignItems: 'flex-end', justifyContent: 'space-between', mb: 3, flexWrap: 'wrap', gap: 2 }}>
        <Box>
          <Typography variant="overline" color="text.secondary">Matches</Typography>
          <Typography variant="h4" fontWeight={700} lineHeight={1.1}>{leagueDef.name}</Typography>
          <Typography variant="body2" color="text.secondary" mt={0.5}>
            {leagueDef.season} · {leagueDef.week} · {allGames.length} matches archived
          </Typography>
        </Box>
        <Stack direction="row" spacing={1}>
          <Button variant="outlined" size="small" startIcon={<Icon>upload</Icon>}>Upload replay</Button>
          <Button variant="contained" size="small" endIcon={<Icon>leaderboard</Icon>}>Leaderboard</Button>
        </Stack>
      </Box>

      {/* ELO standings */}
      <StandingsPanel league={leagueDef} density={density}/>

      {/* Filter bar */}
      <Paper variant="outlined" sx={{ p: 1.5, mb: 2, display: 'flex', gap: 1.5, alignItems: 'center', flexWrap: 'wrap' }}>
        <Chip label={leagueDef.short} size="small" sx={{ bgcolor: leagueDef.color + '22', color: leagueDef.color, border: `1px solid ${leagueDef.color}55`, fontWeight: 700, fontFamily: 'JetBrains Mono' }}/>

        <TextField
          size="small"
          placeholder="Search player, map, match ID…"
          value={query}
          onChange={e => setQuery(e.target.value)}
          sx={{ flex: 1, minWidth: 200 }}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <Icon sx={{ fontSize: 18, color: 'text.secondary' }}>search</Icon>
              </InputAdornment>
            ),
          }}
        />

        <ToggleButtonGroup value={sort} exclusive size="small" onChange={(_, v) => v && setSort(v)}>
          {[['newest','Newest'],['damage','Top damage'],['longest','Longest']].map(([v,l]) => (
            <ToggleButton key={v} value={v} sx={{ px: 1.5, fontSize: 12 }}>{l}</ToggleButton>
          ))}
        </ToggleButtonGroup>
      </Paper>

      {/* Layout variants */}
      {listLayout === 'cards' && <CardsLayout  games={games} density={density} onOpen={onOpen}/>}
      {listLayout === 'table' && <TableLayout  games={games} density={density} onOpen={onOpen}/>}
      {listLayout === 'feed'  && <FeedLayout   games={games} density={density} onOpen={onOpen}/>}
    </Box>
  );
}

// ── Card grid ──────────────────────────────────────────────────────────────

function CardsLayout({ games, density, onOpen }) {
  return (
    <Box sx={{ display: 'grid', gap: density === 'compact' ? 1 : 2, gridTemplateColumns: 'repeat(auto-fill, minmax(340px, 1fr))' }}>
      {games.map(g => <GameCard key={g.id} game={g} density={density} onOpen={onOpen}/>)}
    </Box>
  );
}

function GameCard({ game: g, density, onOpen }) {
  const cp = density === 'compact' ? 1.5 : 2.5;
  const positions = window.gamePositions(g);
  const { deltas } = window.computeLeagueElo(g.leagueId);
  const gameDelta = deltas[g.id] || {};

  return (
    <Card variant="outlined">
      <CardActionArea onClick={() => onOpen(g.id)}>
        <CardContent sx={{ p: cp, '&:last-child': { pb: cp } }}>
          {/* Header */}
          <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1.5, alignItems: 'center' }}>
            <Typography sx={{ fontFamily: 'JetBrains Mono', fontSize: 11, color: 'text.secondary' }}>
              #{g.id.replace('g-','')} · {g.map}
            </Typography>
            <Typography variant="caption" color="text.secondary">
              {window.fmtRelative(g.when)} · {window.fmtTime(g.when)}
            </Typography>
          </Box>

          {/* Finishing order */}
          <Box sx={{ display: 'grid', gap: 0.5, mb: 1.5 }}>
            {positions.map((pid, i) => {
              const place = i + 1;
              const p     = window.PLAYERS[pid];
              const d     = gameDelta[pid] ?? 0;
              const medal = ['#ffca28','#bdbdbd','#cd7f32'][i] || 'rgba(255,255,255,0.12)';
              return (
                <Box key={pid} sx={{ display: 'grid', gridTemplateColumns: '26px 18px 1fr auto', gap: 1, alignItems: 'center' }}>
                  <Box sx={{
                    width: 22, height: 22, borderRadius: '50%',
                    bgcolor: place <= 3 ? medal : 'transparent',
                    border: place <= 3 ? 'none' : '1px solid',
                    borderColor: 'divider',
                    display: 'grid', placeItems: 'center',
                    fontFamily: 'JetBrains Mono', fontSize: 10, fontWeight: 700,
                    color: place <= 3 ? '#000' : 'text.secondary',
                  }}>{place}</Box>
                  <Box sx={{ width: 10, height: 10, borderRadius: '2px', bgcolor: TEAM_COLORS[p.color] }}/>
                  <Typography sx={{ fontWeight: place === 1 ? 700 : 500, fontSize: 13, color: place === 1 ? 'warning.main' : 'text.primary' }} noWrap>
                    {p.name}
                  </Typography>
                  <Typography sx={{
                    fontFamily: 'JetBrains Mono', fontSize: 12, fontWeight: 700,
                    color: d > 0 ? 'success.main' : d < 0 ? 'error.main' : 'text.disabled',
                    minWidth: 40, textAlign: 'right',
                  }}>
                    {d > 0 ? '+' : ''}{d}
                  </Typography>
                </Box>
              );
            })}
          </Box>

          {/* Footer */}
          <Divider sx={{ mb: 1.5 }}/>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Stack direction="row" spacing={0.5}>
              {gameTopWeapons(g).slice(0,3).map(w => <WeaponIcon key={w} name={w}/>)}
            </Stack>
            <Typography sx={{ fontFamily: 'JetBrains Mono', fontSize: 11, color: 'text.secondary' }}>
              {window.fmtDuration(g.duration)} · {gameTurnCount(g)}t · {gameTotalDamage(g)} dmg
            </Typography>
          </Box>
        </CardContent>
      </CardActionArea>
    </Card>
  );
}

// ── Data table ─────────────────────────────────────────────────────────────

function TableLayout({ games, density, onOpen }) {
  return (
    <TableContainer component={Paper} variant="outlined">
      <Table size={density === 'compact' ? 'small' : 'medium'}>
        <TableHead>
          <TableRow>
            <TableCell sx={{ fontWeight: 700 }}>Match</TableCell>
            <TableCell sx={{ fontWeight: 700 }}>Date</TableCell>
            <TableCell sx={{ fontWeight: 700 }}>Finishing order</TableCell>
            <TableCell sx={{ fontWeight: 700 }}>Top weapons</TableCell>
            <TableCell sx={{ fontWeight: 700 }}>Length</TableCell>
            <TableCell sx={{ fontWeight: 700 }} align="right">ELO Δ</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {games.map(g => {
            const positions = window.gamePositions(g);
            const { deltas } = window.computeLeagueElo(g.leagueId);
            const gameDelta = deltas[g.id] || {};
            const winnerDelta = gameDelta[g.winner] ?? 0;
            return (
              <TableRow
                key={g.id}
                hover
                onClick={() => onOpen(g.id)}
                sx={{ cursor: 'pointer' }}
              >
                <TableCell>
                  <Typography sx={{ fontFamily: 'JetBrains Mono', fontSize: 12, color: 'text.secondary' }}>
                    #{g.id.replace('g-','')}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">{g.map}</Typography>
                </TableCell>
                <TableCell>
                  <Typography variant="body2">{window.fmtDate(g.when)}</Typography>
                  <Typography sx={{ fontFamily: 'JetBrains Mono', fontSize: 11, color: 'text.secondary' }}>{window.fmtTime(g.when)}</Typography>
                </TableCell>
                <TableCell>
                  <Stack direction="row" spacing={0.75} flexWrap="wrap" useFlexGap alignItems="center">
                    {positions.map((pid, i) => {
                      const p = window.PLAYERS[pid];
                      const place = i + 1;
                      const d = gameDelta[pid] ?? 0;
                      const medal = ['#ffca28','#bdbdbd','#cd7f32'][i];
                      return (
                        <Box key={pid} sx={{
                          display: 'flex', alignItems: 'center', gap: 0.5,
                          borderRadius: 99, pl: 0.25, pr: 0.75, py: 0.25,
                          border: '1px solid',
                          borderColor: place === 1 ? medal + '88' : 'divider',
                          bgcolor: place === 1 ? medal + '18' : 'transparent',
                        }}>
                          <Box sx={{
                            width: 18, height: 18, borderRadius: '50%',
                            bgcolor: place <= 3 ? medal : 'action.disabledBackground',
                            display: 'grid', placeItems: 'center',
                            fontFamily: 'JetBrains Mono', fontSize: 9, fontWeight: 700,
                            color: place <= 3 ? '#000' : 'text.secondary',
                          }}>{place}</Box>
                          <Box sx={{ width: 8, height: 8, borderRadius: '2px', bgcolor: TEAM_COLORS[p.color] }}/>
                          <Typography sx={{ fontSize: 12, fontWeight: place === 1 ? 700 : 500 }}>
                            {p.name}
                          </Typography>
                          <Typography sx={{
                            fontFamily: 'JetBrains Mono', fontSize: 10,
                            color: d > 0 ? 'success.main' : d < 0 ? 'error.main' : 'text.disabled',
                            ml: 0.25,
                          }}>
                            {d > 0 ? '+' : ''}{d}
                          </Typography>
                        </Box>
                      );
                    })}
                  </Stack>
                </TableCell>
                <TableCell>
                  <Stack direction="row" spacing={0.5}>
                    {gameTopWeapons(g).slice(0,4).map(w => <WeaponIcon key={w} name={w}/>)}
                  </Stack>
                </TableCell>
                <TableCell>
                  <Typography sx={{ fontFamily: 'JetBrains Mono', fontSize: 13 }}>{window.fmtDuration(g.duration)}</Typography>
                </TableCell>
                <TableCell align="right">
                  <Typography sx={{ fontFamily: 'JetBrains Mono', fontWeight: 700, color: 'success.main' }}>
                    +{winnerDelta}
                  </Typography>
                  <Typography sx={{ fontFamily: 'JetBrains Mono', fontSize: 10, color: 'text.disabled' }}>
                    {gameTotalDamage(g)} dmg
                  </Typography>
                </TableCell>
              </TableRow>
            );
          })}
        </TableBody>
      </Table>
    </TableContainer>
  );
}

// ── Activity feed ──────────────────────────────────────────────────────────

function FeedLayout({ games, density, onOpen }) {
  return (
    <Stack spacing={density === 'compact' ? 0.75 : 1.25}>
      {games.map(g => {
        const positions = window.gamePositions(g);
        const { deltas } = window.computeLeagueElo(g.leagueId);
        const gameDelta = deltas[g.id] || {};
        return (
          <Paper
            key={g.id}
            variant="outlined"
            sx={{ cursor: 'pointer', '&:hover': { borderColor: 'primary.main', bgcolor: 'action.hover' } }}
            onClick={() => onOpen(g.id)}
          >
            <Box sx={{
              display: 'grid',
              gridTemplateColumns: '90px 1fr auto',
              gap: 2, alignItems: 'center',
              p: density === 'compact' ? 1.25 : 2,
            }}>
              {/* Date/time */}
              <Box>
                <Typography sx={{ fontFamily: 'JetBrains Mono', fontWeight: 700, fontSize: 15 }}>{window.fmtDate(g.when)}</Typography>
                <Typography sx={{ fontFamily: 'JetBrains Mono', fontSize: 11, color: 'text.secondary' }}>{window.fmtTime(g.when)}</Typography>
              </Box>

              {/* Finishing order */}
              <Box>
                <Stack direction="row" spacing={0.75} alignItems="center" flexWrap="wrap" useFlexGap mb={0.75}>
                  {positions.map((pid, i) => {
                    const p = window.PLAYERS[pid];
                    const place = i + 1;
                    const d = gameDelta[pid] ?? 0;
                    const medal = ['#ffca28','#bdbdbd','#cd7f32'][i];
                    return (
                      <Box key={pid} sx={{
                        display: 'inline-flex', alignItems: 'center', gap: 0.5,
                        pl: 0.25, pr: 0.75, py: 0.25, borderRadius: 99,
                        bgcolor: place === 1 ? medal + '22' : 'transparent',
                        border: '1px solid',
                        borderColor: place === 1 ? medal + '88' : 'divider',
                      }}>
                        <Box sx={{
                          width: 16, height: 16, borderRadius: '50%',
                          bgcolor: place <= 3 ? medal : 'action.disabledBackground',
                          display: 'grid', placeItems: 'center',
                          fontFamily: 'JetBrains Mono', fontSize: 9, fontWeight: 700, color: '#000',
                        }}>{place}</Box>
                        <Box sx={{ width: 8, height: 8, borderRadius: '2px', bgcolor: TEAM_COLORS[p.color] }}/>
                        <Typography sx={{ fontSize: 12, fontWeight: place === 1 ? 700 : 500 }}>{p.name}</Typography>
                        <Typography sx={{
                          fontFamily: 'JetBrains Mono', fontSize: 10,
                          color: d > 0 ? 'success.main' : d < 0 ? 'error.main' : 'text.disabled',
                        }}>
                          {d > 0 ? '+' : ''}{d}
                        </Typography>
                      </Box>
                    );
                  })}
                </Stack>
                <Typography variant="caption" color="text.secondary" sx={{ fontFamily: 'JetBrains Mono' }}>
                  {g.map} · {window.fmtDuration(g.duration)} · {gameTurnCount(g)} turns · {gameTotalDamage(g)} dmg · #{g.id.replace('g-','')}
                </Typography>
              </Box>

              {/* Weapons + arrow */}
              <Stack direction="row" spacing={0.5} alignItems="center">
                {gameTopWeapons(g).slice(0,3).map(w => <WeaponIcon key={w} name={w}/>)}
                <Icon sx={{ color: 'text.secondary', ml: 1 }}>chevron_right</Icon>
              </Stack>
            </Box>
          </Paper>
        );
      })}
    </Stack>
  );
}

// ── ELO Standings ──────────────────────────────────────────────────────────

function StandingsPanel({ league, density }) {
  const { standings } = window.computeLeagueElo(league.id);
  const cp = density === 'compact' ? 1.5 : 2;
  const maxRating = Math.max(...standings.map(s => s.rating));
  const minRating = Math.min(...standings.map(s => s.rating));
  const range = Math.max(maxRating - minRating, 1);

  return (
    <Paper variant="outlined" sx={{ mb: 2, overflow: 'hidden' }}>
      <Box sx={{
        display: 'flex', alignItems: 'center', gap: 1.5,
        px: cp, py: 1.25,
        borderBottom: 1, borderColor: 'divider',
        bgcolor: league.color + '10',
      }}>
        <Icon sx={{ fontSize: 18, color: league.color }}>leaderboard</Icon>
        <Typography variant="overline" sx={{ color: league.color, fontWeight: 700, letterSpacing: '0.12em' }}>
          ELO Standings
        </Typography>
        <Typography variant="caption" color="text.secondary" sx={{ ml: 0.5 }}>
          seeded at 1000 · K=32 · pairwise
        </Typography>
        <Box sx={{ flex: 1 }}/>
        <Typography sx={{ fontFamily: 'JetBrains Mono', fontSize: 11, color: 'text.secondary' }}>
          {standings.length} players · {window.gamesForLeague(league.id).length} matches
        </Typography>
      </Box>
      <Table size={density === 'compact' ? 'small' : 'medium'}>
        <TableHead>
          <TableRow>
            <TableCell sx={{ fontWeight: 700, width: 56 }}>#</TableCell>
            <TableCell sx={{ fontWeight: 700 }}>Player</TableCell>
            <TableCell sx={{ fontWeight: 700 }} align="right">Rating</TableCell>
            <TableCell sx={{ fontWeight: 700, width: 70 }} align="right">Last</TableCell>
            <TableCell sx={{ fontWeight: 700, width: 70 }} align="right">Peak</TableCell>
            <TableCell sx={{ fontWeight: 700, width: 50 }} align="right">W</TableCell>
            <TableCell sx={{ fontWeight: 700, width: 50 }} align="right">G</TableCell>
            <TableCell sx={{ fontWeight: 700, width: 160 }}>Form (L5)</TableCell>
            <TableCell sx={{ fontWeight: 700, minWidth: 140 }}>Range</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {standings.map((s, i) => {
            const p = window.PLAYERS[s.playerId];
            const place = i + 1;
            const medalColor = ['#ffca28', '#bdbdbd', '#cd7f32'][i];
            const pct = ((s.rating - minRating) / range) * 100;
            return (
              <TableRow key={s.playerId} sx={{ '&:last-child td': { borderBottom: 0 } }}>
                <TableCell>
                  <Box sx={{
                    width: 24, height: 24, borderRadius: '50%',
                    bgcolor: place <= 3 ? medalColor : 'transparent',
                    border: place <= 3 ? 'none' : '1px solid',
                    borderColor: 'divider',
                    display: 'grid', placeItems: 'center',
                    fontFamily: 'JetBrains Mono', fontSize: 12, fontWeight: 700,
                    color: place <= 3 ? '#000' : 'text.secondary',
                  }}>{place}</Box>
                </TableCell>
                <TableCell>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.25 }}>
                    <PlayerAvatar playerId={s.playerId} size={28}/>
                    <Box sx={{ minWidth: 0 }}>
                      <Typography variant="body2" fontWeight={place === 1 ? 700 : 600} lineHeight={1.15} noWrap>
                        {p.name}
                      </Typography>
                      <Typography variant="caption" color="text.secondary" sx={{ fontSize: 11 }} noWrap>
                        {p.team}
                      </Typography>
                    </Box>
                  </Box>
                </TableCell>
                <TableCell align="right">
                  <Typography sx={{
                    fontFamily: 'JetBrains Mono', fontWeight: 700, fontSize: 16,
                    color: place === 1 ? league.color : 'text.primary',
                  }}>
                    {s.rating}
                  </Typography>
                </TableCell>
                <TableCell align="right">
                  <Typography sx={{
                    fontFamily: 'JetBrains Mono', fontSize: 12, fontWeight: 700,
                    color: s.lastDelta > 0 ? 'success.main'
                         : s.lastDelta < 0 ? 'error.main' : 'text.disabled',
                  }}>
                    {s.lastDelta > 0 ? '▲ +' : s.lastDelta < 0 ? '▼ ' : '— '}
                    {s.lastDelta !== 0 ? s.lastDelta : ''}
                  </Typography>
                </TableCell>
                <TableCell align="right">
                  <Typography sx={{ fontFamily: 'JetBrains Mono', fontSize: 12, color: 'text.secondary' }}>
                    {s.peak}
                  </Typography>
                </TableCell>
                <TableCell align="right">
                  <Typography sx={{ fontFamily: 'JetBrains Mono', fontSize: 12, color: 'text.primary' }}>
                    {s.wins}
                  </Typography>
                </TableCell>
                <TableCell align="right">
                  <Typography sx={{ fontFamily: 'JetBrains Mono', fontSize: 12, color: 'text.secondary' }}>
                    {s.games}
                  </Typography>
                </TableCell>
                <TableCell>
                  <Stack direction="row" spacing={0.25}>
                    {Array.from({ length: 5 }).map((_, idx) => {
                      const d = s.last5[s.last5.length - 5 + idx];
                      if (d == null) {
                        return <Box key={idx} sx={{ width: 22, height: 16, borderRadius: 0.5, bgcolor: 'action.disabledBackground' }}/>;
                      }
                      const mag = Math.min(Math.abs(d) / 16, 1);
                      return (
                        <Box key={idx} sx={{
                          width: 22, height: 16, borderRadius: 0.5,
                          bgcolor: d > 0 ? `rgba(102,187,106,${0.25 + mag * 0.6})`
                                : d < 0 ? `rgba(239,83,80,${0.25 + mag * 0.6})`
                                : 'action.disabledBackground',
                          display: 'grid', placeItems: 'center',
                        }}>
                          <Typography sx={{ fontFamily: 'JetBrains Mono', fontSize: 9, fontWeight: 700, color: d !== 0 ? '#000' : 'text.disabled' }}>
                            {d > 0 ? '+' : ''}{d}
                          </Typography>
                        </Box>
                      );
                    })}
                  </Stack>
                </TableCell>
                <TableCell>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <Box sx={{
                      flex: 1, height: 4, borderRadius: 99,
                      bgcolor: 'action.disabledBackground', overflow: 'hidden',
                      minWidth: 60,
                    }}>
                      <Box sx={{
                        width: `${Math.max(pct, 3)}%`, height: '100%',
                        bgcolor: place === 1 ? league.color : TEAM_COLORS[p.color],
                      }}/>
                    </Box>
                    <Typography sx={{ fontFamily: 'JetBrains Mono', fontSize: 10, color: 'text.disabled', minWidth: 28, textAlign: 'right' }}>
                      {Math.round(pct)}%
                    </Typography>
                  </Box>
                </TableCell>
              </TableRow>
            );
          })}
        </TableBody>
      </Table>
    </Paper>
  );
}

window.GamesList = GamesList;
