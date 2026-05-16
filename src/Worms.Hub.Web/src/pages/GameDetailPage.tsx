import { useEffect, useState } from 'react'
import { useParams, Link as RouterLink } from 'react-router'
import { useAuth } from 'react-oidc-context'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Breadcrumbs from '@mui/material/Breadcrumbs'
import Chip from '@mui/material/Chip'
import CircularProgress from '@mui/material/CircularProgress'
import Container from '@mui/material/Container'
import Divider from '@mui/material/Divider'
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

interface PlacementDto {
    machine: string
    teamName: string
    position: number | null
}

interface ReplayDetailDto {
    id: string
    name: string
    status: string
    date: string | null
    winner: string | null
    teams: string[] | null
    turns: TurnDto[] | null
    placements: PlacementDto[] | null
}

interface LeagueDto {
    id: string
    name: string
    version: string | null
    schemeUrl: string | null
}

interface TeamDto {
    id: number
    machine: string
    teamName: string
    claimedBy: string | null
    isMyTeam: boolean
}

interface PlacementPillProps {
    placement: PlacementDto
    index: number
    unclaimedTeam: TeamDto | undefined
    pendingClaim: Set<number>
    onClaim: (id: number) => void
}

function PlacementPill({
    placement,
    index,
    unclaimedTeam,
    pendingClaim,
    onClaim,
}: PlacementPillProps) {
    const place = placement.position ?? index + 1
    const isWin = place === 1
    const medal = ['#ffca28', '#bdbdbd', '#cd7f32'][place - 1]
    return (
        <Paper
            variant="outlined"
            sx={{
                display: 'flex',
                alignItems: 'center',
                gap: 1.25,
                pl: 0.5,
                pr: 2,
                py: 0.75,
                borderRadius: 99,
                ...(isWin
                    ? {
                          borderColor: 'rgba(255,202,40,0.5)',
                          bgcolor: 'rgba(255,202,40,0.12)',
                      }
                    : {}),
            }}
        >
            <Box
                sx={{
                    width: 28,
                    height: 28,
                    borderRadius: '50%',
                    bgcolor: medal ?? 'action.disabledBackground',
                    color: '#000',
                    display: 'grid',
                    placeItems: 'center',
                    fontFamily: monoFontFamily,
                    fontWeight: 700,
                    fontSize: 13,
                    flexShrink: 0,
                }}
            >
                {placement.position ?? '?'}
            </Box>
            <Typography sx={{ fontWeight: 700, fontSize: 13 }}>{placement.teamName}</Typography>
            {unclaimedTeam && (
                <Button
                    size="small"
                    variant="outlined"
                    disabled={pendingClaim.has(unclaimedTeam.id)}
                    onClick={() => onClaim(unclaimedTeam.id)}
                    sx={{
                        ml: 0.5,
                        height: 22,
                        fontSize: 11,
                        px: 1,
                        py: 0,
                        minWidth: 0,
                    }}
                >
                    Claim
                </Button>
            )}
        </Paper>
    )
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
                    {turns.map((turn) => {
                        const hasKill = turn.damage.some((d) => d.wormsKilled > 0)
                        return (
                            <TableRow
                                key={turn.turnNumber}
                                sx={{ bgcolor: hasKill ? 'rgba(211,47,47,0.08)' : 'transparent' }}
                            >
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
                                                />
                                            ))}
                                        </Stack>
                                    )}
                                </TableCell>
                                <TableCell>
                                    {turn.damage.length === 0 ? (
                                        <Typography variant="caption" color="text.disabled">
                                            — no damage —
                                        </Typography>
                                    ) : (
                                        <Stack
                                            direction="row"
                                            spacing={1}
                                            sx={{ flexWrap: 'wrap' }}
                                            useFlexGap
                                        >
                                            {turn.damage.map((d) => (
                                                <Box
                                                    key={d.teamName}
                                                    sx={{
                                                        display: 'flex',
                                                        alignItems: 'center',
                                                        gap: 0.5,
                                                    }}
                                                >
                                                    <Typography
                                                        sx={{
                                                            fontFamily: monoFontFamily,
                                                            fontSize: 12,
                                                            color: 'text.secondary',
                                                        }}
                                                    >
                                                        {d.teamName}:
                                                    </Typography>
                                                    <Typography
                                                        sx={{
                                                            fontFamily: monoFontFamily,
                                                            fontSize: 12,
                                                            fontWeight: 700,
                                                            color: 'primary.light',
                                                        }}
                                                    >
                                                        {d.healthLost}
                                                    </Typography>
                                                    {d.wormsKilled > 0 && (
                                                        <Chip
                                                            label={`+${d.wormsKilled} kill${d.wormsKilled > 1 ? 's' : ''}`}
                                                            size="small"
                                                            color="error"
                                                            sx={{
                                                                height: 16,
                                                                fontSize: 9,
                                                                fontWeight: 700,
                                                            }}
                                                        />
                                                    )}
                                                </Box>
                                            ))}
                                        </Stack>
                                    )}
                                </TableCell>
                            </TableRow>
                        )
                    })}
                </TableBody>
            </Table>
        </TableContainer>
    )
}

