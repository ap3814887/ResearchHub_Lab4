import { ref, watch } from 'vue'
import { defineStore } from 'pinia'

export const useThemeStore = defineStore('theme', () => {
  const dark = ref(localStorage.getItem('researchhub.theme') === 'dark')

  function apply() {
    document.documentElement.dataset.theme = dark.value ? 'dark' : 'light'
  }

  function toggle() {
    dark.value = !dark.value
  }

  watch(dark, () => {
    localStorage.setItem('researchhub.theme', dark.value ? 'dark' : 'light')
    apply()
  }, { immediate: true })

  return { dark, toggle }
})
