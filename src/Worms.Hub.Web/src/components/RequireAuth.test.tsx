import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router'
import type { AuthContextProps } from 'react-oidc-context'
import { useAuth } from 'react-oidc-context'
import RequireAuth from './RequireAuth'

vi.mock('react-oidc-context', () => ({
    useAuth: vi.fn(),
}))

const mockUseAuth = vi.mocked(useAuth)

const mockAuth = (isLoading: boolean, isAuthenticated: boolean): AuthContextProps =>
    ({ isLoading, isAuthenticated }) as unknown as AuthContextProps

describe('RequireAuth', () => {
    it('renders nothing while auth is loading', () => {
        mockUseAuth.mockReturnValue(mockAuth(true, false))

        const { container } = render(
            <MemoryRouter>
                <RequireAuth>
                    <div>protected</div>
                </RequireAuth>
            </MemoryRouter>,
        )

        expect(container).toBeEmptyDOMElement()
    })

    it('redirects to / when not authenticated', () => {
        mockUseAuth.mockReturnValue(mockAuth(false, false))

        render(
            <MemoryRouter initialEntries={['/authenticated']}>
                <Routes>
                    <Route
                        path="/authenticated"
                        element={
                            <RequireAuth>
                                <div>protected</div>
                            </RequireAuth>
                        }
                    />
                    <Route path="/" element={<div>landing</div>} />
                </Routes>
            </MemoryRouter>,
        )

        expect(screen.getByText('landing')).toBeInTheDocument()
        expect(screen.queryByText('protected')).not.toBeInTheDocument()
    })

    it('renders children when authenticated', () => {
        mockUseAuth.mockReturnValue(mockAuth(false, true))

        render(
            <MemoryRouter>
                <RequireAuth>
                    <div>protected</div>
                </RequireAuth>
            </MemoryRouter>,
        )

        expect(screen.getByText('protected')).toBeInTheDocument()
    })
})
