import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { createTheme, ThemeProvider } from '@mui/material/styles'
import CssBaseline from '@mui/material/CssBaseline'
import App from './App.tsx'

const theme = createTheme({
    cssVariables: { colorSchemeSelector: 'data-mui-color-scheme' },
    colorSchemes: { light: true, dark: true },
})

createRoot(document.getElementById('root')!).render(
    <StrictMode>
        <ThemeProvider theme={theme} defaultMode="system" noSsr>
            <CssBaseline />
            <App />
        </ThemeProvider>
    </StrictMode>,
)
