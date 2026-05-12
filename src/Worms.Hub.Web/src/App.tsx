import { createBrowserRouter, RouterProvider } from 'react-router'
import Layout from './components/Layout'
import RequireAuth from './components/RequireAuth'
import LandingPage from './pages/LandingPage'
import CallbackPage from './pages/CallbackPage'
import AuthenticatedPage from './pages/AuthenticatedPage'

const router = createBrowserRouter([
    {
        path: '/',
        element: <Layout />,
        children: [
            { index: true, element: <LandingPage /> },
            { path: 'callback', element: <CallbackPage /> },
            {
                path: 'authenticated',
                element: (
                    <RequireAuth>
                        <AuthenticatedPage />
                    </RequireAuth>
                ),
            },
        ],
    },
])

function App() {
    return <RouterProvider router={router} />
}

export default App
