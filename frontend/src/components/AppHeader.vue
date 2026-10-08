<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { useThemeStore } from '../stores/theme'

const auth = useAuthStore()
const theme = useThemeStore()
const router = useRouter()

async function logout() {
  auth.logout()
  await router.replace('/login')
}
</script>

<template>
  <header class="app-header">
    <div class="container header-inner">
      <RouterLink class="brand" to="/projects">
        <span class="brand-mark">R</span>
        <span><strong>ResearchHub</strong></span>
      </RouterLink>
      <nav>
        <RouterLink to="/projects">Проекты</RouterLink>
        <RouterLink to="/users">Пользователи</RouterLink>
      </nav>
      <div class="header-actions">
        <button class="icon-btn" :title="theme.dark ? 'Светлая тема' : 'Тёмная тема'" @click="theme.toggle()">{{ theme.dark ? '☀' : '☾' }}</button>
        <span class="user-chip">{{ auth.user?.name || `ID ${auth.user?.userId}` }}</span>
        <button class="btn btn-secondary" @click="logout">Выйти</button>
      </div>
    </div>
  </header>
</template>
