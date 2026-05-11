import { useColorScheme } from '@mui/material/styles'
import IconButton from '@mui/material/IconButton'
import Tooltip from '@mui/material/Tooltip'
import LightModeIcon from '@mui/icons-material/LightMode'
import DarkModeIcon from '@mui/icons-material/DarkMode'
import SettingsBrightnessIcon from '@mui/icons-material/SettingsBrightness'

type Mode = 'light' | 'dark' | 'system'

const cycle: Mode[] = ['light', 'dark', 'system']

const labels: Record<Mode, string> = {
    light: 'Light (click to switch to Dark)',
    dark: 'Dark (click to switch to System)',
    system: 'System (click to switch to Light)',
}

function ColourSchemePicker() {
    const { mode, setMode } = useColorScheme()
    const resolvedMode = mode ?? 'system'

    function handleClick() {
        const next = cycle[(cycle.indexOf(resolvedMode) + 1) % cycle.length]
        setMode(next)
    }

    return (
        <Tooltip title={labels[resolvedMode]}>
            <IconButton onClick={handleClick} color="inherit" aria-label={labels[resolvedMode]}>
                {resolvedMode === 'light' && <LightModeIcon />}
                {resolvedMode === 'dark' && <DarkModeIcon />}
                {resolvedMode === 'system' && <SettingsBrightnessIcon />}
            </IconButton>
        </Tooltip>
    )
}

export default ColourSchemePicker
