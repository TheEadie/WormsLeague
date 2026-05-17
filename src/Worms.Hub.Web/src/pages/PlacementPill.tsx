import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Paper from '@mui/material/Paper'
import Typography from '@mui/material/Typography'
import { monoFontFamily } from '../theme'

export interface PlacementDto {
    machine: string
    teamName: string
    position: number | null
    playerName: string | null
}

export interface TeamDto {
    id: number
    machine: string
    teamName: string
    claimedBy: string | null
    isMyTeam: boolean
}

interface PlacementPillProps {
    placement: PlacementDto
    index: number
    unclaimedTeam: TeamDto | undefined
    pendingClaim: Set<number>
    onClaim: (id: number) => void
}

export function PlacementPill({
    placement,
    index,
    unclaimedTeam,
    pendingClaim,
    onClaim,
}: PlacementPillProps) {
    const place = placement.position ?? index + 1
    const isWin = place === 1
    const medal = ['#ffca28', '#bdbdbd', '#cd7f32'][place - 1]
    return (
        <Paper
            variant="outlined"
            sx={{
                display: 'flex',
                alignItems: 'center',
                gap: 1.25,
                pl: 0.5,
                pr: 2,
                py: 0.75,
                borderRadius: 99,
                ...(isWin
                    ? {
                          borderColor: 'rgba(255,202,40,0.5)',
                          bgcolor: 'rgba(255,202,40,0.12)',
                      }
                    : {}),
            }}
        >
            <Box
                sx={{
                    width: 28,
                    height: 28,
                    borderRadius: '50%',
                    bgcolor: medal ?? 'action.disabledBackground',
                    color: '#000',
                    display: 'grid',
                    placeItems: 'center',
                    fontFamily: monoFontFamily,
                    fontWeight: 700,
                    fontSize: 13,
                    flexShrink: 0,
                }}
            >
                {placement.position ?? '?'}
            </Box>
            <Box sx={{ display: 'flex', flexDirection: 'column', lineHeight: 1.1 }}>
                {placement.playerName !== null ? (
                    <>
                        <Typography sx={{ fontWeight: 700, fontSize: 13 }}>{placement.playerName}</Typography>
                        <Typography sx={{ fontSize: 10, color: 'text.secondary' }}>
                            {placement.teamName}
                        </Typography>
                    </>
                ) : (
                    <Typography sx={{ fontWeight: 700, fontSize: 13 }}>{placement.teamName}</Typography>
                )}
            </Box>
            {unclaimedTeam && (
                <Button
                    size="small"
                    variant="outlined"
                    disabled={pendingClaim.has(unclaimedTeam.id)}
                    onClick={() => onClaim(unclaimedTeam.id)}
                    sx={{
                        ml: 0.5,
                        height: 22,
                        fontSize: 11,
                        px: 1,
                        py: 0,
                        minWidth: 0,
                    }}
                >
                    Claim
                </Button>
            )}
        </Paper>
    )
}
