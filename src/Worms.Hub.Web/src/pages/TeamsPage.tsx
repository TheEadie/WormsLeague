import { useEffect, useState } from 'react'
import { useAuth } from 'react-oidc-context'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import CircularProgress from '@mui/material/CircularProgress'
import Container from '@mui/material/Container'
import Paper from '@mui/material/Paper'
import Table from '@mui/material/Table'
import TableBody from '@mui/material/TableBody'
import TableCell from '@mui/material/TableCell'
import TableContainer from '@mui/material/TableContainer'
import TableHead from '@mui/material/TableHead'
import TableRow from '@mui/material/TableRow'
import Typography from '@mui/material/Typography'
import { gatewayUrl } from '../api'

interface TeamDto {
    id: number
    machine: string
    teamName: string
    claimedBy: string | null
    isMyTeam: boolean
}

function sortGroup(team: TeamDto): number {
    if (!team.claimedBy) return 0
    if (team.isMyTeam) return 1
    return 2
}

function TeamsPage() {
    const auth = useAuth()
    const [teams, setTeams] = useState<TeamDto[] | null>(null)
    const [loadError, setLoadError] = useState<string | null>(null)
    const [pending, setPending] = useState<Set<number>>(new Set())
    const [errors, setErrors] = useState<Map<number, string>>(new Map())
    const [refetchKey, setRefetchKey] = useState(0)

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
            .then((data) => {
                setTeams(data)
                setLoadError(null)
            })
            .catch((err: unknown) => setLoadError(String(err)))
    }, [auth.user?.access_token, refetchKey])

    async function handleClaim(id: number, claimed: boolean) {
        setPending((prev) => new Set(prev).add(id))
        setErrors((prev) => {
            const m = new Map(prev)
            m.delete(id)
            return m
        })
        try {
            const res = await fetch(`${gatewayUrl}/api/v1/teams`, {
                method: 'PUT',
                headers: {
                    Authorization: `Bearer ${auth.user!.access_token}`,
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ id, claimed }),
            })
            if (!res.ok) {
                const msg =
                    res.status === 409
                        ? 'Already claimed by another player'
                        : res.status === 403
                          ? "You don't own this team"
                          : 'Something went wrong, please try again'
                setErrors((prev) => new Map(prev).set(id, msg))
            } else {
                setErrors(new Map())
                setRefetchKey((k) => k + 1)
            }
        } catch {
            setErrors((prev) => new Map(prev).set(id, 'Something went wrong, please try again'))
        } finally {
            setPending((prev) => {
                const s = new Set(prev)
                s.delete(id)
                return s
            })
        }
    }

    const sorted =
        teams === null
            ? null
            : [...teams].sort((a, b) => {
                  const groupDiff = sortGroup(a) - sortGroup(b)
                  if (groupDiff !== 0) return groupDiff
                  const machineDiff = a.machine.localeCompare(b.machine)
                  if (machineDiff !== 0) return machineDiff
                  return a.teamName.localeCompare(b.teamName)
              })

    return (
        <Box sx={{ flex: 1 }}>
            <Container maxWidth="xl" sx={{ py: { xs: 2, md: 4 } }}>
                <Box sx={{ mb: 4 }}>
                    <Typography variant="h4" sx={{ fontWeight: 700 }}>
                        Teams
                    </Typography>
                </Box>

                {sorted === null && loadError === null && <CircularProgress />}

                {loadError !== null && (
                    <Typography color="error">Error loading teams: {loadError}</Typography>
                )}

                {sorted !== null && sorted.length === 0 && (
                    <Typography color="text.secondary">
                        No teams found. Teams appear here once replays have been processed.
                    </Typography>
                )}

                {sorted !== null && sorted.length > 0 && (
                    <TableContainer component={Paper}>
                        <Table>
                            <TableHead>
                                <TableRow>
                                    <TableCell>Machine</TableCell>
                                    <TableCell>Team Name</TableCell>
                                    <TableCell>Status / Action</TableCell>
                                </TableRow>
                            </TableHead>
                            <TableBody>
                                {sorted.map((team) => (
                                    <TableRow key={team.id}>
                                        <TableCell>{team.machine}</TableCell>
                                        <TableCell>{team.teamName}</TableCell>
                                        <TableCell>
                                            {!team.claimedBy && (
                                                <>
                                                    <Button
                                                        size="small"
                                                        variant="outlined"
                                                        disabled={pending.has(team.id)}
                                                        onClick={() =>
                                                            void handleClaim(team.id, true)
                                                        }
                                                    >
                                                        Claim
                                                    </Button>
                                                    {errors.has(team.id) && (
                                                        <Typography
                                                            variant="caption"
                                                            color="error"
                                                            sx={{ ml: 1 }}
                                                        >
                                                            {errors.get(team.id)}
                                                        </Typography>
                                                    )}
                                                </>
                                            )}
                                            {team.isMyTeam && (
                                                <>
                                                    <Button
                                                        size="small"
                                                        variant="outlined"
                                                        disabled={pending.has(team.id)}
                                                        onClick={() =>
                                                            void handleClaim(team.id, false)
                                                        }
                                                    >
                                                        Unclaim
                                                    </Button>
                                                    {errors.has(team.id) && (
                                                        <Typography
                                                            variant="caption"
                                                            color="error"
                                                            sx={{ ml: 1 }}
                                                        >
                                                            {errors.get(team.id)}
                                                        </Typography>
                                                    )}
                                                </>
                                            )}
                                            {team.claimedBy && !team.isMyTeam && (
                                                <Typography variant="body2" color="text.secondary">
                                                    Claimed by {team.claimedBy}
                                                </Typography>
                                            )}
                                        </TableCell>
                                    </TableRow>
                                ))}
                            </TableBody>
                        </Table>
                    </TableContainer>
                )}
            </Container>
        </Box>
    )
}

export default TeamsPage
