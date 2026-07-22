import axios from 'axios'
import type { AxiosInstance } from 'axios'
import { ElMessage } from 'element-plus'
import type { ApiResponse } from '@/types/api'
import {
  AUTH_UNAUTHORIZED_EVENT,
  clearAuthStorage,
  getAccessToken,
  getStoredCurrentUser,
  isTokenExpired,
} from '@/utils/auth'

let unauthorizedEventPending = false
let lastForbiddenNotificationAt = 0

function notifyUnauthorized() {
  if (
    typeof window === 'undefined' ||
    window.location.pathname === '/login' ||
    unauthorizedEventPending
  ) {
    return
  }

  unauthorizedEventPending = true
  window.dispatchEvent(new Event(AUTH_UNAUTHORIZED_EVENT))
  window.setTimeout(() => {
    unauthorizedEventPending = false
  }, 500)
}

const request: AxiosInstance = axios.create({
  baseURL: '/api',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
})

request.interceptors.request.use((config) => {
  if (!config.headers) (config as any).headers = {}

  const accessToken = getAccessToken()
  if (accessToken && !isTokenExpired()) {
    ;(config as any).headers.Authorization = `Bearer ${accessToken}`

    const currentUser = getStoredCurrentUser()
    if (currentUser?.userName) {
      ;(config as any).headers['X-User-Name'] = currentUser.userName
    }
  } else if (accessToken) {
    clearAuthStorage()
    notifyUnauthorized()
  }

  return config
})

;(request.interceptors.response as any).use(
  (response: any) => {
    const respData = response.data

    // If backend uses ApiResponse wrapper
    if (respData && typeof respData === 'object' && typeof respData.success === 'boolean') {
      const data = respData as ApiResponse<unknown>

      if (data.success === false) {
        const err = new Error(data.message || '请求失败')
        return Promise.reject(err)
      }

      return data.data
    }

    // Fallback: return raw response data
    return respData
  },
  (error: any) => {
    const resp = error?.response?.data as ApiResponse<unknown> | undefined

    if (error?.response?.status === 401) {
      clearAuthStorage()
      notifyUnauthorized()
    } else if (error?.response?.status === 403) {
      const now = Date.now()
      if (now - lastForbiddenNotificationAt > 1000) {
        lastForbiddenNotificationAt = now
        ElMessage.error(resp?.message || '没有权限执行此操作')
      }
    }

    // Try to extract backend message
    if (resp && typeof resp.message === 'string') {
      return Promise.reject(new Error(resp.message))
    }

    return Promise.reject(error)
  },
)

export default request
