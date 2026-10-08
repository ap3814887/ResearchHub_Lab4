<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { createUser, deleteUser, getUsers } from '../api/usersApi'
import ErrorMessage from '../components/ErrorMessage.vue'
import LoadingIndicator from '../components/LoadingIndicator.vue'
import PaginationControls from '../components/PaginationControls.vue'
import { ApiError } from '../api/client'
import { useAuthStore } from '../stores/auth'
import { useToastStore } from '../stores/toast'
import type { User } from '../types'

const auth = useAuthStore()
const toast = useToastStore()
const users = ref<User[]>([])
const loading = ref(true)
const submitting = ref(false)
const error = ref('')
const page = ref(1)
const pageSize = ref(10)
const totalPages = ref(0)
const search = ref('')
const sort = ref('name-asc')
const form = reactive({ name: '', email: '' })
const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

const visible = computed(() => {
  const term = search.value.trim().toLowerCase()
  const result = users.value.filter(u => !term || u.name.toLowerCase().includes(term) || u.email.toLowerCase().includes(term))
  return [...result].sort((a, b) => sort.value === 'email-asc' ? a.email.localeCompare(b.email) : a.name.localeCompare(b.name, 'ru'))
})

async function load() {
  loading.value = true
  error.value = ''
  try {
    const result = await getUsers(page.value, pageSize.value)
    users.value = result.items
    totalPages.value = result.totalPages
  } catch (e) {
    error.value = e instanceof ApiError ? e.message : 'Не удалось загрузить пользователей.'
  } finally {
    loading.value = false
  }
}

async function create() {
  if (!form.name.trim() || !emailPattern.test(form.email)) {
    error.value = 'Укажите имя и корректный email.'
    return
  }
  submitting.value = true
  try {
    await createUser({ name: form.name.trim(), email: form.email.trim() })
    form.name = ''
    form.email = ''
    toast.push('Пользователь создан.', 'success')
    await load()
  } catch (e) {
    error.value = e instanceof ApiError ? e.message : 'Не удалось создать пользователя.'
  } finally {
    submitting.value = false
  }
}

async function remove(user: User) {
  if (!confirm(`Удалить пользователя ${user.name}?`)) return
  try {
    await deleteUser(user.id)
    toast.push('Пользователь удалён.', 'success')
    await load()
  } catch (e) {
    error.value = e instanceof ApiError ? e.message : 'Не удалось удалить пользователя.'
  }
}

watch([page, pageSize], load)
onMounted(load)
</script>

<template>
  <section>
    <div class="page-heading">
      <div><div class="eyebrow">Справочник</div><h1>Пользователи</h1></div>
    </div>
    <ErrorMessage :message="error" />

    <div v-if="auth.isAuthenticated" class="card form-panel">
      <h2>Новый пользователь</h2>
      <form class="form-grid" @submit.prevent="create">
        <label>Имя *<input v-model="form.name" required maxlength="150" /></label>
        <label>Email *<input v-model="form.email" required type="email" maxlength="255" /></label>
        <button class="btn btn-primary span-2" :disabled="submitting">{{ submitting ? 'Создание...' : 'Создать' }}</button>
      </form>
    </div>

    <div class="toolbar card">
      <input v-model="search" placeholder="Поиск по имени или email" />
      <select v-model="sort">
        <option value="name-asc">По имени</option>
        <option value="email-asc">По email</option>
      </select>
    </div>

    <LoadingIndicator v-if="loading" />
    <div v-else-if="visible.length" class="table-card card">
      <table>
        <thead><tr><th>ID</th><th>Имя</th><th>Email</th><th>Создан</th><th></th></tr></thead>
        <tbody>
          <tr v-for="u in visible" :key="u.id">
            <td>{{ u.id }}</td><td><strong>{{ u.name }}</strong></td><td>{{ u.email }}</td>
            <td>{{ new Date(u.createdAt).toLocaleDateString('ru-RU') }}</td>
            <td><button v-if="auth.isAuthenticated" class="btn btn-danger ghost" @click="remove(u)">Удалить</button></td>
          </tr>
        </tbody>
      </table>
    </div>
    <div v-else class="empty-state card">Пользователи отсутствуют.</div>

    <PaginationControls
      :page="page"
      :page-size="pageSize"
      :total-pages="totalPages"
      @update:page="page = $event"
      @update:page-size="pageSize = $event; page = 1"
    />
  </section>
</template>
