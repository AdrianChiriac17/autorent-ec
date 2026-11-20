import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Route, Routes } from 'react-router'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { tokenStorage } from '../../../lib/tokenStorage'
import { jsonResponse, renderWithProviders } from '../../../test/renderWithProviders'
import { LoginPage } from './LoginPage'

const user = {
  id: 1,
  email: 'ana@example.com',
  fullName: 'Ana Popescu',
  phone: null,
  isAdmin: false,
  addressLine: null,
  city: null,
  county: null,
  postalCode: null,
  country: null,
}

function renderLogin() {
  return renderWithProviders(
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/" element={<p>Home page</p>} />
    </Routes>,
    { route: '/login' },
  )
}

afterEach(() => vi.unstubAllGlobals())

describe('LoginPage', () => {
  it('logs in, stores the token and goes to the home page', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(
      jsonResponse(200, { accessToken: 'token-123', expiresAt: '2030-01-01T00:00:00Z', user }),
    ))
    renderLogin()

    await userEvent.type(screen.getByLabelText('Email'), 'ana@example.com')
    await userEvent.type(screen.getByLabelText('Password'), 'the-right-one')
    await userEvent.click(screen.getByRole('button', { name: 'Log in' }))

    expect(await screen.findByText('Home page')).toBeInTheDocument()
    expect(tokenStorage.get()).toBe('token-123')
  })

  it('shows the server message when the credentials are wrong', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(
      jsonResponse(401, { title: 'Email or password is incorrect.', code: 'auth.invalid_credentials' }),
    ))
    renderLogin()

    await userEvent.type(screen.getByLabelText('Email'), 'ana@example.com')
    await userEvent.type(screen.getByLabelText('Password'), 'wrong-password')
    await userEvent.click(screen.getByRole('button', { name: 'Log in' }))

    expect(await screen.findByRole('alert')).toHaveTextContent('Email or password is incorrect.')
    expect(tokenStorage.get()).toBeNull()
  })
})
