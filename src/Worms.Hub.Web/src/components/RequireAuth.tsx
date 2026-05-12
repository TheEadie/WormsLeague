import { useAuth } from 'react-oidc-context'
import { Navigate } from 'react-router'

interface RequireAuthProps {
    children: React.ReactNode
}

function RequireAuth({ children }: RequireAuthProps) {
    const auth = useAuth()

    if (auth.isLoading) {
        return null
    }

    if (!auth.isAuthenticated) {
        return <Navigate to="/" replace />
    }

    return <>{children}</>
}

export default RequireAuth
