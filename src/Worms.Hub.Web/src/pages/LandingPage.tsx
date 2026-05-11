// Image source: https://static.wikia.nocookie.net/oneyplays/images/5/52/Worm.png — Worms Armageddon asset (Team17)
import { useEffect, useState } from 'react'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Divider from '@mui/material/Divider'
import Paper from '@mui/material/Paper'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import { useAuth } from 'react-oidc-context'

const WEAPONS = [
    'Bazooka',
    'Homing Missile',
    'Mortar',
    'Grenade',
    'Cluster Bomb',
    'Banana Bomb',
    'Shotgun',
    'Uzi',
    'Fire Punch',
    'Dragon Ball',
    'Baseball Bat',
    'Holy Hand Grenade',
    'Ninja Rope',
    'Blowtorch',
    'Pneumatic Drill',
    'Girder',
    'Teleport',
    'Parachute',
    'Jetpack',
    'Low Gravity',
    'Fast Walk',
    'Laser Sight',
    'Earthquake',
    'Carpet Bomb',
    'Prod',
    'Kamikaze',
    'Old Woman',
    'Concrete Donkey',
    'Skunk',
    'Mad Cow',
    'Sheep',
    'Super Sheep',
    'Aqua Sheep',
    'Mine',
    'Dynamite',
    'Airstrike',
    'Ming Vase',
    'Pigeon',
]

function LandingPage() {
    const auth = useAuth()

    const [weaponIndex, setWeaponIndex] = useState(() => Math.floor(Math.random() * WEAPONS.length))
    const [displayText, setDisplayText] = useState('')
    const [isDeleting, setIsDeleting] = useState(false)

    useEffect(() => {
        const currentWeapon = WEAPONS[weaponIndex]

        if (!isDeleting && displayText === currentWeapon) {
            const t = setTimeout(() => setIsDeleting(true), 2000)
            return () => clearTimeout(t)
        }

        if (isDeleting && displayText === '') {
            const t = setTimeout(() => {
                setWeaponIndex((prev) => {
                    let next = Math.floor(Math.random() * (WEAPONS.length - 1))
                    if (next >= prev) next++
                    return next
                })
                setIsDeleting(false)
            }, 400)
            return () => clearTimeout(t)
        }

        const speed = isDeleting ? 60 : 100
        const t = setTimeout(() => {
            setDisplayText(
                isDeleting
                    ? displayText.slice(0, -1)
                    : currentWeapon.slice(0, displayText.length + 1),
            )
        }, speed)
        return () => clearTimeout(t)
    }, [displayText, isDeleting, weaponIndex])

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
                    alignItems: 'flex-start',
                    textAlign: 'left',
                    gap: 4,
                    borderRight: { md: 1 },
                    borderColor: { md: 'divider' },
                }}
            >
                <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                    <Typography
                        variant="h2"
                        component="h1"
                        sx={{ fontWeight: 800, lineHeight: 1.0, letterSpacing: '-0.02em' }}
                    >
                        All the chaos.
                        <br />
                        Every{' '}
                        <Box component="span" sx={{ color: 'primary.main' }}>
                            {displayText}
                            <Box
                                component="span"
                                sx={{
                                    '@keyframes blink': {
                                        '0%, 100%': { opacity: 1 },
                                        '50%': { opacity: 0 },
                                    },
                                    animation: 'blink 1s step-end infinite',
                                }}
                            >
                                |
                            </Box>
                            .
                        </Box>
                        <br />
                        Archived.
                    </Typography>
                    <Typography
                        variant="body1"
                        color="text.secondary"
                        sx={{ maxWidth: 460, lineHeight: 1.65 }}
                    >
                        The replay vault for Worms Armageddon. Browse every match, dig into
                        turn-by-turn damage, and finally settle who is the best.
                    </Typography>
                </Box>
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
                <Paper elevation={4} sx={{ width: '100%', maxWidth: 480, p: 4, borderRadius: 3 }}>
                    <Typography variant="h5" sx={{ fontWeight: 700, mb: 0.5 }}>
                        Sign in to continue
                    </Typography>
                    <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
                        League members only.
                    </Typography>

                    <Button
                        variant="contained"
                        fullWidth
                        size="large"
                        sx={{ py: 1.5 }}
                        onClick={() => void auth.signinRedirect()}
                    >
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
