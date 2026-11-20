import { createContext } from 'react'
import type { AuthState, LoginRequest, RegisterRequest } from './types'

export type AuthContextValue = {
  state: AuthState
  login: (request: LoginRequest) => Promise<void>
  register: (request: RegisterRequest) => Promise<void>
  logout: () => Promise<void>
}

export const AuthContext = createContext<AuthContextValue | null>(null)
