<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import ErrorMessage from '../components/ErrorMessage.vue'
import { useAuthStore } from '../stores/auth'
import { useThemeStore } from '../stores/theme'
import { useToastStore } from '../stores/toast'

const auth = useAuthStore()
const theme = useThemeStore()
const toast = useToastStore()
const router = useRouter()
const route = useRoute()
const mode = ref<'login' | 'register'>('login')
const loginForm = reactive({ userId: 1, password: '' })
const registerForm = reactive({ name: '', email: '', password: '' })
const localError = ref('')
const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

const error = computed(() => localError.value || auth.error)

async function afterAuth() {
  const redirect = typeof route.query.redirect === 'string' ? route.query.redirect : '/projects'
  await router.replace(redirect)
}

async function submitLogin() {
  localError.value = ''
  if (loginForm.userId < 1 || !loginForm.password) {
    localError.value = 'Укажите ID пользователя и пароль.'
    return
  }
  try {
    await auth.login(loginForm.userId, loginForm.password)
    toast.push('Вход выполнен.', 'success')
    await afterAuth()
  } catch {
    // сообщение хранится в auth.error
  }
}

async function submitRegister() {
  localError.value = ''
  if (!registerForm.name.trim()) {
    localError.value = 'Укажите имя.'
    return
  }
  if (!emailPattern.test(registerForm.email.trim())) {
    localError.value = 'Укажите корректный email.'
    return
  }
  if (!registerForm.password) {
    localError.value = 'Укажите пароль.'
    return
  }
  try {
    await auth.register(registerForm.name.trim(), registerForm.email.trim(), registerForm.password)
    toast.push('Регистрация выполнена. Вы вошли в систему.', 'success')
    await afterAuth()
  } catch {
    // сообщение хранится в auth.error
  }
}

function switchMode(value: 'login' | 'register') {
  mode.value = value
  localError.value = ''
  auth.error = ''
}
</script>

<template>
  <section class="auth-screen">
    <button class="auth-theme icon-btn" :title="theme.dark ? 'Светлая тема' : 'Тёмная тема'" @click="theme.toggle()">{{ theme.dark ? '☀' : '☾' }}</button>

    <div class="auth-brand">
      <span class="brand-mark auth-brand-mark">R</span>
      <div>
        <strong>ResearchHub</strong>
        <span>Управление проектами и задачами</span>
      </div>
    </div>

    <div class="card auth-card auth-card-large">
      <div class="auth-tabs" role="tablist">
        <button :class="{ active: mode === 'login' }" type="button" @click="switchMode('login')">Вход</button>
        <button :class="{ active: mode === 'register' }" type="button" @click="switchMode('register')">Регистрация</button>
      </div>

      <div class="auth-title">
        <h1>{{ mode === 'login' ? 'Войти в систему' : 'Создать аккаунт' }}</h1>
        <p class="muted">{{ mode === 'login' ? 'После входа будут доступны проекты и задачи.' : 'После регистрации вход выполняется автоматически.' }}</p>
      </div>

      <ErrorMessage :message="error" />

      <form v-if="mode === 'login'" class="auth-form" @submit.prevent="submitLogin">
        <label>
          ID пользователя
          <input v-model.number="loginForm.userId" type="number" min="1" required placeholder="Например, 1" autocomplete="username" />
        </label>
        <label>
          Пароль
          <input v-model="loginForm.password" type="password" required placeholder="Введите пароль" autocomplete="current-password" />
        </label>
        <button class="btn btn-primary" :disabled="auth.loading">{{ auth.loading ? 'Вход...' : 'Войти' }}</button>
      </form>

      <form v-else class="auth-form" @submit.prevent="submitRegister">
        <label>
          Имя
          <input v-model="registerForm.name" maxlength="150" required placeholder="Ваше имя" autocomplete="name" />
        </label>
        <label>
          Email
          <input v-model="registerForm.email" type="email" maxlength="255" required placeholder="name@example.com" autocomplete="email" />
        </label>
        <label>
          Пароль
          <input v-model="registerForm.password" type="password" required placeholder="Введите пароль доступа" autocomplete="new-password" />
        </label>
        <button class="btn btn-primary" :disabled="auth.loading">{{ auth.loading ? 'Регистрация...' : 'Зарегистрироваться' }}</button>
      </form>
    </div>

    <p class="auth-footer">Данные приложения доступны только после авторизации.</p>
  </section>
</template>
