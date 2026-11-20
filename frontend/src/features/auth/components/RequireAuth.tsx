import type { ReactNode } from 'react'
import { Navigate, useLocation } from 'react-router'
import { Spinner } from '../../../components/Spinner'
import { useAuth } from '../hooks/useAuth'

type Props = {
  children: ReactNode
  /** Also require the admin flag. The API enforces this too; this only avoids showing a dead page. */
  admin?: boolean
}

export function RequireAuth({ children, admin = false }: Props) {
  const { state } = useAuth()
  const location = useLocation()

  if (state.status === 'loading') {
    return <Spinner label="Checking your session" />
  }

  if (state.status === 'anonymous') {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />
  }

  if (admin && !state.user.isAdmin) {
    return (
      <div className="mx-auto max-w-md px-4 py-16 text-center">
        <h1 className="font-display text-2xl font-bold">Admins only</h1>
        <p className="mt-2 text-muted">Your account doesn't have access to this page.</p>
      </div>
    )
  }

  return children
}
