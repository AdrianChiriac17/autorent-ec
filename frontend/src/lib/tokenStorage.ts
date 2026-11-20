const TOKEN_KEY = 'autorent.accessToken'

/**
 * Keeps the access token in localStorage so a session survives refreshes and new tabs.
 * Tokens are short-lived (30 min) and every storage call is guarded, because storage
 * can be unavailable (private mode, blocked site data).
 */
export const tokenStorage = {
  get(): string | null {
    try {
      return localStorage.getItem(TOKEN_KEY)
    } catch {
      return null
    }
  },
  set(token: string): void {
    try {
      localStorage.setItem(TOKEN_KEY, token)
    } catch {
      // Storage unavailable: the session simply won't survive a refresh.
    }
  },
  clear(): void {
    try {
      localStorage.removeItem(TOKEN_KEY)
    } catch {
      // Nothing to clear.
    }
  },
}
