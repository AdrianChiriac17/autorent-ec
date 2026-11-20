import { apiRequest } from '../../../lib/http'
import type { AdminUser, PagedResult, UpdateUserAccessRequest } from '../types'

export const adminUsersApi = {
  list: (params: { page: number; pageSize: number; search: string }, signal?: AbortSignal) => {
    const query = new URLSearchParams({ page: String(params.page), pageSize: String(params.pageSize) })
    if (params.search) {
      query.set('search', params.search)
    }
    return apiRequest<PagedResult<AdminUser>>(`/admin/users?${query}`, { signal })
  },
  updateAccess: (id: number, request: UpdateUserAccessRequest) =>
    apiRequest<AdminUser>(`/admin/users/${id}`, { method: 'PATCH', body: request }),
}
