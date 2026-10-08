import type { ApiProblem } from '../types'

const configuredApiBase = import.meta.env.VITE_API_BASE_URL as string | undefined
const API_BASE_URL = configuredApiBase !== undefined
  ? configuredApiBase.replace(/\/$/, '')
  : (import.meta.env.DEV ? 'http://localhost:5082' : '')

export class ApiError extends Error {
  status: number
  problem: ApiProblem | null

  constructor(status: number, message: string, problem: ApiProblem | null = null) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.problem = problem
  }
}

function token(): string | null {
  return localStorage.getItem('researchhub.jwt')
}

export async function apiFetch<T>(path: string, options: RequestInit = {}): Promise<T> {
  const headers = new Headers(options.headers)
  if (!headers.has('Accept')) headers.set('Accept', 'application/json')
  if (options.body && !headers.has('Content-Type')) headers.set('Content-Type', 'application/json')

  const jwt = token()
  if (jwt) headers.set('Authorization', `Bearer ${jwt}`)

  let response: Response
  try {
    response = await fetch(`${API_BASE_URL}${path}`, { ...options, headers })
  } catch {
    throw new ApiError(0, 'Не удалось подключиться к backend. Проверьте, что он работает.')
  }

  if (response.ok) {
    if (response.status === 204) return undefined as T
    const text = await response.text()
    return (text ? JSON.parse(text) : undefined) as T
  }

  let problem: ApiProblem | null = null
  try {
    problem = (await response.json()) as ApiProblem
  } catch {
    problem = null
  }

  const validation = problem?.errors
    ? Object.values(problem.errors).flat().join(' ')
    : ''
  const message =
    problem?.detail ||
    problem?.message ||
    validation ||
    problem?.title ||
    (response.status === 401 ? 'Требуется авторизация.' : `HTTP ${response.status}`)

  throw new ApiError(response.status, message, problem)
}

export function apiBaseUrl() {
  return API_BASE_URL
}
