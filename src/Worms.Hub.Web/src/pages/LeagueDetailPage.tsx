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
import { TeamDto } from './PlacementPill'

interface LeagueDto {
    id: string
    name: string
    version: string | null
    schemeUrl: string | null
}

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

interface ReplayInLeagueDto {
    id: string
    name: string
    status: string
    date: string | null
    winner: string | null
    teams: string[] | null
    turns: TurnDto[] | null
    placements: PlacementDto[] | null
}

function formatDuration(ms: number): string {
    const totalSeconds = Math.floor(ms / 1000)
    const minutes = Math.floor(totalSeconds / 60)
    const seconds = totalSeconds % 60
    return `${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`
}

function topWeaponsByDamage(turns: TurnDto[], n: number): string[] {
    const damage = new Map<string, number>()
    for (const turn of turns) {
        if (turn.weapons.length === 0) continue
        const lastWeapon = turn.weapons[turn.weapons.length - 1]
        const total = turn.damage.reduce((sum, d) => sum + d.healthLost, 0)
        damage.set(lastWeapon.name, (damage.get(lastWeapon.name) ?? 0) + total)
    }
    return Array.from(damage.entries())
        .sort((a, b) => b[1] - a[1])
        .slice(0, n)
        .map(([name]) => name)
}

function LeagueDetailPage() {
    const { id } = useParams<{ id: string }>()
    const navigate = useNavigate()
    const auth = useAuth()
    const [league, setLeague] = useState<LeagueDto | null>(null)
    const [replays, setReplays] = useState<ReplayInLeagueDto[] | null>(null)
    const [teams, setTeams] = useState<TeamDto[] | null>(null)
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

    useEffect(() => {
        if (!auth.user?.access_token) return
        fetch(`${gatewayUrl}/api/v1/teams`, {
            headers: { Authorization: `Bearer ${auth.user.access_token}` },
        })
            .then((res) => {
                if (!res.ok) return
                return res.json() as Promise<TeamDto[]>
            })
            .then((data) => { if (data) setTeams(data) })
            .catch(() => { /* silently omit — display falls back to team name */ })
    }, [auth.user?.access_token])

    const teamsByKey = new Map<string, TeamDto>()
    if (teams !== null) {
        for (const team of teams) {
            teamsByKey.set(`${team.machine}\0${team.teamName}`, team)
        }
    }

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
                                        <TableCell sx={{ fontWeight: 700, width: 420 }}>
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
                                                            {replay.placements !== null &&
                                                            replay.placements.length > 0
                                                                ? replay.placements
                                                                      .slice()
                                                                      .sort(
                                                                          (a, b) =>
                                                                              (a.position ??
                                                                                  Infinity) -
                                                                              (b.position ??
                                                                                  Infinity),
                                                                      )
                                                                      .map((p) => {
                                                                          const medal =
                                                                              p.position !== null
                                                                                  ? [
                                                                                        '#ffca28',
                                                                                        '#bdbdbd',
                                                                                        '#cd7f32',
                                                                                    ][
                                                                                        p.position -
                                                                                            1
                                                                                    ]
                                                                                  : undefined
                                                                          const isWin =
                                                                              p.position === 1
                                                                          return (
                                                                              <Box
                                                                                  key={`${p.machine}-${p.teamName}`}
                                                                                  sx={{
                                                                                      display:
                                                                                          'inline-flex',
                                                                                      alignItems:
                                                                                          'center',
                                                                                      gap: 0.5,
                                                                                      borderRadius: 99,
                                                                                      pl: 0.25,
                                                                                      pr: 0.75,
                                                                                      py: 0.25,
                                                                                      border: '1px solid',
                                                                                      borderColor:
                                                                                          isWin &&
                                                                                          medal
                                                                                              ? `${medal}88`
                                                                                              : 'divider',
                                                                                      bgcolor:
                                                                                          isWin &&
                                                                                          medal
                                                                                              ? `${medal}18`
                                                                                              : 'transparent',
                                                                                  }}
                                                                              >
                                                                                  <Box
                                                                                      sx={{
                                                                                          width: 18,
                                                                                          height: 18,
                                                                                          borderRadius:
                                                                                              '50%',
                                                                                          bgcolor:
                                                                                              medal ??
                                                                                              'action.disabledBackground',
                                                                                          display:
                                                                                              'grid',
                                                                                          placeItems:
                                                                                              'center',
                                                                                          fontFamily:
                                                                                              monoFontFamily,
                                                                                          fontSize: 9,
                                                                                          fontWeight: 700,
                                                                                          color: medal
                                                                                              ? '#000'
                                                                                              : 'text.secondary',
                                                                                      }}
                                                                                  >
                                                                                      {p.position}
                                                                                  </Box>
                                                                                  <Typography
                                                                                      sx={{
                                                                                          fontSize: 12,
                                                                                          fontWeight:
                                                                                              isWin
                                                                                                  ? 700
                                                                                                  : 500,
                                                                                      }}
                                                                                  >
                                                                                      {teamsByKey.get(`${p.machine}\0${p.teamName}`)?.claimedBy ?? p.teamName}
                                                                                  </Typography>
                                                                              </Box>
                                                                          )
                                                                      })
                                                                : replay.teams
                                                                      ?.slice()
                                                                      .sort((a) =>
                                                                          a === replay.winner
                                                                              ? -1
                                                                              : 1,
                                                                      )
                                                                      .map((team) => (
                                                                          <Box
                                                                              key={team}
                                                                              sx={{
                                                                                  display: 'flex',
                                                                                  alignItems:
                                                                                      'center',
                                                                                  gap: 0.5,
                                                                              }}
                                                                          >
                                                                              {team ===
                                                                                  replay.winner && (
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
                                                        {replay.turns && replay.turns.length > 0 ? (
                                                            <Stack
                                                                direction="row"
                                                                spacing={0.5}
                                                                sx={{ flexWrap: 'wrap' }}
                                                                useFlexGap
                                                            >
                                                                {topWeaponsByDamage(
                                                                    replay.turns,
                                                                    3,
                                                                ).map((w) => (
                                                                    <Chip
                                                                        key={w}
                                                                        label={w}
                                                                        size="small"
                                                                        variant="outlined"
                                                                        sx={{
                                                                            fontFamily:
                                                                                monoFontFamily,
                                                                            fontSize: 11,
                                                                        }}
                                                                    />
                                                                ))}
                                                            </Stack>
                                                        ) : (
                                                            <Typography
                                                                variant="caption"
                                                                color="text.disabled"
                                                            >
                                                                —
                                                            </Typography>
                                                        )}
                                                    </TableCell>
                                                    <TableCell>
                                                        {replay.turns && replay.turns.length > 0 ? (
                                                            <Typography
                                                                sx={{
                                                                    fontFamily: monoFontFamily,
                                                                    fontSize: 12,
                                                                }}
                                                            >
                                                                {formatDuration(
                                                                    replay.turns[
                                                                        replay.turns.length - 1
                                                                    ].endMs -
                                                                        replay.turns[0].startMs,
                                                                )}
                                                            </Typography>
                                                        ) : (
                                                            <Typography
                                                                variant="caption"
                                                                color="text.disabled"
                                                            >
                                                                —
                                                            </Typography>
                                                        )}
                                                    </TableCell>
                                                    <TableCell align="right">
                                                        {replay.turns && replay.turns.length > 0 ? (
                                                            <Typography
                                                                sx={{
                                                                    fontFamily: monoFontFamily,
                                                                    fontSize: 16,
                                                                    fontWeight: 700,
                                                                    color: 'primary.main',
                                                                }}
                                                            >
                                                                {Math.max(
                                                                    ...replay.turns.map((t) =>
                                                                        t.damage.reduce(
                                                                            (sum, d) =>
                                                                                sum + d.healthLost,
                                                                            0,
                                                                        ),
                                                                    ),
                                                                )}
                                                            </Typography>
                                                        ) : (
                                                            <Typography
                                                                variant="caption"
                                                                color="text.disabled"
                                                            >
                                                                —
                                                            </Typography>
                                                        )}
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
