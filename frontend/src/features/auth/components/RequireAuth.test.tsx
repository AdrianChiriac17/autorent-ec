import { screen } from '@testing-library/react'
import { Route, Routes } from 'react-router'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { tokenStorage } from '../../../lib/tokenStorage'
import { jsonResponse, renderWithProviders } from '../../../test/renderWithProviders'
import { RequireAuth } from './RequireAuth'

function renderAdminRoute() {
  return renderWithProviders(
    <Routes>
      <Route path="/login" element={<p>Login page</p>} />
      <Route
        path="/admin/users"
        element={
          <RequireAuth admin>
            <p>Secret admin content</p>
          </RequireAuth>
        }
      />
    </Routes>,
    { route: '/admin/users' },
  )
}

const profile = (isAdmin: boolean) => ({
  id: 2,
  email: 'someone@example.com',
  fullName: 'Someone',
  phone: null,
  isAdmin,
  addressLine: null,
  city: null,
  county: null,
  postalCode: null,
  country: null,
})

afterEach(() => vi.unstubAllGlobals())

describe('RequireAuth', () => {
  it('sends anonymous visitors to the login page', async () => {
    renderAdminRoute()

    expect(await screen.findByText('Login page')).toBeInTheDocument()
  })

  it('does not show admin content to a regular user', async () => {
    tokenStorage.set('user-token')
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(jsonResponse(200, profile(false))))

    renderAdminRoute()

    expect(await screen.findByText('Admins only')).toBeInTheDocument()
    expect(screen.queryByText('Secret admin content')).not.toBeInTheDocument()
  })

  it('shows admin content once the server confirms the admin flag', async () => {
    tokenStorage.set('admin-token')
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(jsonResponse(200, profile(true))))

    renderAdminRoute()

    expect(await screen.findByText('Secret admin content')).toBeInTheDocument()
  })
})
