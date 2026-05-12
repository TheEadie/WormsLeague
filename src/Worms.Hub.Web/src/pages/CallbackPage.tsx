import { useEffect } from 'react'
import { useNavigate } from 'react-router'
import { useAuth } from 'react-oidc-context'
import Box from '@mui/material/Box'
import CircularProgress from '@mui/material/CircularProgress'
import Typography from '@mui/material/Typography'

function CallbackPage() {
    const auth = useAuth()
    const navigate = useNavigate()

    useEffect(() => {
        if (auth.isAuthenticated) {
            void navigate('/leagues', { replace: true })
        }
    }, [auth.isAuthenticated, navigate])

    return (
        <Box
            sx={{
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                justifyContent: 'center',
                flex: 1,
                gap: 2,
            }}
        >
            <CircularProgress />
            <Typography color="text.secondary">Completing sign-in…</Typography>
        </Box>
    )
}

export default CallbackPage
