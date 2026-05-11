import { createTheme } from '@mui/material/styles'

const monoFontFamily = '"JetBrains Mono", ui-monospace, SFMono-Regular, Menlo, Consolas, monospace'

const theme = createTheme({
    cssVariables: { colorSchemeSelector: 'data-mui-color-scheme' },
    colorSchemes: { light: true, dark: true },
    typography: {
        // MUI default Roboto/system stack is kept for body text.
        // Re-skin the `overline` variant as our accent caption so the
        // "LEAGUE ACCESS" divider and any future similar callouts inherit
        // it consistently.
        overline: {
            fontFamily: monoFontFamily,
            fontWeight: 500,
            letterSpacing: '0.16em',
        },
    },
})

export { monoFontFamily }
export default theme
