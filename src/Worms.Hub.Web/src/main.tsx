import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import '@fontsource/jetbrains-mono/400.css'
import '@fontsource/jetbrains-mono/500.css'
import '@fontsource/jetbrains-mono/700.css'
import { ThemeProvider } from '@mui/material/styles'
import CssBaseline from '@mui/material/CssBaseline'
import App from './App.tsx'
import theme from './theme.ts'

createRoot(document.getElementById('root')!).render(
    <StrictMode>
        <ThemeProvider theme={theme} defaultMode="system" noSsr>
            <CssBaseline />
            <App />
        </ThemeProvider>
    </StrictMode>,
)
