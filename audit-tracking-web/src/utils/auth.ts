import type { CurrentUser } from '@/types/auth'

const ACCESS_TOKEN_KEY = 'audit_tracking_access_token'
const CURRENT_USER_KEY = 'audit_tracking_current_user'
const TOKEN_EXPIRES_AT_KEY = 'audit_tracking_token_expires_at'

export const AUTH_UNAUTHORIZED_EVENT = 'audit-tracking:unauthorized'

function storageAvailable() {
  return typeof window !== 'undefined' && Boolean(window.localStorage)
}

export function saveAccessToken(accessToken: string) {
  if (storageAvailable()) localStorage.setItem(ACCESS_TOKEN_KEY, accessToken)
}

export function getAccessToken() {
  return storageAvailable() ? localStorage.getItem(ACCESS_TOKEN_KEY) : null
}

export function removeAccessToken() {
  if (storageAvailable()) localStorage.removeItem(ACCESS_TOKEN_KEY)
}

export function saveCurrentUser(currentUser: CurrentUser) {
  if (storageAvailable()) {
    localStorage.setItem(CURRENT_USER_KEY, JSON.stringify(currentUser))
  }
}

export function getStoredCurrentUser(): CurrentUser | null {
  if (!storageAvailable()) return null
  const value = localStorage.getItem(CURRENT_USER_KEY)
  if (!value) return null

  try {
    const parsed: unknown = JSON.parse(value)
    if (
      parsed &&
      typeof parsed === 'object' &&
      'userName' in parsed &&
      'displayName' in parsed &&
      'role' in parsed &&
      typeof parsed.userName === 'string' &&
      typeof parsed.displayName === 'string' &&
      typeof parsed.role === 'string'
    ) {
      return parsed as CurrentUser
    }
  } catch {
    // Invalid local data is removed below.
  }

  localStorage.removeItem(CURRENT_USER_KEY)
  return null
}

export function removeCurrentUser() {
  if (storageAvailable()) localStorage.removeItem(CURRENT_USER_KEY)
}

export function saveTokenExpiresAt(expiresAt: string) {
  if (storageAvailable()) localStorage.setItem(TOKEN_EXPIRES_AT_KEY, expiresAt)
}

export function getTokenExpiresAt() {
  return storageAvailable() ? localStorage.getItem(TOKEN_EXPIRES_AT_KEY) : null
}

export function removeTokenExpiresAt() {
  if (storageAvailable()) localStorage.removeItem(TOKEN_EXPIRES_AT_KEY)
}

export function isTokenExpired(expiresAt = getTokenExpiresAt()) {
  if (!expiresAt) return true
  const expiresAtTime = new Date(expiresAt).getTime()
  return Number.isNaN(expiresAtTime) || expiresAtTime <= Date.now()
}

export function hasValidToken() {
  return Boolean(getAccessToken()) && !isTokenExpired()
}

export function clearAuthStorage() {
  removeAccessToken()
  removeCurrentUser()
  removeTokenExpiresAt()
}
