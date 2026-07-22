import request from '@/utils/request'

import type {
  ChangePasswordRequest,
  CurrentUser,
  LoginRequest,
  LoginResponse,
} from '@/types/auth'

// The backend AuthController is not available yet. These centralized calls are
// ready for the expected endpoints and intentionally do not provide mock login.
export function login(data: LoginRequest): Promise<LoginResponse> {
  return request.post('/auth/login', data)
}

export function getCurrentUser(): Promise<CurrentUser> {
  return request.get('/auth/me')
}

export function changePassword(data: ChangePasswordRequest): Promise<void> {
  return request.put('/auth/change-password', data)
}
