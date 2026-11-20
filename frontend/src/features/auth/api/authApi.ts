import { apiRequest } from '../../../lib/http'
import type { AuthResponse, LoginRequest, RegisterRequest, UserProfile } from '../types'

export const authApi = {
  login: (request: LoginRequest) => apiRequest<AuthResponse>('/auth/login', { method: 'POST', body: request }),
  register: (request: RegisterRequest) => apiRequest<AuthResponse>('/auth/register', { method: 'POST', body: request }),
  logout: () => apiRequest<void>('/auth/logout', { method: 'POST' }),
  me: (signal?: AbortSignal) => apiRequest<UserProfile>('/auth/me', { signal }),
}
