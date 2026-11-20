import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import { setUnauthorizedHandler } from '../../lib/http'
import { tokenStorage } from '../../lib/tokenStorage'
import { authApi } from './api/authApi'
import { AuthContext, type AuthContextValue } from './AuthContext'
import type { AuthResponse, AuthState, LoginRequest, RegisterRequest } from './types'

/**
 * Holds who is logged in. On start-up a stored token is checked with GET /auth/me, so the
 * user and their admin flag always come from the server, never from the token itself.
 */
export function AuthProvider({ children }: { children: ReactNode }) {
  const [state, setState] = useState<AuthState>(() =>
    tokenStorage.get() ? { status: 'loading' } : { status: 'anonymous' },
  )

  const signOutLocally = useCallback(() => {
    tokenStorage.clear()
    setState({ status: 'anonymous' })
  }, [])

  useEffect(() => {
    setUnauthorizedHandler(signOutLocally)
    return () => setUnauthorizedHandler(null)
  }, [signOutLocally])

  useEffect(() => {
    if (!tokenStorage.get()) {
      return
    }
    const controller = new AbortController()
    authApi
      .me(controller.signal)
      .then((user) => setState({ status: 'authenticated', user }))
      .catch((error: unknown) => {
        if (!(error instanceof DOMException && error.name === 'AbortError')) {
          signOutLocally()
        }
      })
    return () => controller.abort()
  }, [signOutLocally])

  const startSession = useCallback((response: AuthResponse) => {
    tokenStorage.set(response.accessToken)
    setState({ status: 'authenticated', user: response.user })
  }, [])

  const value = useMemo<AuthContextValue>(
    () => ({
      state,
      login: async (request: LoginRequest) => startSession(await authApi.login(request)),
      register: async (request: RegisterRequest) => startSession(await authApi.register(request)),
      logout: async () => {
        // JWTs are stateless: the server call is a courtesy, discarding the token is what ends the session.
        await authApi.logout().catch(() => undefined)
        signOutLocally()
      },
    }),
    [state, startSession, signOutLocally],
  )

  return <AuthContext value={value}>{children}</AuthContext>
}
