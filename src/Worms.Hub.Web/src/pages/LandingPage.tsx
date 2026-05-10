// Image source: https://static.wikia.nocookie.net/oneyplays/images/5/52/Worm.png — Worms Armageddon asset (Team17)
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Typography from '@mui/material/Typography'
import Stack from '@mui/material/Stack'

function LandingPage() {
    return (
        <Box
            sx={{
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                justifyContent: 'center',
                minHeight: '60vh',
            }}
        >
            <Stack spacing={3} sx={{ alignItems: 'center' }}>
                <img
                    src="/worm.png"
                    alt="Worm"
                    style={{ maxWidth: 300, width: '100%', height: 'auto' }}
                />
                <Typography variant="h2" component="h1">
                    Worms Hub
                </Typography>
                <Button variant="contained" size="large">
                    Sign in
                </Button>
            </Stack>
        </Box>
    )
}

export default LandingPage
