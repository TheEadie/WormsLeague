const {
  Box, Typography, Paper, Stack, Chip, Icon, Button,
  Tabs, Tab, Breadcrumbs, Link, Divider,
  List, ListItem, ListItemButton, ListItemIcon, ListItemText,
} = window.MaterialUI;

function GameDetail({ gameId, density, detailLayout, onBack }) {
  const game    = window.GAMES.find(g => g.id === gameId) || window.GAMES[0];
  const summary = game.featured ? window.summarizeFeatured() : null;
  const [tab, setTab] = React.useState(0);

  const panels = [
    { label: 'Turn-by-turn', icon: 'timeline',   content: <TimelinePanel game={game}/> },
    { label: 'Weapons',      icon: 'gps_fixed',  content: <WeaponsPanel  game={game} summary={summary}/> },
    { label: 'Damage chart', icon: 'show_chart',  content: <DamagePanel   game={game}/> },
    { label: 'Head-to-head', icon: 'people',      content: <H2HPanel      game={game}/> },
    { label: 'Replay',       icon: 'download',    content: <ReplayPanel   game={game}/> },
  ];

  return (
    <Box className="fade-in" sx={{ p: density === 'compact' ? 2 : 3, maxWidth: 1400, mx: 'auto' }}>
      {/* Breadcrumb */}
      <Breadcrumbs sx={{ mb: 2 }}>
        <Link component="button" variant="body2" color="inherit" underline="hover" onClick={onBack}>
          Matches
        </Link>
        <Typography variant="body2" color="text.primary">
          #{game.id.replace('g-','')} · {game.map}
        </Typography>
      </Breadcrumbs>

      {/* Hero card */}
      <DetailHero game={game} summary={summary}/>

      {/* Content by layout variant */}
      {detailLayout === 'tabs'    && <TabsLayout    panels={panels} tab={tab} setTab={setTab}/>}
      {detailLayout === 'stack'   && <StackLayout   panels={panels}/>}
      {detailLayout === 'sidebar' && <SidebarLayout panels={panels} tab={tab} setTab={setTab}/>}
    </Box>
  );
}

function DetailHero({ game, summary }) {
  const totalDmg   = summary ? summary.totalDamage : (game.totalDamage || 0);
  const turnCount  = summary ? summary.turnCount   : (game.turnCount || 0);
  const kills      = summary
    ? game.turns.reduce((acc, t) => acc + Object.values(t.kills || {}).reduce((a,b)=>a+b,0), 0)
    : (game.players.length - 1) * 4;

  return (
    <Paper variant="outlined" sx={{ p: 3, mb: 2 }}>
      {/* Title row */}
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2, flexWrap: 'wrap' }}>
        <Typography variant="h5" fontWeight={700}>{game.map}</Typography>
        <Typography sx={{ fontFamily: 'JetBrains Mono', fontSize: 12, color: 'text.secondary' }}>
          {window.fmtDate(game.when)} · {window.fmtTime(game.when)}
        </Typography>
        <Chip
          label={`♛ ${window.PLAYERS[game.winner].name} won`}
          color="warning"
          size="small"
          sx={{ ml: 'auto', fontWeight: 700 }}
        />
      </Box>

      {/* Player roster */}
      <Stack direction="row" spacing={1} mb={2.5} flexWrap="wrap" useFlexGap>
        {game.players.map(pid => (
          <Paper
            key={pid}
            variant={pid === game.winner ? 'elevation' : 'outlined'}
            elevation={pid === game.winner ? 3 : 0}
            sx={{
              display: 'flex', alignItems: 'center', gap: 1,
              px: 2, py: 0.75, borderRadius: 99,
              ...(pid === game.winner
                ? { border: '1px solid', borderColor: 'warning.main', bgcolor: 'warning.main', opacity: 0.9 }
                : {}),
            }}
          >
            <PlayerAvatar playerId={pid} size={24}/>
            <Typography fontWeight={600} fontSize={13} color={pid === game.winner ? '#000' : 'text.primary'}>
              {window.PLAYERS[pid].name}
            </Typography>
            <Typography fontSize={11} color={pid === game.winner ? 'rgba(0,0,0,0.6)' : 'text.secondary'}>
              {window.PLAYERS[pid].team}
            </Typography>
            {pid === game.winner && <Icon sx={{ fontSize: 16, color: '#000' }}>workspace_premium</Icon>}
          </Paper>
        ))}
      </Stack>

      {/* Stats strip */}
      <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: 1.5 }}>
        {[
          ['Duration',     window.fmtDuration(game.duration), 'text.primary'],
          ['Turns',        turnCount,                          'text.primary'],
          ['Total damage', totalDmg,                          'primary.main'],
          ['Kills',        kills,                             'error.main'],
        ].map(([label, val, color]) => (
          <Paper key={label} variant="outlined" sx={{ p: 1.5 }}>
            <Typography variant="caption" color="text.secondary" display="block" sx={{ letterSpacing: '0.12em', textTransform: 'uppercase' }}>
              {label}
            </Typography>
            <Typography sx={{ fontFamily: 'JetBrains Mono', fontWeight: 700, fontSize: 22, color, mt: 0.25 }}>
              {val}
            </Typography>
          </Paper>
        ))}
      </Box>
    </Paper>
  );
}

function TabsLayout({ panels, tab, setTab }) {
  return (
    <Box>
      <Tabs value={tab} onChange={(_, v) => setTab(v)} sx={{ borderBottom: 1, borderColor: 'divider', mb: 2 }}>
        {panels.map((p, i) => (
          <Tab key={i} label={p.label} icon={<Icon sx={{ fontSize: 16 }}>{p.icon}</Icon>}
            iconPosition="start" sx={{ minHeight: 44, fontSize: 13, textTransform: 'none' }}/>
        ))}
      </Tabs>
      {panels[tab].content}
    </Box>
  );
}

function StackLayout({ panels }) {
  return (
    <Stack spacing={3}>
      {panels.map((p, i) => (
        <Box key={i}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1.5 }}>
            <Icon color="primary" sx={{ fontSize: 18 }}>{p.icon}</Icon>
            <Typography variant="h6" fontWeight={700}>{p.label}</Typography>
          </Box>
          {p.content}
          {i < panels.length - 1 && <Divider sx={{ mt: 3 }}/>}
        </Box>
      ))}
    </Stack>
  );
}

function SidebarLayout({ panels, tab, setTab }) {
  return (
    <Box sx={{ display: 'grid', gridTemplateColumns: '220px 1fr', gap: 2, alignItems: 'start' }}>
      <Paper variant="outlined" sx={{ p: 0.75, position: 'sticky', top: 76 }}>
        <List dense disablePadding>
          {panels.map((p, i) => (
            <ListItemButton
              key={i}
              selected={tab === i}
              onClick={() => setTab(i)}
              sx={{ borderRadius: 1, mb: 0.25 }}
            >
              <ListItemIcon sx={{ minWidth: 32 }}>
                <Icon sx={{ fontSize: 18 }}>{p.icon}</Icon>
              </ListItemIcon>
              <ListItemText primary={p.label} primaryTypographyProps={{ fontSize: 13, fontWeight: tab === i ? 700 : 400 }}/>
            </ListItemButton>
          ))}
        </List>
      </Paper>
      <Box>{panels[tab].content}</Box>
    </Box>
  );
}

window.GameDetail = GameDetail;
