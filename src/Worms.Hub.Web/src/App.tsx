import { createBrowserRouter, RouterProvider } from 'react-router'
import Layout from './components/Layout'
import RequireAuth from './components/RequireAuth'
import LandingPage from './pages/LandingPage'
import CallbackPage from './pages/CallbackPage'
import LeagueListPage from './pages/LeagueListPage'

const router = createBrowserRouter([
    {
        path: '/',
        element: <Layout />,
        children: [
            { index: true, element: <LandingPage /> },
            { path: 'callback', element: <CallbackPage /> },
            {
                path: 'leagues',
                element: (
                    <RequireAuth>
                        <LeagueListPage />
                    </RequireAuth>
                ),
            },
            {
                path: 'leagues/:id',
                element: (
                    <RequireAuth>
                        <div>League detail — coming soon</div>
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
