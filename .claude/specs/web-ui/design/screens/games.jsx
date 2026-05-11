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
  const maxDmg = Math.max(...Object.values(g.finals || {}), 1);
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

          {/* Players */}
          <Box sx={{ display: 'grid', gap: 0.75, mb: 1.5 }}>
            {g.players.map(pid => {
              const isWin = pid === g.winner;
              const dmg   = g.finals?.[pid] ?? 0;
              const pct   = Math.max(5, (dmg / maxDmg) * 100);
              const p     = window.PLAYERS[pid];
              return (
                <Box key={pid} sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  {isWin
                    ? <Chip label="♛" size="small" color="warning" sx={{ height: 20, fontSize: 10, fontWeight: 700, minWidth: 30 }}/>
                    : <Box sx={{ width: 30 }}/>
                  }
                  <Typography sx={{ fontWeight: isWin ? 700 : 400, fontSize: 13, width: 80, flexShrink: 0, color: isWin ? 'warning.main' : 'text.primary' }}>
                    {p.name}
                  </Typography>
                  <LinearProgress
                    variant="determinate"
                    value={pct}
                    sx={{
                      flex: 1, height: 5, borderRadius: 99,
                      bgcolor: 'action.disabledBackground',
                      '& .MuiLinearProgress-bar': { bgcolor: isWin ? 'warning.main' : TEAM_COLORS[p.color] },
                    }}
                  />
                  <Typography sx={{ fontFamily: 'JetBrains Mono', fontSize: 11, color: isWin ? 'warning.main' : 'text.secondary', minWidth: 32, textAlign: 'right' }}>
                    {dmg}
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
            <TableCell sx={{ fontWeight: 700 }}>Players</TableCell>
            <TableCell sx={{ fontWeight: 700 }}>Top weapons</TableCell>
            <TableCell sx={{ fontWeight: 700 }}>Length</TableCell>
            <TableCell sx={{ fontWeight: 700 }} align="right">Damage</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {games.map(g => (
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
                <Stack direction="row" spacing={0.5} flexWrap="wrap" useFlexGap>
                  {g.players.map(pid => (
                    <Box key={pid} sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                      {pid === g.winner && <Icon sx={{ fontSize: 14, color: 'warning.main' }}>workspace_premium</Icon>}
                      <TeamBadge playerId={pid}/>
                    </Box>
                  ))}
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
                <Typography sx={{ fontFamily: 'JetBrains Mono', fontWeight: 700, color: 'primary.main' }}>
                  {gameTotalDamage(g)}
                </Typography>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
}

// ── Activity feed ──────────────────────────────────────────────────────────

function FeedLayout({ games, density, onOpen }) {
  return (
    <Stack spacing={density === 'compact' ? 0.75 : 1.25}>
      {games.map(g => (
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

            {/* Players + meta */}
            <Box>
              <Stack direction="row" spacing={0.75} alignItems="center" flexWrap="wrap" useFlexGap mb={0.75}>
                <Chip label="♛" size="small" color="warning" sx={{ height: 18, fontSize: 10, fontWeight: 700 }}/>
                <TeamBadge playerId={g.winner}/>
                <Typography variant="caption" color="text.secondary" sx={{ letterSpacing: '0.14em', textTransform: 'uppercase' }}>defeated</Typography>
                {g.players.filter(p => p !== g.winner).map(pid => (
                  <TeamBadge key={pid} playerId={pid}/>
                ))}
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
      ))}
    </Stack>
  );
}

window.GamesList = GamesList;
