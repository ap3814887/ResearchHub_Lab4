export interface User {
  id: number
  name: string
  email: string
  createdAt: string
}

export interface Project {
  id: number
  name: string
  description: string | null
  ownerId: number
  createdAt: string
}

export interface ProjectMember {
  projectId: number
  userId: number
  userName: string | null
  email: string | null
  role: string
  joinedAt: string
}

export type TaskStatus = 'New' | 'InProgress' | 'Done' | 'Cancelled'
export type TaskPriority = 'Low' | 'Normal' | 'High' | 'Critical'

export interface TaskItem {
  id: number
  projectId: number
  title: string
  description: string | null
  assigneeId: number | null
  status: TaskStatus
  priority: TaskPriority
  dueDate: string | null
  createdAt: string
  updatedAt: string
}

export interface TaskStatusCount {
  status: string
  taskCount: number
}

export interface PagedResponse<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface CreateUserRequest {
  name: string
  email: string
}

export interface UpdateUserRequest {
  name: string
  email: string
}

export interface CreateProjectRequest {
  name: string
  description: string | null
  ownerId: number
}

export interface UpdateProjectRequest {
  name: string
  description: string | null
}

export interface AddProjectMemberRequest {
  userId: number
  role: string
}

export interface CreateTaskRequest {
  projectId: number
  title: string
  description: string | null
  assigneeId: number | null
  status: TaskStatus
  priority: TaskPriority
  dueDate: string | null
}

export interface UpdateTaskRequest {
  title: string
  description: string | null
  assigneeId: number | null
  priority: TaskPriority
  dueDate: string | null
}

export interface TaskStatusChangeResponse {
  id: number
  oldStatus: string
  newStatus: string
  updatedAt: string
}


export interface RegisterRequest {
  name: string
  email: string
  password: string
}

export interface TokenResponse {
  accessToken: string
  tokenType: string
  expiresAtUtc: string
  userId: number
  name: string
}

export interface ApiProblem {
  message?: string
  title?: string
  status?: number
  detail?: string
  instance?: string
  sqlState?: string
  traceId?: string
  errors?: Record<string, string[]>
}