type TeamBreakdown = {
    teamName: string
    usageCount: number
    attributedDamage: number
    attributedKills: number
}
type WeaponCard = {
    name: string
    totalUses: number
    totalDamage: number
    totalKills: number
    byTeam: TeamBreakdown[]
}

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

    const weaponMap = new Map<string, WeaponCard>()

    for (const turn of turns) {
        for (const weapon of turn.weapons) {
            if (!weaponMap.has(weapon.name)) {
                weaponMap.set(weapon.name, {
                    name: weapon.name,
                    totalUses: 0,
                    totalDamage: 0,
                    totalKills: 0,
                    byTeam: [],
                })
            }
            const card = weaponMap.get(weapon.name)!
            card.totalUses++
            let teamEntry = card.byTeam.find((t) => t.teamName === turn.teamName)
            if (!teamEntry) {
                teamEntry = {
                    teamName: turn.teamName,
                    usageCount: 0,
                    attributedDamage: 0,
                    attributedKills: 0,
                }
                card.byTeam.push(teamEntry)
            }
            teamEntry.usageCount++
        }

        if (turn.weapons.length > 0) {
            const lastWeapon = turn.weapons[turn.weapons.length - 1]
            const turnDamage = turn.damage.reduce((sum, d) => sum + d.healthLost, 0)
            const turnKills = turn.damage.reduce((sum, d) => sum + d.wormsKilled, 0)
            const card = weaponMap.get(lastWeapon.name)!
            card.totalDamage += turnDamage
            card.totalKills += turnKills
            const teamEntry = card.byTeam.find((t) => t.teamName === turn.teamName)
            if (teamEntry) {
                teamEntry.attributedDamage += turnDamage
                teamEntry.attributedKills += turnKills
            }
        }
    }

    const weapons = Array.from(weaponMap.values()).sort(
        (a, b) => b.totalDamage - a.totalDamage || b.totalUses - a.totalUses,
    )

    return (
        <Box
            sx={{
                display: 'grid',
                gridTemplateColumns: 'repeat(4, 1fr)',
                gap: 2,
            }}
        >
            {weapons.map((weapon) => (
                <Paper key={weapon.name} variant="outlined" sx={{ p: 2 }}>
                    <Typography variant="subtitle2" sx={{ fontWeight: 700, mb: 1.5 }}>
                        {weapon.name}
                    </Typography>
                    <Box sx={{ display: 'flex', mb: 1.5 }}>
                        {(
                            [
                                ['Uses', weapon.totalUses, undefined],
                                ['Damage', weapon.totalDamage, 'primary.main'],
                                ['Kills', weapon.totalKills, undefined],
                            ] as const
                        ).map(([label, value, color]) => (
                            <Box key={label} sx={{ flex: 1 }}>
                                <Typography
                                    variant="caption"
                                    color="text.secondary"
                                    sx={{
                                        display: 'block',
                                        textTransform: 'uppercase',
                                        letterSpacing: '0.1em',
                                    }}
                                >
                                    {label}
                                </Typography>
                                <Typography
                                    sx={{
                                        fontFamily: monoFontFamily,
                                        fontWeight: 700,
                                        fontSize: 20,
                                        color: color ?? 'text.primary',
                                    }}
                                >
                                    {value}
                                </Typography>
                            </Box>
                        ))}
                    </Box>
                    <Divider sx={{ mb: 1 }} />
                    <Stack spacing={0.5}>
                        {weapon.byTeam.map((team) => (
                            <Box
                                key={team.teamName}
                                sx={{
                                    display: 'flex',
                                    justifyContent: 'space-between',
                                    alignItems: 'center',
                                }}
                            >
                                <Typography variant="caption" color="text.secondary">
                                    {team.teamName}
                                </Typography>
                                <Typography variant="caption" sx={{ fontFamily: monoFontFamily }}>
                                    {team.usageCount}× · {team.attributedDamage}dmg ·{' '}
                                    {team.attributedKills}k
                                </Typography>
                            </Box>
                        ))}
                    </Stack>
                </Paper>
            ))}
        </Box>
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
    const [teams, setTeams] = useState<TeamDto[] | null>(null)
    const [teamsRefetchKey, setTeamsRefetchKey] = useState(0)
    const [pendingClaim, setPendingClaim] = useState<Set<number>>(new Set())

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

    useEffect(() => {
        if (!auth.user?.access_token) return
        const token = auth.user.access_token
        fetch(`${gatewayUrl}/api/v1/teams`, {
            headers: { Authorization: `Bearer ${token}` },
        })
            .then((res) => {
                if (!res.ok) throw new Error(`HTTP ${res.status}`)
                return res.json() as Promise<TeamDto[]>
            })
            .then((data) => setTeams(data))
            .catch(() => {
                // Silently omit Claim buttons on failure — leave teams as null
            })
    }, [auth.user?.access_token, teamsRefetchKey])

    async function handleClaim(id: number) {
        setPendingClaim((prev) => new Set(prev).add(id))
        try {
            const res = await fetch(`${gatewayUrl}/api/v1/teams`, {
                method: 'PUT',
                headers: {
                    Authorization: `Bearer ${auth.user!.access_token}`,
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ id, claimed: true }),
            })
            if (res.ok) {
                setTeamsRefetchKey((k) => k + 1)
            }
            // On non-OK response, button silently re-enables (no error shown)
        } catch {
            // Network error — silently re-enable
        } finally {
            setPendingClaim((prev) => {
                const s = new Set(prev)
                s.delete(id)
                return s
            })
        }
    }

    const unclaimedTeamsByKey = new Map<string, TeamDto>()
    if (teams !== null && replay?.placements) {
        for (const team of teams) {
            if (team.claimedBy === null) {
                const key = `${team.machine}\0${team.teamName}`
                unclaimedTeamsByKey.set(key, team)
            }
        }
    }

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
                            {league.version !== null && (
                                <Chip
                                    label={`Scheme v${league.version}`}
                                    size="small"
                                    variant="outlined"
                                    sx={{ fontFamily: monoFontFamily, fontSize: 11 }}
                                />
                            )}
                            {replay.winner !== null && replay.placements === null && (
                                <Chip
                                    label={replay.winner}
                                    color={replay.winner === 'Draw' ? 'default' : 'warning'}
                                    size="small"
                                    sx={{ fontWeight: 700 }}
                                />
                            )}
                        </Box>

                        {/* Finishing order */}
                        {(replay.teams !== null || replay.placements !== null) && (
                            <Box sx={{ mb: 2.5 }}>
                                {replay.placements !== null && replay.placements.length > 0 && (
                                    <Typography
                                        variant="overline"
                                        color="text.secondary"
                                        sx={{ letterSpacing: '0.12em', display: 'block' }}
                                    >
                                        Result
                                    </Typography>
                                )}
                                <Stack
                                    direction="row"
                                    spacing={1}
                                    sx={{ mt: 0.5, flexWrap: 'wrap' }}
                                    useFlexGap
                                >
                                    {replay.placements !== null && replay.placements.length > 0
                                        ? replay.placements
                                              .slice()
                                              .sort((a, b) => {
                                                  if (a.position === null && b.position === null)
                                                      return 0
                                                  if (a.position === null) return 1
                                                  if (b.position === null) return -1
                                                  return a.position - b.position
                                              })
                                              .map((p, i) => (
                                                  <PlacementPill
                                                      key={`${p.machine}-${p.teamName}`}
                                                      placement={p}
                                                      index={i}
                                                      unclaimedTeam={unclaimedTeamsByKey.get(
                                                          `${p.machine}\0${p.teamName}`,
                                                      )}
                                                      pendingClaim={pendingClaim}
                                                      onClaim={(id) => void handleClaim(id)}
                                                  />
                                              ))
                                        : (replay.teams ?? []).map((label) => (
                                              <Chip
                                                  key={label}
                                                  label={label}
                                                  size="small"
                                                  variant="outlined"
                                                  sx={{ fontFamily: monoFontFamily, fontSize: 11 }}
                                              />
                                          ))}
                                </Stack>
                            </Box>
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
