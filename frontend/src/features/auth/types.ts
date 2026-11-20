export type UserProfile = {
  id: number
  email: string
  fullName: string
  phone: string | null
  isAdmin: boolean
  addressLine: string | null
  city: string | null
  county: string | null
  postalCode: string | null
  country: string | null
}

export type AuthResponse = {
  accessToken: string
  expiresAt: string
  user: UserProfile
}

export type LoginRequest = {
  email: string
  password: string
}

export type RegisterRequest = {
  email: string
  password: string
  fullName: string
  phone?: string
}

export type AuthState =
  | { status: 'loading' }
  | { status: 'anonymous' }
  | { status: 'authenticated'; user: UserProfile }
