import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import ReplayLogPanel from './ReplayLogPanel'

describe('ReplayLogPanel', () => {
    it('renders the supplied log text with line breaks preserved', () => {
        const log = 'first line\nsecond line\nthird line'
        render(<ReplayLogPanel log={log} />)

        const pre = screen.getByText((_, node) => node?.tagName === 'PRE')
        expect(pre.textContent).toBe(log)
    })

    it('shows the empty state when log is null', () => {
        render(<ReplayLogPanel log={null} />)
        expect(screen.getByText('No replay log available.')).toBeInTheDocument()
    })

    it('shows the empty state when log is an empty string', () => {
        render(<ReplayLogPanel log="" />)
        expect(screen.getByText('No replay log available.')).toBeInTheDocument()
    })
})
