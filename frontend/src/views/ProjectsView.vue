<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { createProject, deleteProject, getProjects } from '../api/projectsApi'
import { getUser } from '../api/usersApi'
import ErrorMessage from '../components/ErrorMessage.vue'
import LoadingIndicator from '../components/LoadingIndicator.vue'
import PaginationControls from '../components/PaginationControls.vue'
import ProjectCard from '../components/ProjectCard.vue'
import ProjectForm from '../components/ProjectForm.vue'
import { ApiError } from '../api/client'
import { useAuthStore } from '../stores/auth'
import { useToastStore } from '../stores/toast'
import type { Project } from '../types'

const auth = useAuthStore()
const toast = useToastStore()

const projects = ref<Project[]>([])
const ownerNames = ref<Record<number, string>>({})
const loading = ref(true)
const submitting = ref(false)
const error = ref('')
const page = ref(1)
const pageSize = ref(10)
const totalPages = ref(0)
const totalCount = ref(0)
const search = ref('')
const sort = ref('created-desc')
const showForm = ref(false)

const currentUserId = computed(() => auth.user?.userId ?? 0)
const visibleProjects = computed(() => {
  const term = search.value.trim().toLowerCase()
  let result = projects.value.filter(p => !term || p.name.toLowerCase().includes(term) || (p.description || '').toLowerCase().includes(term))
  result = [...result]
  if (sort.value === 'name-asc') result.sort((a, b) => a.name.localeCompare(b.name, 'ru'))
  if (sort.value === 'created-desc') result.sort((a, b) => b.createdAt.localeCompare(a.createdAt))
  if (sort.value === 'created-asc') result.sort((a, b) => a.createdAt.localeCompare(b.createdAt))
  return result
})

function canManage(project: Project) {
  return project.ownerId === currentUserId.value
}

async function load() {
  loading.value = true
  error.value = ''
  try {
    const result = await getProjects(page.value, pageSize.value)
    projects.value = result.items
    totalPages.value = result.totalPages
    totalCount.value = result.totalCount
    await Promise.all([...new Set(result.items.map(x => x.ownerId))].map(async id => {
      try { ownerNames.value[id] = (await getUser(id)).name } catch { ownerNames.value[id] = `ID ${id}` }
    }))
  } catch (e) {
    error.value = e instanceof ApiError ? e.message : 'Не удалось загрузить проекты.'
  } finally {
    loading.value = false
  }
}

async function create(value: { name: string; description: string | null; ownerId: number }) {
  if (!currentUserId.value) return
  submitting.value = true
  error.value = ''
  try {
    await createProject({ ...value, ownerId: currentUserId.value })
    toast.push('Проект создан.', 'success')
    showForm.value = false
    page.value = 1
    await load()
  } catch (e) {
    error.value = e instanceof ApiError ? e.message : 'Не удалось создать проект.'
  } finally {
    submitting.value = false
  }
}

async function remove(project: Project) {
  if (!canManage(project)) {
    error.value = 'Редактировать и удалять можно только собственные проекты.'
    return
  }
  if (!confirm(`Удалить проект «${project.name}»? Связанные данные будут обработаны PostgreSQL.`)) return
  try {
    await deleteProject(project.id)
    toast.push('Проект удалён.', 'success')
    await load()
  } catch (e) {
    error.value = e instanceof ApiError ? e.message : 'Не удалось удалить проект.'
  }
}

watch([page, pageSize], () => load())
onMounted(load)
</script>

<template>
  <section>
    <div class="page-heading">
      <div>
        <h1>Проекты</h1>
        <p class="muted"></p>
      </div>
      <button class="btn btn-primary" @click="showForm = !showForm">{{ showForm ? 'Скрыть форму' : '+ Новый проект' }}</button>
    </div>

    <ErrorMessage :message="error" />

    <div v-if="showForm" class="card form-panel">
      <h2>Создание проекта</h2>
      <ProjectForm :submitting="submitting" :default-owner-id="currentUserId" @submit="create" />
    </div>

    <div class="toolbar card">
      <input v-model="search" placeholder="Поиск по проектам..." />
      <select v-model="sort">
        <option value="created-desc">Сначала новые</option>
        <option value="created-asc">Сначала старые</option>
        <option value="name-asc">По названию</option>
      </select>
    </div>

    <LoadingIndicator v-if="loading" />
    <div v-else-if="visibleProjects.length" class="project-grid">
      <ProjectCard
        v-for="project in visibleProjects"
        :key="project.id"
        :project="project"
        :owner-name="ownerNames[project.ownerId]"
        :can-manage="canManage(project)"
        @delete="remove"
      />
    </div>
    <div v-else class="empty-state card">Проекты отсутствуют или не найдены.</div>

    <PaginationControls
      :page="page"
      :page-size="pageSize"
      :total-pages="totalPages"
      @update:page="page = $event"
      @update:page-size="pageSize = $event; page = 1"
    />
  </section>
</template>
