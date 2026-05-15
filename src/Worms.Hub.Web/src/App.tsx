import { createBrowserRouter, RouterProvider } from 'react-router'
import Layout from './components/Layout'
import RequireAuth from './components/RequireAuth'
import LandingPage from './pages/LandingPage'
import CallbackPage from './pages/CallbackPage'
import LeagueListPage from './pages/LeagueListPage'
import LeagueDetailPage from './pages/LeagueDetailPage'
import GameDetailPage from './pages/GameDetailPage'
import TeamsPage from './pages/TeamsPage'

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
                        <LeagueDetailPage />
                    </RequireAuth>
                ),
            },
            {
                path: 'leagues/:id/replays/:replayId',
                element: (
                    <RequireAuth>
                        <GameDetailPage />
                    </RequireAuth>
                ),
            },
            {
                path: 'teams',
                element: (
                    <RequireAuth>
                        <TeamsPage />
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
