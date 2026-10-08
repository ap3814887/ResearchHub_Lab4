import { apiFetch } from './client'
import type {
  CreateTaskRequest,
  TaskItem,
  TaskPriority,
  TaskStatus,
  TaskStatusChangeResponse,
  UpdateTaskRequest
} from '../types'

export function getTask(id: number) {
  return apiFetch<TaskItem>(`/api/tasks/${id}`)
}

export function createTask(request: CreateTaskRequest) {
  return apiFetch<TaskItem>('/api/tasks', { method: 'POST', body: JSON.stringify(request) })
}

export function updateTask(id: number, request: UpdateTaskRequest) {
  return apiFetch<TaskItem>(`/api/tasks/${id}`, { method: 'PUT', body: JSON.stringify(request) })
}

export function deleteTask(id: number) {
  return apiFetch<void>(`/api/tasks/${id}`, { method: 'DELETE' })
}

export function changeTaskStatus(id: number, status: TaskStatus) {
  return apiFetch<TaskStatusChangeResponse>(`/api/tasks/${id}/status`, {
    method: 'PATCH',
    body: JSON.stringify({ status })
  })
}

export function assignTask(id: number, assigneeId: number | null) {
  return apiFetch<void>(`/api/tasks/${id}/assignee`, {
    method: 'PATCH',
    body: JSON.stringify({ assigneeId })
  })
}

export function changeTaskPriority(id: number, priority: TaskPriority) {
  return apiFetch<void>(`/api/tasks/${id}/priority`, {
    method: 'PATCH',
    body: JSON.stringify({ priority })
  })
}
