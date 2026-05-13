import { useEffect, useState } from 'react'
import { useParams, useNavigate, Link as RouterLink } from 'react-router'
import { useAuth } from 'react-oidc-context'
import Box from '@mui/material/Box'
import Breadcrumbs from '@mui/material/Breadcrumbs'
import Chip from '@mui/material/Chip'
import CircularProgress from '@mui/material/CircularProgress'
import Container from '@mui/material/Container'
import MuiLink from '@mui/material/Link'
import Paper from '@mui/material/Paper'
import Stack from '@mui/material/Stack'
import Table from '@mui/material/Table'
import TableBody from '@mui/material/TableBody'
import TableCell from '@mui/material/TableCell'
import TableContainer from '@mui/material/TableContainer'
import TableHead from '@mui/material/TableHead'
import TableRow from '@mui/material/TableRow'
import Typography from '@mui/material/Typography'
import WorkspacePremiumIcon from '@mui/icons-material/WorkspacePremium'
import { monoFontFamily } from '../theme'
import { gatewayUrl } from '../api'

interface LeagueDto {
    id: string
    name: string
    version: string | null
    schemeUrl: string | null
}

interface ReplayInLeagueDto {
    id: string
    name: string
    status: string
    date: string | null
    winner: string | null
    teams: string[] | null
}

