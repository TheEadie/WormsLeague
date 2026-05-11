// Image source: https://static.wikia.nocookie.net/oneyplays/images/5/52/Worm.png — Worms Armageddon asset (Team17)
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Divider from '@mui/material/Divider'
import Paper from '@mui/material/Paper'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'

function LandingPage() {
    return (
        <Box
            sx={{
                display: 'grid',
                gridTemplateColumns: { xs: '1fr', md: '1.15fr 0.85fr' },
                flex: { md: 1 },
            }}
        >
            {/* Hero column */}
            <Box
                sx={{
                    p: { xs: 4, md: 8 },
                    display: 'flex',
                    flexDirection: 'column',
                    justifyContent: 'center',
                    gap: 4,
                    borderRight: { md: 1 },
                    borderColor: { md: 'divider' },
                }}
            >
                <Box
                    component="img"
                    src="/worm.png"
                    alt="Worm"
                    sx={{ maxWidth: 240, width: '100%', height: 'auto' }}
                />
                <Typography
                    variant="h2"
                    component="h1"
                    sx={{ fontWeight: 800, lineHeight: 1.0, letterSpacing: '-0.02em' }}
                >
                    Every shot. Every kill. Archived.
                </Typography>
            </Box>

            {/* Sign-in column */}
            <Box
                sx={{
                    p: { xs: 4, md: 8 },
                    display: 'grid',
                    placeItems: 'center',
                    bgcolor: 'background.default',
                }}
            >
                <Paper elevation={4} sx={{ width: '100%', maxWidth: 400, p: 4, borderRadius: 3 }}>
                    <Typography variant="h5" sx={{ fontWeight: 700, mb: 0.5 }}>
                        Sign in to continue
                    </Typography>
                    <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
                        League members only.
                    </Typography>

                    <Button variant="contained" fullWidth size="large" sx={{ py: 1.5 }}>
                        Sign in
                    </Button>

                    <Divider sx={{ my: 3 }}>
                        <Typography variant="overline" color="text.secondary">
                            League access
                        </Typography>
                    </Divider>

                    <Stack
                        direction="row"
                        sx={{ justifyContent: 'space-between', alignItems: 'center' }}
                    >
                        <Typography variant="body2" color="text.secondary">
                            Need an invite?
                        </Typography>
                        <Typography variant="body2" color="primary.light">
                            Contact Eadie on Slack
                        </Typography>
                    </Stack>
                </Paper>
            </Box>
        </Box>
    )
}

export default LandingPage
