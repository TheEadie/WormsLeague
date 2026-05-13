import { useEffect, useState } from 'react'
import { useParams, Link as RouterLink } from 'react-router'
import { useAuth } from 'react-oidc-context'
import Box from '@mui/material/Box'
import Breadcrumbs from '@mui/material/Breadcrumbs'
import Chip from '@mui/material/Chip'
import CircularProgress from '@mui/material/CircularProgress'
import Container from '@mui/material/Container'
import List from '@mui/material/List'
import ListItemButton from '@mui/material/ListItemButton'
import ListItemIcon from '@mui/material/ListItemIcon'
import ListItemText from '@mui/material/ListItemText'
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
import TimelineIcon from '@mui/icons-material/Timeline'
import GpsFixedIcon from '@mui/icons-material/GpsFixed'
import { monoFontFamily } from '../theme'
import { gatewayUrl } from '../api'

interface WeaponDto {
    name: string
}

interface DamageSummaryDto {
    teamName: string
    healthLost: number
    wormsKilled: number
}

interface TurnDto {
    turnNumber: number
    teamName: string
    startMs: number
    endMs: number
    weapons: WeaponDto[]
    damage: DamageSummaryDto[]
}

interface ReplayDetailDto {
    id: string
    name: string
    status: string
    date: string | null
    winner: string | null
    teams: string[] | null
    turns: TurnDto[] | null
}

interface LeagueDto {
    id: string
    name: string
    version: string | null
    schemeUrl: string | null
}

function formatDuration(ms: number): string {
    const totalSeconds = Math.floor(ms / 1000)
    const minutes = Math.floor(totalSeconds / 60)
    const seconds = totalSeconds % 60
    return `${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`
}

interface TurnByTurnPanelProps {
    turns: TurnDto[] | null
}

function TurnByTurnPanel({ turns }: TurnByTurnPanelProps) {
    if (turns === null || turns.length === 0) {
        return (
            <Paper variant="outlined" sx={{ p: 3, textAlign: 'center' }}>
                <Typography color="text.secondary">
                    No turn-by-turn data available for this replay.
                </Typography>
            </Paper>
        )
    }

    return (
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
                    {turns.map((turn) => (
                        <TableRow key={turn.turnNumber}>
                            <TableCell
                                sx={{
                                    fontFamily: monoFontFamily,
                                    fontSize: 12,
                                    color: 'text.secondary',
                                }}
                            >
                                {String(turn.turnNumber).padStart(2, '0')}
                            </TableCell>
                            <TableCell>
                                <Chip size="small" label={turn.teamName} />
                            </TableCell>
                            <TableCell>
                                {turn.weapons.length === 0 ? (
                                    <Typography variant="caption" color="text.disabled">
                                        —
                                    </Typography>
                                ) : (
                                    <Stack
                                        direction="row"
                                        spacing={0.5}
                                        sx={{ flexWrap: 'wrap' }}
                                        useFlexGap
                                    >
                                        {turn.weapons.map((w, i) => (
                                            <Chip
                                                key={i}
                                                size="small"
                                                label={w.name}
                                                variant="outlined"
                                                sx={
                                                    i === turn.weapons.length - 1
                                                        ? { fontWeight: 700 }
                                                        : undefined
                                                }
                                            />
                                        ))}
                                    </Stack>
                                )}
                            </TableCell>
                            <TableCell>
                                {turn.damage.length === 0 ? (
                                    <Typography variant="caption" color="text.disabled">
                                        —
                                    </Typography>
                                ) : (
                                    <Stack direction="column" spacing={0.5}>
                                        {turn.damage.map((d) => (
                                            <Box
                                                key={d.teamName}
                                                sx={{
                                                    display: 'flex',
                                                    alignItems: 'center',
                                                    gap: 0.5,
                                                }}
                                            >
                                                <Typography variant="body2">
                                                    {d.teamName}: {d.healthLost}
                                                </Typography>
                                                {d.wormsKilled > 0 && (
                                                    <Chip
                                                        label={`+${d.wormsKilled} kill${d.wormsKilled > 1 ? 's' : ''}`}
                                                        size="small"
                                                        color="error"
                                                    />
                                                )}
                                            </Box>
                                        ))}
                                    </Stack>
                                )}
                            </TableCell>
                        </TableRow>
                    ))}
                </TableBody>
            </Table>
        </TableContainer>
    )
}

