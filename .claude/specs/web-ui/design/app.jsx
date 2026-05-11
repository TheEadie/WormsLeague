const {
  ThemeProvider, createTheme, CssBaseline,
  AppBar, Toolbar, Box, Typography, Button, Icon,
  Drawer, Fab, Divider, Stack,
  ToggleButton, ToggleButtonGroup, Select, MenuItem,
  FormControl, Breadcrumbs, Link, Chip, Menu,
  ListItemIcon, ListItemText,
} = window.MaterialUI;

const darkTheme = createTheme({ palette: { mode: 'dark' } });

// ── Tweaks state + host protocol ──────────────────────────────────────────

function useTweakState(defaults) {
  const [tweaks, setTweaksState] = React.useState(defaults);
  const setTweak = React.useCallback((key, val) => {
    setTweaksState(prev => {
      const next = { ...prev, [key]: val };
      try { window.parent.postMessage({ type: '__edit_mode_set_keys', edits: { [key]: val } }, '*'); } catch(e) {}
      return next;
    });
  }, []);
  return [tweaks, setTweak];
}

// ── Tweaks drawer ─────────────────────────────────────────────────────────

function TweaksDrawer({ open, onClose, tweaks, setTweak, onJump }) {
  return (
    <Drawer anchor="right" open={open} onClose={onClose}
      PaperProps={{ sx: { width: 280, p: 2 } }}>
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1 }}>
        <Typography variant="h6" fontWeight={700}>Tweaks</Typography>
        <Button size="small" onClick={onClose} startIcon={<Icon>close</Icon>}>Close</Button>
      </Box>
      <Divider sx={{ mb: 2 }}/>

      <Typography variant="overline" color="text.secondary">Density</Typography>
      <ToggleButtonGroup value={tweaks.density} exclusive fullWidth size="small"
        onChange={(_, v) => v && setTweak('density', v)} sx={{ mb: 2.5, mt: 0.5 }}>
        <ToggleButton value="compact">Compact</ToggleButton>
        <ToggleButton value="comfortable">Comfortable</ToggleButton>
      </ToggleButtonGroup>

      <Typography variant="overline" color="text.secondary">Games list layout</Typography>
      <FormControl fullWidth size="small" sx={{ mb: 2.5, mt: 0.5 }}>
        <Select value={tweaks.listLayout} onChange={e => setTweak('listLayout', e.target.value)}>
          <MenuItem value="cards">Card grid</MenuItem>
          <MenuItem value="table">Data table</MenuItem>
          <MenuItem value="feed">Activity feed</MenuItem>
        </Select>
      </FormControl>

      <Typography variant="overline" color="text.secondary">Detail page layout</Typography>
      <FormControl fullWidth size="small" sx={{ mb: 2.5, mt: 0.5 }}>
        <Select value={tweaks.detailLayout} onChange={e => setTweak('detailLayout', e.target.value)}>
          <MenuItem value="tabs">Tabbed</MenuItem>
          <MenuItem value="stack">Long-scroll stack</MenuItem>
          <MenuItem value="sidebar">Sidebar nav</MenuItem>
        </Select>
      </FormControl>

      <Divider sx={{ mb: 2 }}/>
      <Typography variant="overline" color="text.secondary">Jump to</Typography>
      <Stack spacing={1} mt={0.5}>
        <Button variant="outlined" fullWidth size="small" startIcon={<Icon>login</Icon>}
          onClick={() => onJump('landing')}>Sign-in screen</Button>
        <Button variant="outlined" fullWidth size="small" startIcon={<Icon>hub</Icon>}
          onClick={() => onJump('leagues')}>League hub</Button>
        <Button variant="outlined" fullWidth size="small" startIcon={<Icon>list</Icon>}
          onClick={() => onJump('games')}>BBL matches</Button>
        <Button variant="outlined" fullWidth size="small" startIcon={<Icon>analytics</Icon>}
          onClick={() => onJump('detail')}>Featured match #0042</Button>
      </Stack>
    </Drawer>
  );
}

// ── League selector dropdown ───────────────────────────────────────────────

