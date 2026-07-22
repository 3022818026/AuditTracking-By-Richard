import { defineStore } from 'pinia'

import { getCurrentUser, login as loginRequest } from '@/api/auth'
import type { CurrentUser, LoginRequest } from '@/types/auth'
import {
  clearAuthStorage,
  getAccessToken,
  getStoredCurrentUser,
  getTokenExpiresAt,
  isTokenExpired,
  saveAccessToken,
  saveCurrentUser,
  saveTokenExpiresAt,
} from '@/utils/auth'

interface AuthState {
  accessToken: string | null
  currentUser: CurrentUser | null
  expiresAt: string | null
  isAuthenticated: boolean
  loginLoading: boolean
  initialized: boolean
}

export const useAuthStore = defineStore('auth', {
  state: (): AuthState => ({
    accessToken: null,
    currentUser: null,
    expiresAt: null,
    isAuthenticated: false,
    loginLoading: false,
    initialized: false,
  }),

  actions: {
    initializeAuth() {
      if (this.initialized) {
        if (this.isAuthenticated && isTokenExpired(this.expiresAt)) this.clearAuth()
        return
      }

      this.initialized = true
      const accessToken = getAccessToken()
      const currentUser = getStoredCurrentUser()
      const expiresAt = getTokenExpiresAt()

      if (!accessToken || !currentUser || isTokenExpired(expiresAt)) {
        this.clearAuth()
        return
      }

      this.accessToken = accessToken
      this.currentUser = currentUser
      this.expiresAt = expiresAt
      this.isAuthenticated = true
    },

    async login(payload: LoginRequest) {
      if (this.loginLoading) return
      this.loginLoading = true
      this.clearAuth()

      try {
        const response = await loginRequest(payload)
        const currentUser: CurrentUser = {
          userName: response.userName,
          displayName: response.displayName,
          role: response.role,
          lastLoginAt: null,
        }

        saveAccessToken(response.accessToken)
        saveCurrentUser(currentUser)
        saveTokenExpiresAt(response.expiresAt)

        this.accessToken = response.accessToken
        this.currentUser = currentUser
        this.expiresAt = response.expiresAt
        this.isAuthenticated = true
      } catch (error) {
        this.clearAuth()
        throw error
      } finally {
        this.loginLoading = false
      }
    },

    async loadCurrentUser() {
      try {
        const currentUser = await getCurrentUser()
        saveCurrentUser(currentUser)
        this.currentUser = currentUser
        return currentUser
      } catch (error) {
        this.clearAuth()
        throw error
      }
    },

    logout() {
      this.clearAuth()
    },

    clearAuth() {
      clearAuthStorage()
      this.accessToken = null
      this.currentUser = null
      this.expiresAt = null
      this.isAuthenticated = false
    },
  },
})
