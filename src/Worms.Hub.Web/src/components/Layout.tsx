import { Outlet } from 'react-router'
import Header from './Header'
import Footer from './Footer'
import Box from '@mui/material/Box'

function Layout() {
    return (
        <Box sx={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
            <Header />
            <Box component="main" sx={{ flex: 1, display: 'flex', flexDirection: 'column' }}>
                <Outlet />
            </Box>
            <Footer />
        </Box>
    )
}

export default Layout
