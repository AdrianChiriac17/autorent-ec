import { Link, NavLink, Outlet, useNavigate } from 'react-router'
import { useAuth } from '../features/auth/hooks/useAuth'

const navLinkClass = ({ isActive }: { isActive: boolean }) =>
  `rounded-lg px-3 py-2 text-sm font-medium transition-colors ${isActive ? 'bg-white/10 text-white' : 'text-white/70 hover:text-white'}`

export function AppLayout() {
  const { state, logout } = useAuth()
  const navigate = useNavigate()

  async function handleLogout() {
    await logout()
    navigate('/login', { replace: true })
  }

  return (
    <div className="flex min-h-dvh flex-col">
      <header className="bg-ink text-white">
        <div className="mx-auto flex w-full max-w-5xl flex-wrap items-center justify-between gap-x-4 gap-y-2 px-4 py-3">
          <Link to="/" className="flex items-center gap-2 font-display text-lg font-bold tracking-tight">
            <img src="/favicon.svg" alt="" className="size-7" />
            AutoRent<span className="text-accent">Craiova</span>
          </Link>
          <nav aria-label="Main" className="flex items-center gap-1">
            {state.status === 'authenticated' && state.user.isAdmin && (
              <NavLink to="/admin/users" className={navLinkClass}>
                Users
              </NavLink>
            )}
            {state.status === 'authenticated' ? (
              <>
                <span className="hidden px-2 text-sm text-white/70 sm:inline">{state.user.fullName}</span>
                <button type="button" onClick={handleLogout} className="rounded-lg px-3 py-2 text-sm font-medium text-white/70 hover:text-white">
                  Log out
                </button>
              </>
            ) : state.status === 'anonymous' ? (
              <>
                <NavLink to="/login" className={navLinkClass}>
                  Log in
                </NavLink>
                <Link to="/register" className="rounded-lg bg-accent px-3 py-2 text-sm font-semibold text-ink hover:bg-accent-strong">
                  Sign up
                </Link>
              </>
            ) : null}
          </nav>
        </div>
      </header>
      <main className="flex-1">
        <Outlet />
      </main>
      <footer className="border-t border-line px-4 py-6 text-center text-xs text-muted">
        AutoRent Craiova · E-Commerce course project
      </footer>
    </div>
  )
}
