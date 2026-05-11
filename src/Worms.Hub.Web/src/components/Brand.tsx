import Box from '@mui/material/Box'
import Typography from '@mui/material/Typography'
import { monoFontFamily } from '../theme'

function Brand() {
    return (
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <Box
                component="img"
                src="/worm.png"
                alt=""
                sx={{ height: 26, width: 'auto', display: 'block' }}
            />
            <Typography
                component="span"
                sx={{
                    fontFamily: monoFontFamily,
                    fontWeight: 700,
                    letterSpacing: '0.06em',
                    textTransform: 'uppercase',
                    fontSize: 14,
                }}
            >
                Worms Hub
            </Typography>
        </Box>
    )
}

export default Brand
