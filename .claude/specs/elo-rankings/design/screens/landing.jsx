const {
  Box, Paper, Typography, Button, Divider, Link, Chip,
} = window.MaterialUI;

function Landing({ onSignIn }) {
  return (
    <Box sx={{
      display: 'grid',
      gridTemplateColumns: { xs: '1fr', md: '1.15fr 0.85fr' },
      minHeight: '100vh',
    }}>
      {/* ── Left: hero ── */}
      <Box sx={{
        p: { xs: 4, md: 8 },
        display: 'flex', flexDirection: 'column',
        borderRight: 1, borderColor: 'divider',
        position: 'relative', overflow: 'hidden',
      }}>
        {/* Brand */}
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
          <Box sx={{
            width: 38, height: 38, borderRadius: 2,
            background: 'linear-gradient(135deg, #1976d2 0%, #66bb6a 100%)',
            display: 'grid', placeItems: 'center',
          }}>
            <Typography sx={{ fontFamily: 'JetBrains Mono', fontWeight: 800, fontSize: 15, color: '#fff' }}>W</Typography>
          </Box>
          <Box>
            <Typography sx={{ fontWeight: 700, letterSpacing: '0.06em', textTransform: 'uppercase', lineHeight: 1.1, fontSize: 14 }}>
              Worms Hub
            </Typography>
            <Typography variant="caption" color="text.secondary">Banana Bomb League</Typography>
          </Box>
        </Box>

        {/* Hero copy */}
        <Box sx={{ mt: 'auto', mb: 6 }}>
          <Chip
            label="● Season 4 · Week 6 live"
            size="small"
            color="primary"
            variant="outlined"
            sx={{ mb: 3, fontFamily: 'JetBrains Mono', fontSize: 11, letterSpacing: '0.1em' }}
          />
          <Typography variant="h2" sx={{
            fontWeight: 800, lineHeight: 1.0,
            letterSpacing: '-0.02em', mb: 2,
          }}>
            Every shot.<br/>
            Every{' '}
            <Box component="span" sx={{ color: 'primary.main' }}>kill</Box>
            .<br/>
            Archived.
          </Typography>
          <Typography variant="body1" color="text.secondary" sx={{ maxWidth: 460, lineHeight: 1.65 }}>
            The private replay vault for the Banana Bomb League. Browse every match,
            dig into turn-by-turn damage, and settle the head-to-head debates.
          </Typography>

          {/* Stat strip */}
          <Box sx={{ display: 'flex', gap: 5, mt: 5 }}>
            {[['142','Matches'],['6','Players'],['38,402','Damage'],['2,191','Turns']].map(([n, l]) => (
              <Box key={l}>
                <Typography sx={{ fontFamily: 'JetBrains Mono', fontWeight: 700, fontSize: 28, lineHeight: 1 }}>{n}</Typography>
                <Typography variant="caption" color="text.secondary" sx={{ letterSpacing: '0.16em', textTransform: 'uppercase' }}>{l}</Typography>
              </Box>
            ))}
          </Box>
        </Box>

        <Typography variant="caption" color="text.disabled">v0.4.0 · Private build · Updated 2 min ago</Typography>
        <WormBobber/>
      </Box>

      {/* ── Right: sign-in card ── */}
      <Box sx={{ p: { xs: 4, md: 8 }, display: 'grid', placeItems: 'center', bgcolor: 'background.default' }}>
        <Paper elevation={4} sx={{ width: '100%', maxWidth: 400, p: 4, borderRadius: 3 }}>
          <Typography variant="h5" fontWeight={700} mb={0.5}>Sign in to continue</Typography>
          <Typography variant="body2" color="text.secondary" mb={3}>
            League members only. Auth handled via Auth0.
          </Typography>

          <Button
            variant="contained"
            fullWidth
            size="large"
            onClick={onSignIn}
            startIcon={
              <Box sx={{
                width: 24, height: 24, borderRadius: 1,
                bgcolor: '#c0392b',
                display: 'grid', placeItems: 'center',
                fontFamily: 'JetBrains Mono', fontWeight: 800, fontSize: 10, color: '#fff',
              }}>A0</Box>
            }
            sx={{ py: 1.5, justifyContent: 'flex-start' }}
          >
            Continue with Auth0
          </Button>

          <Divider sx={{ my: 3 }}>
            <Typography variant="caption" color="text.secondary" sx={{ letterSpacing: '0.16em', textTransform: 'uppercase' }}>
              League access
            </Typography>
          </Divider>

          <Box sx={{ display: 'grid', gap: 1.5, mb: 3 }}>
            {[
              ['Need an invite?', 'Ask Eadie on Discord', false],
              ['Replay uploader',  'v1.2 · auto-sync',    true ],
            ].map(([k, v, mono]) => (
              <Box key={k} sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Typography variant="body2" color="text.secondary">{k}</Typography>
                <Typography
                  variant="body2"
                  color="primary.light"
                  sx={{ fontFamily: mono ? 'JetBrains Mono' : 'inherit', fontSize: mono ? 12 : 'inherit' }}
                >{v}</Typography>
              </Box>
            ))}
          </Box>

          <Typography variant="caption" color="text.disabled" sx={{ display: 'block', textAlign: 'center', lineHeight: 1.7 }}>
            By signing in you agree to keep replays inside the league.{' '}
            <Link href="#" color="inherit" underline="always">Privacy</Link>
            {' · '}
            <Link href="#" color="inherit" underline="always">House rules</Link>
          </Typography>
        </Paper>
      </Box>
    </Box>
  );
}

window.Landing = Landing;
