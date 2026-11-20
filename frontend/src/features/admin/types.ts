export type AdminUser = {
  id: number
  email: string
  fullName: string
  phone: string | null
  city: string | null
  isAdmin: boolean
  isAccountActive: boolean
}

export type PagedResult<T> = {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export type UpdateUserAccessRequest = {
  isAdmin?: boolean
  isAccountActive?: boolean
}
