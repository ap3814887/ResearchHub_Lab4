import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import { getToken, registerUser } from '../api/authApi'
import { ApiError } from '../api/client'

const TOKEN_KEY = 'researchhub.jwt'
const USER_KEY = 'researchhub.auth.user'

interface StoredUser {
  userId: number
  name: string
  expiresAtUtc: string
}

function readStoredUser(): StoredUser | null {
  const raw = localStorage.getItem(USER_KEY)
  if (!raw) return null
  try {
    return JSON.parse(raw) as StoredUser
  } catch {
    localStorage.removeItem(USER_KEY)
    return null
  }
}

export const useAuthStore = defineStore('auth', () => {
  const token = ref(localStorage.getItem(TOKEN_KEY))
  const user = ref<StoredUser | null>(readStoredUser())
  const loading = ref(false)
  const error = ref('')

  const isAuthenticated = computed(() => {
    if (!token.value || !user.value) return false
    const expires = Date.parse(user.value.expiresAtUtc)
    return Number.isFinite(expires) && expires > Date.now()
  })

  function persist(accessToken: string, value: StoredUser) {
    token.value = accessToken
    user.value = value
    localStorage.setItem(TOKEN_KEY, accessToken)
    localStorage.setItem(USER_KEY, JSON.stringify(value))
  }

  function clearSession() {
    token.value = null
    user.value = null
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(USER_KEY)
  }

  function ensureValidSession() {
    if (isAuthenticated.value) return true
    clearSession()
    return false
  }

  async function login(userId: number, password: string) {
    loading.value = true
    error.value = ''
    try {
      const result = await getToken(userId, password)
      persist(result.accessToken, {
        userId: result.userId,
        name: result.name,
        expiresAtUtc: result.expiresAtUtc
      })
    } catch (e) {
      error.value = e instanceof ApiError ? e.message : 'Не удалось выполнить вход.'
      throw e
    } finally {
      loading.value = false
    }
  }

  async function register(name: string, email: string, password: string) {
    loading.value = true
    error.value = ''
    try {
      const result = await registerUser({ name, email, password })
      persist(result.accessToken, {
        userId: result.userId,
        name: result.name,
        expiresAtUtc: result.expiresAtUtc
      })
    } catch (e) {
      if (e instanceof ApiError && e.status === 404) {
        error.value = 'Backend не поддерживает регистрацию. Примените backend_patch из архива ЛР3.'
      } else {
        error.value = e instanceof ApiError ? e.message : 'Не удалось выполнить регистрацию.'
      }
      throw e
    } finally {
      loading.value = false
    }
  }

  function logout() {
    error.value = ''
    clearSession()
  }

  return {
    token,
    user,
    loading,
    error,
    isAuthenticated,
    login,
    register,
    logout,
    ensureValidSession
  }
})
