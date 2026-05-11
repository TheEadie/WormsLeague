import { useEffect, useState } from 'react'
import { useAuth } from 'react-oidc-context'
import Box from '@mui/material/Box'
import Typography from '@mui/material/Typography'
import CircularProgress from '@mui/material/CircularProgress'
import { gatewayUrl } from '../api'

function AuthenticatedPage() {
    const auth = useAuth()
    const [games, setGames] = useState<unknown[] | null>(null)
    const [error, setError] = useState<string | null>(null)

    useEffect(() => {
        if (!auth.user?.access_token) return
        fetch(`${gatewayUrl}/api/v1/games`, {
            headers: { Authorization: `Bearer ${auth.user.access_token}` },
        })
            .then((res) => {
                if (!res.ok) throw new Error(`HTTP ${res.status}`)
                return res.json() as Promise<unknown[]>
            })
            .then(setGames)
            .catch((err: unknown) => setError(String(err)))
    }, [auth.user?.access_token])

    return (
        <Box sx={{ p: 4 }}>
            <Typography variant="h4" sx={{ mb: 2 }}>
                Authenticated
            </Typography>
            {games === null && error === null && <CircularProgress />}
            {error !== null && <Typography color="error">Error fetching games: {error}</Typography>}
            {games !== null && games.length === 0 && (
                <Typography color="text.secondary">No games found.</Typography>
            )}
            {games !== null && games.length > 0 && (
                <Box component="pre" sx={{ fontSize: 12, overflowX: 'auto' }}>
                    {JSON.stringify(games, null, 2)}
                </Box>
            )}
        </Box>
    )
}

export default AuthenticatedPage