function LeagueMenu({ currentLeagueId, onSwitch }) {
  const [anchor, setAnchor] = React.useState(null);
  const leagues = Object.values(window.LEAGUE_DEFS);
  const current = window.LEAGUE_DEFS[currentLeagueId];

  return (
    <>
      <Button
        size="small"
        onClick={e => setAnchor(e.currentTarget)}
        endIcon={<Icon sx={{ fontSize: '14px !important' }}>expand_more</Icon>}
        sx={{
          fontWeight: 600, textTransform: 'none', fontSize: 13,
          color: current?.color || 'text.primary',
          px: 1.25, borderRadius: 1.5,
          '&:hover': { bgcolor: 'action.hover' },
        }}
      >
        {current?.short || 'Select league'}
      </Button>
      <Menu anchorEl={anchor} open={Boolean(anchor)} onClose={() => setAnchor(null)}
        PaperProps={{ sx: { minWidth: 240, mt: 0.5 } }}>
        {leagues.map(l => (
          <MenuItem
            key={l.id}
            selected={l.id === currentLeagueId}
            onClick={() => { onSwitch(l.id); setAnchor(null); }}
          >
            <ListItemIcon>
              <Box sx={{ width: 10, height: 10, borderRadius: '50%', bgcolor: l.color }}/>
            </ListItemIcon>
            <ListItemText
              primary={l.name}
              secondary={`${l.season} · ${l.week}`}
              primaryTypographyProps={{ fontWeight: l.id === currentLeagueId ? 700 : 400 }}
              secondaryTypographyProps={{ fontSize: 11 }}
            />
            <Typography variant="caption" sx={{ fontFamily: 'JetBrains Mono', color: 'text.secondary', ml: 1 }}>
              {window.gamesForLeague(l.id).length}m
            </Typography>
          </MenuItem>
        ))}
        <Divider/>
        <MenuItem dense sx={{ color: 'text.secondary', fontSize: 13 }}>
          <ListItemIcon><Icon sx={{ fontSize: 16 }}>hub</Icon></ListItemIcon>
          <ListItemText primary="All leagues" primaryTypographyProps={{ fontSize: 13 }}/>
        </MenuItem>
      </Menu>
    </>
  );
}

// ── Top navigation bar ─────────────────────────────────────────────────────

function TopBar({ route, go }) {
  const inLeague = route.name === 'games' || route.name === 'detail';
  const leagueId = route.leagueId;

  return (
    <AppBar position="sticky" color="default" elevation={0}
      sx={{ borderBottom: 1, borderColor: 'divider', bgcolor: 'background.paper' }}>
      <Toolbar variant="dense" sx={{ gap: 1.5, minHeight: 52 }}>
        {/* Brand */}
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, cursor: 'pointer' }}
          onClick={() => go('leagues')}>
          <Box sx={{
            width: 26, height: 26, borderRadius: 1.5, flexShrink: 0,
            background: 'linear-gradient(135deg, #1976d2 0%, #66bb6a 100%)',
            display: 'grid', placeItems: 'center',
          }}>
            <Typography sx={{ fontFamily: 'JetBrains Mono', fontWeight: 800, fontSize: 12, color: '#fff' }}>W</Typography>
          </Box>
          <Typography fontWeight={700} sx={{ letterSpacing: '0.04em', textTransform: 'uppercase', fontSize: 13 }}>
            Worms Hub
          </Typography>
        </Box>

        {/* Breadcrumb */}
        <Breadcrumbs sx={{ fontSize: 13 }} separator={<Icon sx={{ fontSize: 14, color: 'text.disabled' }}>chevron_right</Icon>}>
          <Link
            component="button"
            underline="hover"
            color="text.secondary"
            sx={{ fontSize: 13 }}
            onClick={() => go('leagues')}
          >
            Leagues
          </Link>
          {inLeague && leagueId && (
            <LeagueMenu
              currentLeagueId={leagueId}
              onSwitch={id => go('games', { leagueId: id })}
            />
          )}
        </Breadcrumbs>

        {/* Nav items (league-scoped) */}
        {inLeague && (
          <Stack direction="row" spacing={0.25} ml={0.5}>
            {[['Matches','games'],['Leaderboard',null],['Players',null],['Awards',null]].map(([label, r]) => (
              <Button
                key={label}
                size="small"
                color={route.name === r ? 'primary' : 'inherit'}
                onClick={r === 'games' ? () => go('games', { leagueId }) : undefined}
                sx={{ fontWeight: route.name === r ? 700 : 400, textTransform: 'none', fontSize: 13, px: 1 }}
              >
                {label}
              </Button>
            ))}
          </Stack>
        )}

        <Box sx={{ flex: 1 }}/>

        {/* Total archive count */}
        <Chip
          label={`${window.GAMES.length} matches`}
          size="small"
          icon={<Box sx={{ width: 6, height: 6, borderRadius: 99, bgcolor: 'success.main', ml: '8px !important' }}/>}
          variant="outlined"
          sx={{ fontSize: 11 }}
        />

        {/* Profile */}
        <Chip
          avatar={<Box sx={{
            width: 24, height: 24, borderRadius: 99,
            background: 'linear-gradient(135deg, #42a5f5, #ab47bc)',
            display: 'grid', placeItems: 'center',
            fontWeight: 700, fontSize: 11, color: '#000', ml: '2px !important',
          }}>E</Box>}
          label="Eadie"
          variant="outlined"
          sx={{ fontWeight: 600 }}
        />
      </Toolbar>
    </AppBar>
  );
}

