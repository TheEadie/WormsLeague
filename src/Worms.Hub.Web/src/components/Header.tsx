import AppBar from '@mui/material/AppBar'
import Toolbar from '@mui/material/Toolbar'
import Typography from '@mui/material/Typography'
import Box from '@mui/material/Box'
import ColourSchemePicker from './ColourSchemePicker'

function Header() {
    return (
        <AppBar position="static">
            <Toolbar>
                <Typography variant="h6" component="div">
                    Worms Hub
                </Typography>
                <Box sx={{ flexGrow: 1 }} />
                <ColourSchemePicker />
            </Toolbar>
        </AppBar>
    )
}

export default Header
