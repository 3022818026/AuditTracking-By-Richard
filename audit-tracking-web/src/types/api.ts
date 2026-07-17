export interface ApiResponse<T> {
  success: boolean
  message: string
  data: T
  errors?: unknown
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  total: number
  totalPages: number
}
