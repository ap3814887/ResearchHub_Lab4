import { ref } from 'vue'
import { defineStore } from 'pinia'

export type ToastKind = 'success' | 'error' | 'info'
export interface ToastMessage {
  id: number
  kind: ToastKind
  text: string
}

export const useToastStore = defineStore('toast', () => {
  const items = ref<ToastMessage[]>([])
  let nextId = 1

  function push(text: string, kind: ToastKind = 'info', timeout = 3500) {
    const id = nextId++
    items.value.push({ id, kind, text })
    window.setTimeout(() => remove(id), timeout)
  }

  function remove(id: number) {
    items.value = items.value.filter(x => x.id !== id)
  }

  return { items, push, remove }
})