// ── Root App ───────────────────────────────────────────────────────────────

function App() {
  const [route,      setRoute]      = React.useState({ name: 'landing' });
  const [tweaks,     setTweak]      = useTweakState(window.TWEAK_DEFAULTS);
  const [drawerOpen, setDrawerOpen] = React.useState(false);

  // Host tweaks protocol
  React.useEffect(() => {
    const handler = e => {
      if (e.data?.type === '__activate_edit_mode')   setDrawerOpen(true);
      if (e.data?.type === '__deactivate_edit_mode') setDrawerOpen(false);
    };
    window.addEventListener('message', handler);
    window.parent.postMessage({ type: '__edit_mode_available' }, '*');
    return () => window.removeEventListener('message', handler);
  }, []);

  const go = (name, extra = {}) => setRoute({ name, ...extra });

  const handleDrawerClose = () => {
    setDrawerOpen(false);
    try { window.parent.postMessage({ type: '__edit_mode_dismissed' }, '*'); } catch(e) {}
  };

  const handleJump = name => {
    if (name === 'landing')  go('landing');
    if (name === 'leagues')  go('leagues');
    if (name === 'games')    go('games',  { leagueId: 'bbl' });
    if (name === 'detail')   go('detail', { leagueId: 'bbl', gameId: 'g-0042' });
    setDrawerOpen(false);
  };

  // When entering a game from the activity feed on leagues hub
  const handleLeagueHubOpen = (leagueId, gameId) => {
    if (gameId) go('detail', { leagueId, gameId });
    else        go('games',  { leagueId });
  };

  const showNav = route.name !== 'landing';

  return (
    <ThemeProvider theme={darkTheme}>
      <CssBaseline/>
      <Box sx={{ minHeight: '100vh', display: 'flex', flexDirection: 'column' }}>
        {showNav && <TopBar route={route} go={go}/>}

        <Box sx={{ flex: 1 }}>
          {route.name === 'landing' && (
            <Landing onSignIn={() => go('leagues')}/>
          )}
          {route.name === 'leagues' && (
            <LeagueHub onEnter={handleLeagueHubOpen}/>
          )}
          {route.name === 'games' && (
            <GamesList
              density={tweaks.density}
              listLayout={tweaks.listLayout}
              league={route.leagueId || 'bbl'}
              onOpen={id => go('detail', { leagueId: route.leagueId || 'bbl', gameId: id })}
            />
          )}
          {route.name === 'detail' && (
            <GameDetail
              gameId={route.gameId || 'g-0042'}
              density={tweaks.density}
              detailLayout={tweaks.detailLayout}
              onBack={() => go('games', { leagueId: route.leagueId || 'bbl' })}
            />
          )}
        </Box>

        {/* Tweaks Fab */}
        <Fab size="small" color="default" onClick={() => setDrawerOpen(true)}
          sx={{ position: 'fixed', bottom: 20, right: 20, zIndex: 1300, opacity: 0.45, '&:hover': { opacity: 1 } }}>
          <Icon>tune</Icon>
        </Fab>

        <TweaksDrawer
          open={drawerOpen}
          onClose={handleDrawerClose}
          tweaks={tweaks}
          setTweak={setTweak}
          onJump={handleJump}
        />
      </Box>
    </ThemeProvider>
  );
}

ReactDOM.createRoot(document.getElementById('root')).render(<App/>);
