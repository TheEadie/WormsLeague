// Detail panel components: Timeline, Weapons, Damage chart, H2H, Replay

const {
  Box, Typography, Paper, Stack, Chip, Icon,
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
  LinearProgress, Button, Divider,
} = window.MaterialUI;

// ── Turn-by-turn timeline ─────────────────────────────────────────────────

function TimelinePanel({ game }) {
  const [filter, setFilter] = React.useState('all');

  if (!game.turns) {
    return (
      <Paper variant="outlined" sx={{ p: 3, textAlign: 'center' }}>
        <Icon sx={{ fontSize: 40, color: 'text.disabled', mb: 1 }}>timeline</Icon>
        <Typography color="text.secondary">
          Per-turn data is only available for the featured match.<br/>
          Open <Box component="span" sx={{ fontFamily: 'JetBrains Mono', color: 'primary.main' }}>#0042</Box> to see the full breakdown.
        </Typography>
      </Paper>
    );
  }

  const turns = game.turns.filter(t => {
    if (filter === 'kills')  return t.kills && Object.values(t.kills).some(k => k > 0);
    if (filter === 'damage') return Object.values(t.damage || {}).some(d => d > 0);
    return true;
  });

  return (
    <Box>
      <Stack direction="row" spacing={1} mb={2}>
        {[['all','All turns'],['kills','Kills only'],['damage','Damage only']].map(([v,l]) => (
          <Chip key={v} label={l} size="small" variant={filter === v ? 'filled' : 'outlined'}
            color={filter === v ? 'primary' : 'default'} onClick={() => setFilter(v)} clickable/>
        ))}
      </Stack>
      <TableContainer component={Paper} variant="outlined">
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell sx={{ fontWeight: 700, width: 48 }}>#</TableCell>
              <TableCell sx={{ fontWeight: 700 }}>Team</TableCell>
              <TableCell sx={{ fontWeight: 700 }}>Weapons used</TableCell>
              <TableCell sx={{ fontWeight: 700 }}>Damage dealt</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {turns.map(t => {
              const p = Object.values(window.PLAYERS).find(p => p.team === t.team);
              const hasKill = t.kills && Object.values(t.kills).some(k => k > 0);
              return (
                <TableRow
                  key={t.num}
                  sx={{ bgcolor: hasKill ? 'error.main' : 'transparent',
                        '&.MuiTableRow-root': { bgcolor: hasKill ? 'rgba(211,47,47,0.08)' : 'transparent' } }}
                >
                  <TableCell sx={{ fontFamily: 'JetBrains Mono', fontSize: 12, color: 'text.secondary' }}>
                    {String(t.num).padStart(2,'0')}
                  </TableCell>
                  <TableCell>
                    <Chip
                      size="small"
                      label={p?.name || t.team}
                      avatar={
                        <Box component="span" sx={{
                          width: 12, height: 12, borderRadius: '3px',
                          bgcolor: TEAM_COLORS[p?.color] || '#888',
                          display: 'inline-block', ml: '6px !important',
                        }}/>
                      }
                      sx={{ fontWeight: 600 }}
                    />
                  </TableCell>
                  <TableCell>
                    <Stack direction="row" spacing={0.5} flexWrap="wrap" useFlexGap>
                      {t.weapons.map((w, i) => <WeaponIcon key={i} name={w}/>)}
                    </Stack>
                  </TableCell>
                  <TableCell>
                    {Object.entries(t.damage || {}).length === 0 ? (
                      <Typography variant="caption" color="text.disabled">— no damage —</Typography>
                    ) : (
                      <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
                        {Object.entries(t.damage).map(([tm, d]) => {
                          const tp = Object.values(window.PLAYERS).find(p => p.team === tm);
                          const k = t.kills?.[tm];
                          return (
                            <Box key={tm} sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                              <Typography sx={{ fontFamily: 'JetBrains Mono', fontSize: 12, color: 'text.secondary' }}>
                                {tp?.name || tm}:
                              </Typography>
                              <Typography sx={{ fontFamily: 'JetBrains Mono', fontSize: 12, fontWeight: 700, color: 'primary.light' }}>
                                {d}
                              </Typography>
                              {k ? (
                                <Chip label={`+${k} kill`} size="small" color="error"
                                  sx={{ height: 16, fontSize: 9, fontWeight: 700 }}/>
                              ) : null}
                            </Box>
                          );
                        })}
                      </Stack>
                    )}
                  </TableCell>
                </TableRow>
              );
            })}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  );
}

