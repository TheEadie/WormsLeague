import { useEffect, useState } from 'react'
import { useAuth } from 'react-oidc-context'
import { Link } from 'react-router'
import Box from '@mui/material/Box'
import Breadcrumbs from '@mui/material/Breadcrumbs'
import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
import Chip from '@mui/material/Chip'
import CircularProgress from '@mui/material/CircularProgress'
import Container from '@mui/material/Container'
import Divider from '@mui/material/Divider'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import { monoFontFamily } from '../theme'
import { gatewayUrl } from '../api'
import type { StandingDto } from '../types'

interface LeagueDto {
    id: string
    name: string
    version: string | null
    schemeUrl: string
    standings: StandingDto[] | null
}

function LeagueCard({ league }: { league: LeagueDto }) {
    return (
        <Link
            to={`/leagues/${league.id}`}
            style={{ textDecoration: 'none', display: 'block', height: '100%' }}
        >
            <Card
                variant="outlined"
                sx={{
                    borderTop: '3px solid',
                    borderTopColor: 'primary.main',
                    cursor: 'pointer',
                    transition: 'box-shadow 0.15s',
                    '&:hover': { boxShadow: 4 },
                    height: '100%',
                }}
            >
                <CardContent sx={{ p: 2.5, '&:last-child': { pb: 2.5 } }}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                        <Box
                            sx={{
                                width: 8,
                                height: 8,
                                borderRadius: '50%',
                                bgcolor: 'primary.main',
                                boxShadow: (t) => `0 0 6px ${t.palette.primary.main}`,
                                flexShrink: 0,
                            }}
                        />
                        <Typography
                            variant="caption"
                            sx={{
                                color: 'primary.main',
                                fontFamily: monoFontFamily,
                                fontWeight: 700,
                                letterSpacing: '0.05em',
                            }}
                        >
                            {league.id}
                        </Typography>
                    </Box>

                    <Typography variant="h6" sx={{ fontWeight: 700, lineHeight: 1.2 }}>
                        {league.name}
                    </Typography>

                    {league.version !== null && (
                        <Chip
                            label={`Scheme v${league.version}`}
                            size="small"
                            variant="outlined"
                            sx={{ mt: 1.5, fontFamily: monoFontFamily, fontSize: 11 }}
                        />
                    )}

                    {league.standings !== null && league.standings.length > 0 && (
                        <>
                            <Divider sx={{ mt: 2, mb: 1.5 }} />
                            <Box>
                                <Box
                                    sx={{
                                        display: 'flex',
                                        alignItems: 'baseline',
                                        justifyContent: 'space-between',
                                        mb: 1,
                                    }}
                                >
                                    <Typography
                                        variant="caption"
                                        color="text.secondary"
                                        sx={{
                                            letterSpacing: '0.1em',
                                        }}
                                    >
                                        LEADERBOARD
                                    </Typography>
                                    <Typography
                                        sx={{
                                            fontFamily: monoFontFamily,
                                            fontSize: 10,
                                            color: 'text.disabled',
                                        }}
                                    >
                                        top {Math.min(3, league.standings.length)} of{' '}
                                        {league.standings.length}
                                    </Typography>
                                </Box>
                                <Stack spacing={0.5}>
                                    {league.standings.slice(0, 3).map((s, index) => {
                                        const place = index + 1
                                        const medal = (['#ffca28', '#bdbdbd', '#cd7f32'] as const)[
                                            index
                                        ]
                                        return (
                                            <Box
                                                key={s.playerName}
                                                sx={{
                                                    display: 'grid',
                                                    gridTemplateColumns: '22px 1fr auto',
                                                    gap: 1,
                                                    alignItems: 'center',
                                                }}
                                            >
                                                <Typography
                                                    sx={{
                                                        fontFamily: monoFontFamily,
                                                        fontSize: 12,
                                                        fontWeight: 700,
                                                        color: medal,
                                                        textAlign: 'center',
                                                    }}
                                                >
                                                    {place}
                                                </Typography>
                                                <Typography
                                                    variant="body2"
                                                    sx={{
                                                        fontWeight: place === 1 ? 700 : 500,
                                                        overflow: 'hidden',
                                                        textOverflow: 'ellipsis',
                                                        whiteSpace: 'nowrap',
                                                    }}
                                                >
                                                    {s.playerName}
                                                </Typography>
                                                <Typography
                                                    sx={{
                                                        fontFamily: monoFontFamily,
                                                        fontSize: 13,
                                                        fontWeight: 700,
                                                        color: 'primary.main',
                                                        textAlign: 'right',
                                                    }}
                                                >
                                                    {s.elo}
                                                </Typography>
                                            </Box>
                                        )
                                    })}
                                </Stack>
                            </Box>
                        </>
                    )}
                </CardContent>
            </Card>
        </Link>
    )
}

function LeagueListPage() {
    const auth = useAuth()
    const [leagues, setLeagues] = useState<LeagueDto[] | null>(null)
    const [error, setError] = useState<string | null>(null)

    useEffect(() => {
        if (!auth.user?.access_token) return
        fetch(`${gatewayUrl}/api/v1/leagues`, {
            headers: { Authorization: `Bearer ${auth.user.access_token}` },
        })
            .then((res) => {
                if (!res.ok) throw new Error(`HTTP ${res.status}`)
                return res.json() as Promise<LeagueDto[]>
            })
            .then(setLeagues)
            .catch((err: unknown) => setError(String(err)))
    }, [auth.user?.access_token])

    return (
        <Container maxWidth="xl" sx={{ py: { xs: 2, md: 4 } }}>
            <Breadcrumbs sx={{ mb: 2 }}>
                <Typography variant="body2" color="text.primary" component="span">
                    Leagues
                </Typography>
            </Breadcrumbs>
            <Box sx={{ mb: 4 }}>
                <Typography variant="h4" sx={{ fontWeight: 700 }}>
                    Leagues
                </Typography>
                {leagues !== null && (
                    <Typography variant="body2" color="text.secondary" sx={{ mt: 0.75 }}>
                        {leagues.length} active {leagues.length === 1 ? 'league' : 'leagues'}
                    </Typography>
                )}
            </Box>

            {leagues === null && error === null && <CircularProgress />}
            {error !== null && (
                <Typography color="error">Error loading leagues: {error}</Typography>
            )}
            {leagues !== null && leagues.length === 0 && (
                <Typography color="text.secondary">No leagues found.</Typography>
            )}
            {leagues !== null && leagues.length > 0 && (
                <Box
                    sx={{
                        display: 'grid',
                        gap: 2.5,
                        gridTemplateColumns: { xs: '1fr', md: 'repeat(3, 1fr)' },
                    }}
                >
                    {leagues.map((league) => (
                        <LeagueCard key={league.id} league={league} />
                    ))}
                </Box>
            )}
        </Container>
    )
}

export default LeagueListPage
