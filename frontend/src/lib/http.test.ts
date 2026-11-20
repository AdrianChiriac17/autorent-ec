import { afterEach, describe, expect, it, vi } from 'vitest'
import { ApiError, apiRequest, setUnauthorizedHandler } from './http'
import { tokenStorage } from './tokenStorage'

function mockFetch(status: number, body?: unknown) {
  const fetchMock = vi.fn().mockResolvedValue(
    new Response(body === undefined ? null : JSON.stringify(body), {
      status,
      headers: { 'Content-Type': 'application/json' },
    }),
  )
  vi.stubGlobal('fetch', fetchMock)
  return fetchMock
}

afterEach(() => {
  vi.unstubAllGlobals()
  setUnauthorizedHandler(null)
})

describe('apiRequest', () => {
  it('sends JSON and the stored bearer token', async () => {
    tokenStorage.set('abc123')
    const fetchMock = mockFetch(200, { ok: true })

    await apiRequest('/auth/login', { method: 'POST', body: { email: 'a@b.c' } })

    const [url, init] = fetchMock.mock.calls[0] as [string, RequestInit]
    expect(url.endsWith('/api/v1/auth/login')).toBe(true)
    expect(init.body).toBe('{"email":"a@b.c"}')
    expect(init.headers).toMatchObject({ Authorization: 'Bearer abc123', 'Content-Type': 'application/json' })
  })

  it('turns a ProblemDetails response into an ApiError with code and field errors', async () => {
    mockFetch(400, {
      title: 'One or more fields are invalid.',
      code: 'validation.failed',
      errors: { email: ['The Email field is not a valid e-mail address.'] },
    })

    const error = await apiRequest('/auth/register').catch((e: unknown) => e)

    expect(error).toBeInstanceOf(ApiError)
    expect(error).toMatchObject({ status: 400, code: 'validation.failed', message: 'One or more fields are invalid.' })
    expect((error as ApiError).fieldErrors.email).toHaveLength(1)
  })

  it('reports an unreachable server as a network error', async () => {
    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new TypeError('Failed to fetch')))

    await expect(apiRequest('/auth/me')).rejects.toMatchObject({ status: 0, code: 'network.unreachable' })
  })

  it('calls the unauthorized handler when a stored token is rejected', async () => {
    tokenStorage.set('expired')
    const onUnauthorized = vi.fn()
    setUnauthorizedHandler(onUnauthorized)
    mockFetch(401, { title: 'Authentication is required.', code: 'auth.unauthenticated' })

    await expect(apiRequest('/auth/me')).rejects.toBeInstanceOf(ApiError)
    expect(onUnauthorized).toHaveBeenCalledOnce()
  })

  it('returns undefined for 204 No Content', async () => {
    mockFetch(204)

    await expect(apiRequest('/auth/logout', { method: 'POST' })).resolves.toBeUndefined()
  })
})
