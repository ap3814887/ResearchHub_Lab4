import { apiFetch } from './client'
import type {
  AddProjectMemberRequest,
  CreateProjectRequest,
  PagedResponse,
  Project,
  ProjectMember,
  TaskItem,
  TaskPriority,
  TaskStatus,
  TaskStatusCount,
  UpdateProjectRequest
} from '../types'

export function getProjects(page = 1, pageSize = 20) {
  return apiFetch<PagedResponse<Project>>(`/api/projects?page=${page}&pageSize=${pageSize}`)
}

export function getProject(id: number) {
  return apiFetch<Project>(`/api/projects/${id}`)
}

export function createProject(request: CreateProjectRequest) {
  return apiFetch<Project>('/api/projects', { method: 'POST', body: JSON.stringify(request) })
}

export function updateProject(id: number, request: UpdateProjectRequest) {
  return apiFetch<Project>(`/api/projects/${id}`, { method: 'PUT', body: JSON.stringify(request) })
}

export function deleteProject(id: number) {
  return apiFetch<void>(`/api/projects/${id}`, { method: 'DELETE' })
}

export function getMembers(projectId: number) {
  return apiFetch<ProjectMember[]>(`/api/projects/${projectId}/members`)
}

export function addMember(projectId: number, request: AddProjectMemberRequest) {
  return apiFetch<ProjectMember>(`/api/projects/${projectId}/members`, {
    method: 'POST',
    body: JSON.stringify(request)
  })
}

export function removeMember(projectId: number, userId: number) {
  return apiFetch<void>(`/api/projects/${projectId}/members/${userId}`, { method: 'DELETE' })
}

export interface TaskQuery {
  status?: TaskStatus | ''
  priority?: TaskPriority | ''
  assigneeId?: number | null
  page?: number
  pageSize?: number
}

export function getProjectTasks(projectId: number, query: TaskQuery = {}) {
  const params = new URLSearchParams()
  if (query.status) params.set('status', query.status)
  if (query.priority) params.set('priority', query.priority)
  if (query.assigneeId) params.set('assigneeId', String(query.assigneeId))
  params.set('page', String(query.page ?? 1))
  params.set('pageSize', String(query.pageSize ?? 20))
  return apiFetch<PagedResponse<TaskItem>>(`/api/projects/${projectId}/tasks?${params.toString()}`)
}

export function getTaskStats(projectId: number) {
  return apiFetch<TaskStatusCount[]>(`/api/projects/${projectId}/task-stats`)
}
