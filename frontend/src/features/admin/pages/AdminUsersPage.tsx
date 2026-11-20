import { useEffect, useState } from 'react'
import { useSearchParams } from 'react-router'
import { Alert } from '../../../components/Alert'
import { Button } from '../../../components/Button'
import { Spinner } from '../../../components/Spinner'
import { Switch } from '../../../components/Switch'
import { useAuth } from '../../auth/hooks/useAuth'
import { useAdminUsers } from '../hooks/useAdminUsers'
import type { AdminUser } from '../types'

export function AdminUsersPage() {
  const { state: auth } = useAuth()
  const currentUserId = auth.status === 'authenticated' ? auth.user.id : null

  // Search and page live in the URL, so the view survives refreshes and can be shared.
  const [params, setParams] = useSearchParams()
  const page = Math.max(1, Number(params.get('page')) || 1)
  const search = params.get('q') ?? ''
  const [searchInput, setSearchInput] = useState(search)

  useEffect(() => {
    const handle = setTimeout(() => {
      if (searchInput.trim() !== search) {
        setParams(searchInput.trim() ? { q: searchInput.trim() } : {}, { replace: true })
      }
    }, 300)
    return () => clearTimeout(handle)
  }, [searchInput, search, setParams])

  const { state, savingId, rowError, updateAccess } = useAdminUsers(page, search)

  const goToPage = (next: number) => {
    const nextParams = new URLSearchParams(params)
    nextParams.set('page', String(next))
    setParams(nextParams)
  }

  return (
    <div className="mx-auto w-full max-w-5xl px-4 py-8">
      <header className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <p className="text-xs font-semibold uppercase tracking-widest text-accent-strong">Administration</p>
          <h1 className="font-display text-3xl font-bold tracking-tight">Users</h1>
          {state.status === 'ready' && (
            <p className="mt-1 text-sm text-muted">
              {state.data.totalCount} {state.data.totalCount === 1 ? 'account' : 'accounts'}
            </p>
          )}
        </div>
        <label className="w-full sm:w-72">
          <span className="sr-only">Search users by name or email</span>
          <input
            type="search"
            placeholder="Search name or email"
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            className="block min-h-11 w-full rounded-lg border border-line bg-white px-3 outline-none focus:border-ink"
          />
        </label>
      </header>

      <div className="mt-6">
        {state.status === 'loading' && <Spinner label="Loading users" />}
        {state.status === 'error' && <Alert>{state.error.message}</Alert>}
        {state.status === 'ready' && state.data.items.length === 0 && (
          <Alert tone="info">{search ? `No users match “${search}”.` : 'There are no users yet.'}</Alert>
        )}
        {state.status === 'ready' && state.data.items.length > 0 && (
          <>
            <ul className="divide-y divide-line overflow-hidden rounded-2xl border border-line bg-white">
              <li className="hidden grid-cols-[1fr_7rem_7rem] gap-4 bg-paper px-5 py-3 text-xs font-semibold uppercase tracking-wider text-muted md:grid">
                <span>User</span>
                <span className="text-center">Admin</span>
                <span className="text-center">Active</span>
              </li>
              {state.data.items.map((user) => (
                <UserRow
                  key={user.id}
                  user={user}
                  isSelf={user.id === currentUserId}
                  saving={savingId === user.id}
                  error={rowError?.id === user.id ? rowError.message : undefined}
                  onChange={(request) => updateAccess(user.id, request)}
                />
              ))}
            </ul>

            {state.data.totalPages > 1 && (
              <nav aria-label="Pagination" className="mt-4 flex items-center justify-between gap-2">
                <Button variant="ghost" disabled={page <= 1} onClick={() => goToPage(page - 1)}>
                  ← Previous
                </Button>
                <span className="text-sm text-muted">
                  Page {state.data.page} of {state.data.totalPages}
                </span>
                <Button variant="ghost" disabled={page >= state.data.totalPages} onClick={() => goToPage(page + 1)}>
                  Next →
                </Button>
              </nav>
            )}
          </>
        )}
      </div>
    </div>
  )
}

type RowProps = {
  user: AdminUser
  isSelf: boolean
  saving: boolean
  error?: string
  onChange: (request: { isAdmin?: boolean; isAccountActive?: boolean }) => void
}

function UserRow({ user, isSelf, saving, error, onChange }: RowProps) {
  return (
    <li className="grid gap-3 px-5 py-4 md:grid-cols-[1fr_7rem_7rem] md:items-center md:gap-4">
      <div className="min-w-0">
        <p className="truncate font-semibold">
          {user.fullName}
          {isSelf && <span className="ml-2 rounded-full bg-accent/20 px-2 py-0.5 text-xs font-semibold text-ink">You</span>}
        </p>
        <p className="truncate text-sm text-muted">{user.email}</p>
        {(user.phone || user.city) && (
          <p className="truncate text-xs text-muted">{[user.phone, user.city].filter(Boolean).join(' · ')}</p>
        )}
        {error && <p className="mt-1 text-sm text-danger">{error}</p>}
      </div>
      <div className="flex items-center justify-between md:justify-center">
        <span className="text-sm text-muted md:hidden">Admin</span>
        <Switch
          label={`Admin rights for ${user.fullName}`}
          checked={user.isAdmin}
          disabled={saving || isSelf}
          onChange={(isAdmin) => onChange({ isAdmin })}
        />
      </div>
      <div className="flex items-center justify-between md:justify-center">
        <span className="text-sm text-muted md:hidden">Active</span>
        <Switch
          label={`Account active for ${user.fullName}`}
          checked={user.isAccountActive}
          disabled={saving || isSelf}
          onChange={(isAccountActive) => onChange({ isAccountActive })}
        />
      </div>
    </li>
  )
}