// ── Weapon usage ──────────────────────────────────────────────────────────

function WeaponsPanel({ game, summary }) {
  const data = summary
    ? Object.entries(summary.weaponCount).sort((a, b) => b[1] - a[1])
    : (game.topWeapons || []).map(w => [w, Math.floor(2 + Math.random() * 5)]);
  const maxCount = Math.max(...data.map(d => d[1]), 1);

  return (
    <Box>
      <Typography variant="caption" color="text.secondary" mb={2} display="block">
        {data.length} unique weapons · usage frequency
      </Typography>
      <Box sx={{ display: 'grid', gap: 1, gridTemplateColumns: 'repeat(auto-fill, minmax(200px, 1fr))' }}>
        {data.map(([w, c]) => {
          const wi = window.WEAPONS[w] || window.WEAPON_DEFAULT;
          return (
            <Paper key={w} variant="outlined" sx={{ p: 1.5, display: 'flex', gap: 1.5, alignItems: 'flex-start' }}>
              <WeaponIcon name={w} size="lg"/>
              <Box sx={{ flex: 1, minWidth: 0 }}>
                <Typography variant="body2" fontWeight={600} noWrap>{w}</Typography>
                <Typography sx={{ fontFamily: 'JetBrains Mono', fontSize: 11, color: 'text.secondary' }}>
                  {c} use{c !== 1 ? 's' : ''}
                </Typography>
                <LinearProgress
                  variant="determinate"
                  value={(c / maxCount) * 100}
                  sx={{
                    mt: 0.75, height: 4, borderRadius: 99,
                    bgcolor: 'action.disabledBackground',
                    '& .MuiLinearProgress-bar': { bgcolor: wi.color },
                  }}
                />
              </Box>
            </Paper>
          );
        })}
      </Box>
    </Box>
  );
}

// ── Cumulative damage chart ────────────────────────────────────────────────

