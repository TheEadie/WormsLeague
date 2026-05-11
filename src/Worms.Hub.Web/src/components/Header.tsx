import { useState } from 'react'
import { useAuth } from 'react-oidc-context'
import AppBar from '@mui/material/AppBar'
import Toolbar from '@mui/material/Toolbar'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Menu from '@mui/material/Menu'
import MenuItem from '@mui/material/MenuItem'
import Brand from './Brand'
import ColourSchemePicker from './ColourSchemePicker'

function Header() {
    const auth = useAuth()
    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null)

    const username =
        auth.user?.profile.nickname ?? auth.user?.profile.name ?? auth.user?.profile.sub

    function handleOpen(event: React.MouseEvent<HTMLButtonElement>) {
        setAnchorEl(event.currentTarget)
    }

    function handleClose() {
        setAnchorEl(null)
    }

    function handleSignOut() {
        handleClose()
        void auth.signoutRedirect()
    }

    return (
        <AppBar
            position="sticky"
            color="default"
            elevation={0}
            sx={{ borderBottom: 1, borderColor: 'divider', bgcolor: 'background.paper' }}
        >
            <Toolbar variant="dense" sx={{ gap: 1.5, minHeight: 52 }}>
                <Brand />
                <Box sx={{ flexGrow: 1 }} />
                {auth.isAuthenticated && username !== undefined && (
                    <>
                        <Button color="inherit" onClick={handleOpen} size="small">
                            {username}
                        </Button>
                        <Menu anchorEl={anchorEl} open={Boolean(anchorEl)} onClose={handleClose}>
                            <MenuItem onClick={handleSignOut}>Sign out</MenuItem>
                        </Menu>
                    </>
                )}
                <ColourSchemePicker />
            </Toolbar>
        </AppBar>
    )
}

export default Header
