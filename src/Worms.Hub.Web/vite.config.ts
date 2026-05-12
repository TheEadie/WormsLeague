import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'

export default defineConfig({
    plugins: [react()],
    build: {
        outDir: '../../.artifacts/web/',
        emptyOutDir: true,
    },
    test: {
        globals: true,
        environment: 'jsdom',
        setupFiles: ['./src/test-setup.ts'],
        clearMocks: true,
    },
})
