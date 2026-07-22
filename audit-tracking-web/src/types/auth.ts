export interface LoginRequest {
  userName: string
  password: string
}

export interface LoginResponse {
  accessToken: string
  tokenType: string
  userName: string
  displayName: string
  role: string
  expiresAt: string
}

export interface CurrentUser {
  id?: number
  userName: string
  displayName: string
  role: string
  lastLoginAt?: string | null
}

export interface ChangePasswordRequest {
  currentPassword: string
  newPassword: string
}
