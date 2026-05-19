import Paper from '@mui/material/Paper'
import Typography from '@mui/material/Typography'
import { monoFontFamily } from '../theme'

interface ReplayLogPanelProps {
    log: string | null
}

function ReplayLogPanel({ log }: ReplayLogPanelProps) {
    if (log === null || log.length === 0) {
        return (
            <Paper variant="outlined" sx={{ p: 3, textAlign: 'center' }}>
                <Typography color="text.secondary">No replay log available.</Typography>
            </Paper>
        )
    }

    return (
        <Paper variant="outlined" sx={{ p: 2 }}>
            <Typography
                component="pre"
                sx={{
                    fontFamily: monoFontFamily,
                    fontSize: 12,
                    margin: 0,
                    whiteSpace: 'pre',
                }}
            >
                {log}
            </Typography>
        </Paper>
    )
}

export default ReplayLogPanel
