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
import Typography from '@mui/material/Typography'
import { monoFontFamily } from '../theme'
import { gatewayUrl } from '../api'

interface LeagueDto {
    id: string
    name: string
    version: string | null
    schemeUrl: string
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