function DamagePanel({ game }) {
  if (!game.turns) {
    return (
      <Paper variant="outlined" sx={{ p: 3, textAlign: 'center' }}>
        <Typography color="text.secondary">Per-turn damage not recorded for this match in the prototype.</Typography>
      </Paper>
    );
  }

  const teams = [...new Set(game.turns.map(t => t.team))];
  const series = teams.map(tm => {
    let total = 0;
    const cum = game.turns.map(t => {
      if (t.team === tm) {
        Object.entries(t.damage || {}).forEach(([target, d]) => { if (target !== tm) total += d; });
      }
      return total;
    });
    return { team: tm, cum };
  });

  const W = 700, H = 240, P = 36;
  const xs  = game.turns.length;
  const yMax = Math.max(...series.flatMap(s => s.cum), 100);

  const colorForTeam = tm => {
    const p = Object.values(window.PLAYERS).find(p => p.team === tm);
    return TEAM_COLORS[p?.color] || '#888';
  };

  return (
    <Box>
      <Typography variant="caption" color="text.secondary" mb={2} display="block">
        Cumulative damage dealt per team · by turn · excludes self-damage
      </Typography>
      <Paper variant="outlined" sx={{ p: 2 }}>
        <svg viewBox={`0 0 ${W} ${H}`} style={{ width: '100%', height: 'auto', display: 'block' }}>
          {/* grid lines */}
          {[0, 0.25, 0.5, 0.75, 1].map(f => (
            <g key={f}>
              <line x1={P} x2={W - P} y1={P + (H - 2*P)*f} y2={P + (H - 2*P)*f}
                stroke="rgba(255,255,255,0.06)" strokeDasharray="3 5"/>
              <text x={P - 8} y={P + (H - 2*P)*(1 - f) + 4}
                fontSize="9" fontFamily="JetBrains Mono" fill="rgba(255,255,255,0.3)" textAnchor="end">
                {Math.round(yMax * f)}
              </text>
            </g>
          ))}
          {/* x axis labels */}
          {[1, Math.floor(xs/2), xs].map(n => (
            <text key={n} x={P + (W - 2*P)*((n - 1)/(xs - 1 || 1))} y={H - 10}
              fontSize="9" fontFamily="JetBrains Mono" fill="rgba(255,255,255,0.3)" textAnchor="middle">
              T{n}
            </text>
          ))}
          {/* series lines */}
          {series.map(s => {
            const pts = s.cum.map((v, i) => {
              const x = P + (W - 2*P)*(i/(xs - 1 || 1));
              const y = P + (H - 2*P)*(1 - v/yMax);
              return `${x},${y}`;
            }).join(' ');
            const endX = P + (W - 2*P);
            const endY = P + (H - 2*P)*(1 - s.cum[s.cum.length - 1]/yMax);
            return (
              <g key={s.team}>
                <polyline points={pts} fill="none" stroke={colorForTeam(s.team)}
                  strokeWidth="2.5" strokeLinejoin="round" strokeLinecap="round"/>
                <circle cx={endX} cy={endY} r="4" fill={colorForTeam(s.team)}/>
              </g>
            );
          })}
        </svg>

        {/* Legend */}
        <Stack direction="row" spacing={2} mt={1} flexWrap="wrap" useFlexGap>
          {series.map(s => {
            const p = Object.values(window.PLAYERS).find(p => p.team === s.team);
            return (
              <Box key={s.team} sx={{ display: 'flex', alignItems: 'center', gap: 0.75 }}>
                <Box sx={{ width: 10, height: 10, borderRadius: '2px', bgcolor: colorForTeam(s.team) }}/>
                <Typography variant="caption">{p?.name || s.team}</Typography>
                <Typography sx={{ fontFamily: 'JetBrains Mono', fontSize: 11, color: 'text.secondary' }}>
                  {s.cum[s.cum.length - 1]}
                </Typography>
              </Box>
            );
          })}
        </Stack>
      </Paper>
    </Box>
  );
}

// ── Head-to-head ──────────────────────────────────────────────────────────