type WeaponStats = { name: string; usageCount: number; attributedDamage: number }
type TeamWeaponMap = Map<string, WeaponStats[]>

interface WeaponsPanelProps {
    turns: TurnDto[] | null
}

function WeaponsPanel({ turns }: WeaponsPanelProps) {
    if (turns === null || turns.length === 0) {
        return (
            <Paper variant="outlined" sx={{ p: 3, textAlign: 'center' }}>
                <Typography color="text.secondary">
                    No weapon data available for this replay.
                </Typography>
            </Paper>
        )
    }

    const teamWeaponMap: TeamWeaponMap = new Map()

    for (const turn of turns) {
        if (!teamWeaponMap.has(turn.teamName)) {
            teamWeaponMap.set(turn.teamName, [])
        }
        const stats = teamWeaponMap.get(turn.teamName)!

        for (const weapon of turn.weapons) {
            let entry = stats.find((s) => s.name === weapon.name)
            if (!entry) {
                entry = { name: weapon.name, usageCount: 0, attributedDamage: 0 }
                stats.push(entry)
            }
            entry.usageCount++
        }

        if (turn.weapons.length > 0) {
            const lastWeapon = turn.weapons[turn.weapons.length - 1]
            const totalDamage = turn.damage.reduce((sum, d) => sum + d.healthLost, 0)
            const entry = stats.find((s) => s.name === lastWeapon.name)
            if (entry) {
                entry.attributedDamage += totalDamage
            }
        }
    }

    const sortedTeams = Array.from(teamWeaponMap.entries()).map(([teamName, weaponStats]) => ({
        teamName,
        weapons: weaponStats
            .slice()
            .sort((a, b) => b.attributedDamage - a.attributedDamage || b.usageCount - a.usageCount),
    }))

    return (
        <Stack spacing={2}>
            {sortedTeams.map(({ teamName, weapons }) => (
                <Box key={teamName}>
                    <Typography variant="subtitle2" sx={{ fontWeight: 700, mb: 1 }}>
                        {teamName}
                    </Typography>
                    <TableContainer component={Paper} variant="outlined">
                        <Table size="small">
                            <TableHead>
                                <TableRow>
                                    <TableCell sx={{ fontWeight: 700 }}>Weapon</TableCell>
                                    <TableCell sx={{ fontWeight: 700, width: 60 }}>Uses</TableCell>
                                    <TableCell sx={{ fontWeight: 700, width: 120 }}>
                                        Attributed Damage
                                    </TableCell>
                                </TableRow>
                            </TableHead>
                            <TableBody>
                                {weapons.map((w) => (
                                    <TableRow key={w.name}>
                                        <TableCell>{w.name}</TableCell>
                                        <TableCell
                                            sx={{ fontFamily: monoFontFamily, fontSize: 12 }}
                                        >
                                            {w.usageCount}
                                        </TableCell>
                                        <TableCell
                                            sx={{ fontFamily: monoFontFamily, fontSize: 12 }}
                                        >
                                            {w.attributedDamage}
                                        </TableCell>
                                    </TableRow>
                                ))}
                            </TableBody>
                        </Table>
                    </TableContainer>
                </Box>
            ))}
        </Stack>
    )
}