function LeagueDetailPage() {
    const { id } = useParams<{ id: string }>()
    const navigate = useNavigate()
    const auth = useAuth()
    const [league, setLeague] = useState<LeagueDto | null>(null)
    const [replays, setReplays] = useState<ReplayInLeagueDto[] | null>(null)
    const [error, setError] = useState<string | null>(null)
    const [notFound, setNotFound] = useState(false)

    useEffect(() => {
        if (!auth.user?.access_token || !id) return
        const token = auth.user.access_token
        const headers = { Authorization: `Bearer ${token}` }
        Promise.all([
            fetch(`${gatewayUrl}/api/v1/leagues/${id}`, { headers }),
            fetch(`${gatewayUrl}/api/v1/leagues/${id}/replays`, { headers }),
        ])
            .then(async ([leagueRes, replaysRes]) => {
                if (leagueRes.status === 404) {
                    setNotFound(true)
                    return
                }
                if (!leagueRes.ok) throw new Error(`HTTP ${leagueRes.status}`)
                if (!replaysRes.ok) throw new Error(`HTTP ${replaysRes.status}`)
                const [leagueData, replaysData] = await Promise.all([
                    leagueRes.json() as Promise<LeagueDto>,
                    replaysRes.json() as Promise<ReplayInLeagueDto[]>,
                ])
                setLeague(leagueData)
                setReplays(replaysData)
            })
            .catch((err: unknown) => setError(String(err)))
    }, [auth.user?.access_token, id])

    return (
        <Container maxWidth="xl" sx={{ py: { xs: 2, md: 4 } }}>
            {league === null && replays === null && error === null && !notFound && (
                <CircularProgress />
            )}
            {notFound && <Typography>League not found.</Typography>}
            {error !== null && <Typography color="error">Error: {error}</Typography>}
            {league !== null && replays !== null && (
                <Box>
                    <Breadcrumbs sx={{ mb: 2 }}>
                        <MuiLink
                            component={RouterLink}
                            to="/leagues"
                            variant="body2"
                            color="inherit"
                            underline="hover"
                        >
                            Leagues
                        </MuiLink>
                        <Typography variant="body2" color="text.primary" component="span">
                            {league.name}
                        </Typography>
                    </Breadcrumbs>
                    <Box sx={{ mb: 3 }}>
                        <Typography variant="h4" sx={{ fontWeight: 700, mb: 1 }}>
                            {league.name}
                        </Typography>
                        {league.version !== null && (
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 1.5 }}>
                                <Chip
                                    label={`Scheme v${league.version}`}
                                    size="small"
                                    variant="outlined"
                                    sx={{ fontFamily: monoFontFamily, fontSize: 11 }}
                                />
                            </Box>
                        )}
                    </Box>

                    {replays.length === 0 && (
                        <Typography color="text.secondary">
                            No replays found for this league.
                        </Typography>
                    )}
                    {replays.length > 0 && (
                        <TableContainer component={Paper} variant="outlined">
                            <Table size="small">
                                <TableHead>
                                    <TableRow>
                                        <TableCell sx={{ fontWeight: 700, width: 72 }}>
                                            Match
                                        </TableCell>
                                        <TableCell sx={{ fontWeight: 700, width: 150 }}>
                                            Date
                                        </TableCell>
                                        <TableCell sx={{ fontWeight: 700 }}>Players</TableCell>
                                        <TableCell sx={{ fontWeight: 700, width: 160 }}>
                                            Top weapons
                                        </TableCell>
                                        <TableCell sx={{ fontWeight: 700, width: 80 }}>
                                            Length
                                        </TableCell>
                                        <TableCell
                                            sx={{ fontWeight: 700, width: 160 }}
                                            align="right"
                                        >
                                            Most Damage
                                        </TableCell>
                                    </TableRow>
                                </TableHead>
                                <TableBody>
                                    {replays
                                        .slice()
                                        .sort(
                                            (a, b) =>
                                                new Date(b.date ?? b.name).getTime() -
                                                new Date(a.date ?? a.name).getTime(),
                                        )
                                        .map((replay) =>
                                            replay.status === 'Processed' ? (
                                                <TableRow
                                                    key={replay.id}
                                                    hover
                                                    onClick={() =>
                                                        void navigate(
                                                            `/leagues/${id}/replays/${replay.id}`,
                                                        )
                                                    }
                                                    sx={{ cursor: 'pointer' }}
                                                >
                                                    <TableCell>
                                                        <Typography
                                                            sx={{
                                                                fontFamily: monoFontFamily,
                                                                fontSize: 12,
                                                                color: 'text.secondary',
                                                            }}
                                                        >
                                                            #{replay.id.padStart(3, '0')}
                                                        </Typography>
                                                    </TableCell>
                                                    <TableCell>
                                                        <Typography variant="body2">
                                                            {new Date(
                                                                replay.date!,
                                                            ).toLocaleDateString('en-GB', {
                                                                year: 'numeric',
                                                                month: 'short',
                                                                day: 'numeric',
                                                            })}
                                                        </Typography>
                                                        <Typography
                                                            sx={{
                                                                fontFamily: monoFontFamily,
                                                                fontSize: 11,
                                                                color: 'text.secondary',
                                                            }}
                                                        >
                                                            {new Date(
                                                                replay.date!,
                                                            ).toLocaleTimeString('en-GB', {
                                                                hour: '2-digit',
                                                                minute: '2-digit',
                                                            })}
                                                        </Typography>
                                                    </TableCell>
                                                    <TableCell>
                                                        <Stack
                                                            direction="row"
                                                            spacing={0.5}
                                                            sx={{ flexWrap: 'wrap' }}
                                                            useFlexGap
                                                        >
                                                            {replay.teams
                                                                ?.slice()
                                                                .sort((a) =>
                                                                    a === replay.winner ? -1 : 1,
                                                                )
                                                                .map((team) => (
                                                                    <Box
                                                                        key={team}
                                                                        sx={{
                                                                            display: 'flex',
                                                                            alignItems: 'center',
                                                                            gap: 0.5,
                                                                        }}
                                                                    >
                                                                        {team === replay.winner && (
                                                                            <WorkspacePremiumIcon
                                                                                sx={{
                                                                                    fontSize: 14,
                                                                                    color: 'warning.main',
                                                                                }}
                                                                            />
                                                                        )}
                                                                        <Chip
                                                                            label={team}
                                                                            size="small"
                                                                            variant="outlined"
                                                                            sx={{
                                                                                fontFamily:
                                                                                    monoFontFamily,
                                                                                fontSize: 11,
                                                                            }}
                                                                        />
                                                                    </Box>
                                                                ))}
                                                        </Stack>
                                                    </TableCell>
                                                    <TableCell>
                                                        <Typography
                                                            variant="caption"
                                                            color="text.disabled"
                                                        >
                                                            —
                                                        </Typography>
                                                    </TableCell>
                                                    <TableCell>
                                                        <Typography
                                                            variant="caption"
                                                            color="text.disabled"
                                                        >
                                                            —
                                                        </Typography>
                                                    </TableCell>
                                                    <TableCell align="right">
                                                        <Typography
                                                            variant="caption"
                                                            color="text.disabled"
                                                        >
                                                            —
                                                        </Typography>
                                                    </TableCell>
                                                </TableRow>
                                            ) : (
                                                <TableRow key={replay.id}>
                                                    <TableCell>
                                                        <Typography
                                                            sx={{
                                                                fontFamily: monoFontFamily,
                                                                fontSize: 12,
                                                                color: 'text.secondary',
                                                            }}
                                                        >
                                                            #{replay.id.padStart(3, '0')}
                                                        </Typography>
                                                    </TableCell>
                                                    <TableCell>
                                                        <Typography variant="body2">
                                                            {new Date(
                                                                replay.name,
                                                            ).toLocaleDateString('en-GB', {
                                                                year: 'numeric',
                                                                month: 'short',
                                                                day: 'numeric',
                                                            })}
                                                        </Typography>
                                                        <Typography
                                                            sx={{
                                                                fontFamily: monoFontFamily,
                                                                fontSize: 11,
                                                                color: 'text.secondary',
                                                            }}
                                                        >
                                                            &nbsp;
                                                        </Typography>
                                                    </TableCell>
                                                    <TableCell colSpan={4}>
                                                        <Box
                                                            sx={{
                                                                display: 'flex',
                                                                alignItems: 'center',
                                                                gap: 1,
                                                            }}
                                                        >
                                                            <CircularProgress size={12} />
                                                            <Typography
                                                                variant="body2"
                                                                color="text.secondary"
                                                            >
                                                                Processing...
                                                            </Typography>
                                                        </Box>
                                                    </TableCell>
                                                </TableRow>
                                            ),
                                        )}
                                </TableBody>
                            </Table>
                        </TableContainer>
                    )}
                </Box>
            )}
        </Container>
    )
}

export default LeagueDetailPage
