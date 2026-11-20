import { useCallback, useEffect, useState } from 'react'
import { ApiError } from '../../../lib/http'
import { adminUsersApi } from '../api/adminUsersApi'
import type { AdminUser, PagedResult, UpdateUserAccessRequest } from '../types'

export const ADMIN_USERS_PAGE_SIZE = 20

type ListState =
  | { status: 'loading' }
  | { status: 'error'; error: ApiError }
  | { status: 'ready'; data: PagedResult<AdminUser> }

type LoadedResult = { key: string; state: Exclude<ListState, { status: 'loading' }> }

export function useAdminUsers(page: number, search: string) {
  const requestKey = `${page}|${search}`
  // The result remembers which query produced it; while a newer query is in flight we show "loading".
  const [result, setResult] = useState<LoadedResult | null>(null)
  const [savingId, setSavingId] = useState<number | null>(null)
  const [rowError, setRowError] = useState<{ id: number; message: string } | null>(null)

  useEffect(() => {
    const controller = new AbortController()
    adminUsersApi
      .list({ page, pageSize: ADMIN_USERS_PAGE_SIZE, search }, controller.signal)
      .then((data) => setResult({ key: requestKey, state: { status: 'ready', data } }))
      .catch((error: unknown) => {
        if (error instanceof ApiError) {
          setResult({ key: requestKey, state: { status: 'error', error } })
        }
      })
    return () => controller.abort()
  }, [page, search, requestKey])

  const updateAccess = useCallback(async (id: number, request: UpdateUserAccessRequest) => {
    setSavingId(id)
    setRowError(null)
    try {
      const updated = await adminUsersApi.updateAccess(id, request)
      setResult((current) =>
        current?.state.status === 'ready'
          ? {
              ...current,
              state: {
                status: 'ready',
                data: { ...current.state.data, items: current.state.data.items.map((u) => (u.id === id ? updated : u)) },
              },
            }
          : current,
      )
    } catch (error) {
      setRowError({ id, message: error instanceof ApiError ? error.message : 'Could not update this user.' })
    } finally {
      setSavingId(null)
    }
  }, [])

  const state: ListState = result?.key === requestKey ? result.state : { status: 'loading' }
  return { state, savingId, rowError, updateAccess }
}
