import { apiFetch } from './client'
import type { RegisterRequest, TokenResponse } from '../types'

export function getToken(userId: number, password: string) {
  return apiFetch<TokenResponse>('/api/auth/token', {
    method: 'POST',
    body: JSON.stringify({ userId, password })
  })
}

export function registerUser(request: RegisterRequest) {
  return apiFetch<TokenResponse>('/api/auth/register', {
    method: 'POST',
    body: JSON.stringify(request)
  })
}