function GameDetailPage() {
    const { id, replayId } = useParams<{ id: string; replayId: string }>()
    const auth = useAuth()
    const [replay, setReplay] = useState<ReplayDetailDto | null>(null)
    const [league, setLeague] = useState<LeagueDto | null>(null)
    const [error, setError] = useState<string | null>(null)
    const [notFound, setNotFound] = useState(false)
    const [activePanel, setActivePanel] = useState(0)

    useEffect(() => {
        if (!auth.user?.access_token || !id || !replayId) return
        const token = auth.user.access_token
        const headers = { Authorization: `Bearer ${token}` }
        Promise.all([
            fetch(`${gatewayUrl}/api/v1/leagues/${id}/replays/${replayId}`, { headers }),
            fetch(`${gatewayUrl}/api/v1/leagues/${id}`, { headers }),
        ])
            .then(async ([replayRes, leagueRes]) => {
                if (replayRes.status === 404) {
                    setNotFound(true)
                    return
                }
                if (!replayRes.ok) throw new Error(`HTTP ${replayRes.status}`)
                if (!leagueRes.ok) throw new Error(`HTTP ${leagueRes.status}`)
                const [replayData, leagueData] = await Promise.all([
                    replayRes.json() as Promise<ReplayDetailDto>,
                    leagueRes.json() as Promise<LeagueDto>,
                ])
                setReplay(replayData)
                setLeague(leagueData)
            })
            .catch((err: unknown) => setError(String(err)))
    }, [auth.user?.access_token, id, replayId])

    const panels = [
        {
            label: 'Turn-by-turn',
            icon: <TimelineIcon sx={{ fontSize: 18 }} />,
            content: <TurnByTurnPanel turns={replay?.turns ?? null} />,
        },
        {
            label: 'Weapons',
            icon: <GpsFixedIcon sx={{ fontSize: 18 }} />,
            content: <WeaponsPanel turns={replay?.turns ?? null} />,
        },
    ]

    const isLoading = replay === null && league === null && error === null && !notFound

    const turns = replay?.turns ?? null
    const hasTurns = turns !== null && turns.length > 0

    const duration = hasTurns ? turns[turns.length - 1].endMs - turns[0].startMs : 0

    const maxDamagePerTurn = hasTurns
        ? Math.max(...turns.map((t) => t.damage.reduce((sum, d) => sum + d.healthLost, 0)))
        : 0

    const totalKills = hasTurns
        ? turns.reduce((sum, t) => sum + t.damage.reduce((sum2, d) => sum2 + d.wormsKilled, 0), 0)
        : 0

    return (
        <Container maxWidth="xl" sx={{ py: { xs: 2, md: 4 } }}>
            {isLoading && <CircularProgress />}
            {notFound && <Typography>Replay not found.</Typography>}
            {error !== null && <Typography color="error">Error: {error}</Typography>}
            {replay !== null && replay.status !== 'Processed' && (
                <Typography color="text.secondary">
                    This replay is being processed. Please check back soon.
                </Typography>
            )}
            {replay !== null && replay.status === 'Processed' && league !== null && (
                <Box>
                    {/* Breadcrumb */}
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
                        <MuiLink
                            component={RouterLink}
                            to={`/leagues/${id}`}
                            variant="body2"
                            color="inherit"
                            underline="hover"
                        >
                            {league.name}
                        </MuiLink>
                        <Typography variant="body2" color="text.primary" component="span">
                            Match #{replay.id.padStart(3, '0')}
                        </Typography>
                    </Breadcrumbs>

                    {/* Hero card */}
                    <Paper variant="outlined" sx={{ p: 3, mb: 2 }}>
                        {/* Title row */}
                        <Box
                            sx={{
                                display: 'flex',
                                alignItems: 'center',
                                gap: 2,
                                mb: 2,
                                flexWrap: 'wrap',
                            }}
                        >
                            <Typography variant="h5" sx={{ fontWeight: 700 }}>
                                Match #{replay.id.padStart(3, '0')}
                            </Typography>
                            {replay.date !== null && (
                                <Typography
                                    sx={{
                                        fontFamily: monoFontFamily,
                                        fontSize: 12,
                                        color: 'text.secondary',
                                    }}
                                >
                                    {new Date(replay.date).toLocaleDateString('en-GB', {
                                        year: 'numeric',
                                        month: 'short',
                                        day: 'numeric',
                                    })}{' '}
                                    &middot;{' '}
                                    {new Date(replay.date).toLocaleTimeString('en-GB', {
                                        hour: '2-digit',
                                        minute: '2-digit',
                                    })}
                                </Typography>
                            )}
                            {replay.winner !== null && (
                                <Chip
                                    label={replay.winner}
                                    color={replay.winner === 'Draw' ? 'default' : 'warning'}
                                    size="small"
                                    sx={{ fontWeight: 700 }}
                                />
                            )}
                        </Box>

                        {/* Team chips */}
                        {replay.teams !== null && (
                            <Stack
                                direction="row"
                                spacing={1}
                                sx={{ mb: 2.5, flexWrap: 'wrap' }}
                                useFlexGap
                            >
                                {replay.teams.map((team) => (
                                    <Chip
                                        key={team}
                                        label={team}
                                        size="small"
                                        variant="outlined"
                                        sx={{ fontFamily: monoFontFamily, fontSize: 11 }}
                                    />
                                ))}
                                {league.version !== null && (
                                    <Chip
                                        label={`Scheme v${league.version}`}
                                        size="small"
                                        variant="outlined"
                                        sx={{ fontFamily: monoFontFamily, fontSize: 11 }}
                                    />
                                )}
                            </Stack>
                        )}

                        {/* Stats strip */}
                        {hasTurns && (
                            <Box
                                sx={{
                                    display: 'grid',
                                    gridTemplateColumns: 'repeat(4, 1fr)',
                                    gap: 1.5,
                                }}
                            >
                                {[
                                    ['Duration', formatDuration(duration)],
                                    ['Turns', String(turns.length)],
                                    ['Max Damage', String(maxDamagePerTurn)],
                                    ['Kills', String(totalKills)],
                                ].map(([label, value]) => (
                                    <Paper key={label} variant="outlined" sx={{ p: 1.5 }}>
                                        <Typography
                                            variant="caption"
                                            color="text.secondary"
                                            sx={{
                                                display: 'block',
                                                letterSpacing: '0.12em',
                                                textTransform: 'uppercase',
                                            }}
                                        >
                                            {label}
                                        </Typography>
                                        <Typography
                                            sx={{
                                                fontFamily: monoFontFamily,
                                                fontWeight: 700,
                                                fontSize: 22,
                                                mt: 0.25,
                                            }}
                                        >
                                            {value}
                                        </Typography>
                                    </Paper>
                                ))}
                            </Box>
                        )}
                    </Paper>

                    {/* Sidebar layout */}
                    <Box
                        sx={{
                            display: 'grid',
                            gridTemplateColumns: '200px 1fr',
                            gap: 2,
                            alignItems: 'start',
                        }}
                    >
                        {/* Left nav */}
                        <Paper variant="outlined" sx={{ p: 0.75 }}>
                            <List dense disablePadding>
                                {panels.map((panel, i) => (
                                    <ListItemButton
                                        key={i}
                                        selected={activePanel === i}
                                        onClick={() => setActivePanel(i)}
                                        sx={{ borderRadius: 1, mb: 0.25 }}
                                    >
                                        <ListItemIcon sx={{ minWidth: 32 }}>
                                            {panel.icon}
                                        </ListItemIcon>
                                        <ListItemText
                                            primary={panel.label}
                                            slotProps={{
                                                primary: {
                                                    style: {
                                                        fontSize: 13,
                                                        fontWeight: activePanel === i ? 700 : 400,
                                                    },
                                                },
                                            }}
                                        />
                                    </ListItemButton>
                                ))}
                            </List>
                        </Paper>

                        {/* Right content */}
                        <Box>{panels[activePanel].content}</Box>
                    </Box>
                </Box>
            )}
        </Container>
    )
}

export default GameDetailPage
