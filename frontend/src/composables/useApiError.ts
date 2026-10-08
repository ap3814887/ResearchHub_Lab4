import { ref } from 'vue'
import { ApiError } from '../api/client'

export function useApiError() {
  const error = ref('')

  function capture(e: unknown, fallback = 'Операция не выполнена.') {
    error.value = e instanceof ApiError ? e.message : fallback
    return error.value
  }

  function clear() {
    error.value = ''
  }

  return { error, capture, clear }
}