function H2HPanel({ game }) {
  const { players } = game;
  const pairs = [];
  for (let i = 0; i < players.length; i++)
    for (let j = i + 1; j < players.length; j++)
      pairs.push([players[i], players[j]]);

  const h2h = pairs.map(([a, b]) => {
    let aw = 0, bw = 0, met = 0;
    window.GAMES.forEach(g => {
      if (!g.players.includes(a) || !g.players.includes(b)) return;
      met++;
      if (g.winner === a) aw++;
      else if (g.winner === b) bw++;
    });
    return { a, b, aw, bw, met };
  });

  return (
    <Stack spacing={1.5}>
      <Typography variant="caption" color="text.secondary">
        Across all {window.GAMES.length} archived matches
      </Typography>
      {h2h.map(({ a, b, aw, bw, met }) => {
        const tot = Math.max(aw + bw, 1);
        return (
          <Paper key={a + b} variant="outlined" sx={{ p: 2 }}>
            <Box sx={{ display: 'grid', gridTemplateColumns: '1fr auto 1fr', gap: 2, alignItems: 'center' }}>
              {/* Player A */}
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
                <PlayerAvatar playerId={a} size={36}/>
                <Box>
                  <Typography fontWeight={600}>{window.PLAYERS[a].name}</Typography>
                  <Typography variant="caption" color="text.secondary">{window.PLAYERS[a].team}</Typography>
                </Box>
              </Box>

              {/* Score */}
              <Box sx={{ textAlign: 'center', minWidth: 120 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 1 }}>
                  <Typography sx={{ fontFamily: 'JetBrains Mono', fontSize: 26, fontWeight: 700, color: aw >= bw ? 'warning.main' : 'text.primary' }}>{aw}</Typography>
                  <Typography variant="caption" color="text.secondary" sx={{ letterSpacing: '0.14em' }}>VS</Typography>
                  <Typography sx={{ fontFamily: 'JetBrains Mono', fontSize: 26, fontWeight: 700, color: bw > aw ? 'warning.main' : 'text.primary' }}>{bw}</Typography>
                </Box>
                <LinearProgress
                  variant="determinate"
                  value={(aw / tot) * 100}
                  sx={{
                    height: 5, borderRadius: 99,
                    bgcolor: TEAM_COLORS[window.PLAYERS[b].color] + '55',
                    '& .MuiLinearProgress-bar': { bgcolor: TEAM_COLORS[window.PLAYERS[a].color] },
                  }}
                />
                <Typography variant="caption" color="text.secondary" sx={{ letterSpacing: '0.1em', textTransform: 'uppercase' }}>
                  {met} match{met !== 1 ? 'es' : ''}
                </Typography>
              </Box>

              {/* Player B */}
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, justifyContent: 'flex-end' }}>
                <Box sx={{ textAlign: 'right' }}>
                  <Typography fontWeight={600}>{window.PLAYERS[b].name}</Typography>
                  <Typography variant="caption" color="text.secondary">{window.PLAYERS[b].team}</Typography>
                </Box>
                <PlayerAvatar playerId={b} size={36}/>
              </Box>
            </Box>
          </Paper>
        );
      })}
    </Stack>
  );
}

// ── Replay download ────────────────────────────────────────────────────────

function ReplayPanel({ game }) {
  return (
    <Stack spacing={1.5}>
      <Paper variant="outlined" sx={{ p: 2, display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 2 }}>
        <Box>
          <Typography sx={{ fontFamily: 'JetBrains Mono', fontSize: 13 }}>
            {game.id}__{game.map.toLowerCase()}.WAgame
          </Typography>
          <Typography variant="caption" color="text.secondary">
            {game.replaySize || '~380 KB'} · uploaded {window.fmtRelative(game.when)} · sha256 verified
          </Typography>
        </Box>
        <Chip label="✓ Verified" color="success" size="small" variant="outlined"/>
        <Button variant="contained" size="small" startIcon={<Icon>download</Icon>}>
          Download .WAgame
        </Button>
      </Paper>

      <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 1.5 }}>
        <Paper variant="outlined" sx={{ p: 2, display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 1 }}>
          <Box>
            <Typography variant="body2" fontWeight={600}>Open in W:A</Typography>
            <Typography variant="caption" color="text.secondary">Launches your local client</Typography>
          </Box>
          <Button variant="outlined" size="small" endIcon={<Icon>open_in_new</Icon>}>Launch</Button>
        </Paper>
        <Paper variant="outlined" sx={{ p: 2, display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 1 }}>
          <Box>
            <Typography variant="body2" fontWeight={600}>Share match link</Typography>
            <Typography sx={{ fontFamily: 'JetBrains Mono', fontSize: 11, color: 'text.secondary' }}>
              worms.hub/m/{game.id.replace('g-','')}
            </Typography>
          </Box>
          <Button variant="outlined" size="small" startIcon={<Icon>content_copy</Icon>}>Copy</Button>
        </Paper>
      </Box>
    </Stack>
  );
}

Object.assign(window, { TimelinePanel, WeaponsPanel, DamagePanel, H2HPanel, ReplayPanel });
