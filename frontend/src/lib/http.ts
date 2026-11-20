import { tokenStorage } from './tokenStorage'

const API_BASE_URL: string = import.meta.env.VITE_API_BASE_URL ?? '/api/v1'

/** A failed API call, normalised from the backend's ProblemDetails response. */
export class ApiError extends Error {
  readonly status: number
  /** Stable machine-readable code from the API, e.g. "auth.invalid_credentials". */
  readonly code: string
  /** Per-field validation messages keyed by camelCase field name. */
  readonly fieldErrors: Record<string, string[]>

  constructor(status: number, code: string, message: string, fieldErrors: Record<string, string[]> = {}) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.code = code
    this.fieldErrors = fieldErrors
  }
}

type ProblemDetails = {
  title?: string
  code?: string
  errors?: Record<string, string[]>
}

type RequestOptions = {
  method?: 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE'
  body?: unknown
  signal?: AbortSignal
}

let onUnauthorized: (() => void) | null = null

/** Registers a callback for 401 responses (the token is missing, expired or invalid). */
export function setUnauthorizedHandler(handler: (() => void) | null): void {
  onUnauthorized = handler
}

/** The single entry point for API calls: base URL, bearer token, JSON and error normalisation. */
export async function apiRequest<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const headers: Record<string, string> = { Accept: 'application/json' }
  if (options.body !== undefined) {
    headers['Content-Type'] = 'application/json'
  }
  const token = tokenStorage.get()
  if (token) {
    headers.Authorization = `Bearer ${token}`
  }

  let response: Response
  try {
    response = await fetch(`${API_BASE_URL}${path}`, {
      method: options.method ?? 'GET',
      headers,
      body: options.body === undefined ? undefined : JSON.stringify(options.body),
      signal: options.signal,
    })
  } catch (error) {
    if (error instanceof DOMException && error.name === 'AbortError') {
      throw error
    }
    throw new ApiError(0, 'network.unreachable', 'Cannot reach the server. Check your connection and try again.')
  }

  if (response.status === 204) {
    return undefined as T
  }

  const payload: unknown = await response.json().catch(() => null)

  if (!response.ok) {
    const problem = (payload ?? {}) as ProblemDetails
    if (response.status === 401 && token) {
      onUnauthorized?.()
    }
    throw new ApiError(
      response.status,
      problem.code ?? `http.${response.status}`,
      problem.title ?? 'Something went wrong. Please try again.',
      problem.errors ?? {},
    )
  }

  return payload as T
}
