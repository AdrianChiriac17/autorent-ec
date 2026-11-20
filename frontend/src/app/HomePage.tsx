import { Link } from 'react-router'
import { useAuth } from '../features/auth/hooks/useAuth'

export function HomePage() {
  const { state } = useAuth()

  return (
    <section className="mx-auto w-full max-w-5xl px-4 py-14 sm:py-20">
      <p className="text-xs font-semibold uppercase tracking-widest text-accent-strong">Car rental · Craiova</p>
      <h1 className="mt-3 max-w-2xl font-display text-4xl font-bold leading-tight tracking-tight sm:text-5xl">
        Pick the exact car, the dates and the extras.
      </h1>
      <p className="mt-4 max-w-xl text-lg text-muted">
        The catalog is on its way. For now you can create an account and log in.
      </p>
      <div className="mt-8 flex flex-wrap gap-3">
        {state.status === 'authenticated' ? (
          <p className="rounded-lg border border-line bg-white px-4 py-3 text-sm">
            Logged in as <strong>{state.user.fullName}</strong>
            {state.user.isAdmin && (
              <>
                {' · '}
                <Link to="/admin/users" className="font-semibold underline underline-offset-4">
                  Manage users
                </Link>
              </>
            )}
          </p>
        ) : (
          <>
            <Link to="/register" className="inline-flex min-h-11 items-center rounded-lg bg-accent px-5 font-semibold text-ink hover:bg-accent-strong">
              Create an account
            </Link>
            <Link to="/login" className="inline-flex min-h-11 items-center rounded-lg border border-ink px-5 font-semibold hover:bg-ink/5">
              Log in
            </Link>
          </>
        )}
      </div>
    </section>
  )
}
