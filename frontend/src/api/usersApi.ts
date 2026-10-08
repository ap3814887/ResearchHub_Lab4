import { apiFetch } from './client'
import type { CreateUserRequest, PagedResponse, UpdateUserRequest, User } from '../types'

export function getUsers(page = 1, pageSize = 20) {
  return apiFetch<PagedResponse<User>>(`/api/users?page=${page}&pageSize=${pageSize}`)
}

export function getUser(id: number) {
  return apiFetch<User>(`/api/users/${id}`)
}

export function createUser(request: CreateUserRequest) {
  return apiFetch<User>('/api/users', { method: 'POST', body: JSON.stringify(request) })
}

export function updateUser(id: number, request: UpdateUserRequest) {
  return apiFetch<User>(`/api/users/${id}`, { method: 'PUT', body: JSON.stringify(request) })
}

export function deleteUser(id: number) {
  return apiFetch<void>(`/api/users/${id}`, { method: 'DELETE' })
}
