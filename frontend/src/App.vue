<script setup lang="ts">
import AppHeader from './components/AppHeader.vue'
import ToastContainer from './components/ToastContainer.vue'
import { useAuthStore } from './stores/auth'

const auth = useAuthStore()
</script>

<template>
  <div class="app-shell" :class="{ 'auth-only': !auth.isAuthenticated }">
    <AppHeader v-if="auth.isAuthenticated" />
    <main :class="auth.isAuthenticated ? 'container page' : 'auth-page'">
      <RouterView />
    </main>
    <ToastContainer />
  </div>
</template>
