import AppBar from '@mui/material/AppBar'
import Toolbar from '@mui/material/Toolbar'
import Box from '@mui/material/Box'
import Brand from './Brand'
import ColourSchemePicker from './ColourSchemePicker'

function Header() {
    return (
        <AppBar
            position="sticky"
            color="default"
            elevation={0}
            sx={{
                borderBottom: 1,
                borderColor: 'divider',
                bgcolor: 'background.paper',
            }}
        >
            <Toolbar variant="dense" sx={{ gap: 1.5, minHeight: 52 }}>
                <Brand />
                <Box sx={{ flexGrow: 1 }} />
                <ColourSchemePicker />
            </Toolbar>
        </AppBar>
    )
}

export default Header
