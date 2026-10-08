<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import { addMember, getMembers, getProject, getProjectTasks, getTaskStats, removeMember, updateProject } from '../api/projectsApi'
import { getUser, getUsers } from '../api/usersApi'
import ErrorMessage from '../components/ErrorMessage.vue'
import LoadingIndicator from '../components/LoadingIndicator.vue'
import ProjectForm from '../components/ProjectForm.vue'
import ProjectMemberList from '../components/ProjectMemberList.vue'
import { ApiError } from '../api/client'
import { useAuthStore } from '../stores/auth'
import { useToastStore } from '../stores/toast'
import type { Project, ProjectMember, TaskStatusCount, User } from '../types'

const route = useRoute()
const projectId = Number(route.params.id)
const auth = useAuthStore()
const toast = useToastStore()

const project = ref<Project | null>(null)
const owner = ref<User | null>(null)
const members = ref<ProjectMember[]>([])
const users = ref<User[]>([])
const stats = ref<TaskStatusCount[]>([])
const taskCount = ref(0)
const loading = ref(true)
const error = ref('')
const editing = ref(false)
const submitting = ref(false)
const memberSubmitting = ref(false)

const statsMap = computed(() => Object.fromEntries(stats.value.map(x => [x.status, x.taskCount])))
const canManage = computed(() => Boolean(project.value && auth.user && project.value.ownerId === auth.user.userId))

async function load() {
  loading.value = true
  error.value = ''
  try {
    project.value = await getProject(projectId)
    const [m, u, s, tasks] = await Promise.all([
      getMembers(projectId),
      getUsers(1, 100),
      getTaskStats(projectId),
      getProjectTasks(projectId, { page: 1, pageSize: 1 })
    ])
    members.value = m
    users.value = u.items
    stats.value = s
    taskCount.value = tasks.totalCount
    owner.value = await getUser(project.value.ownerId)
  } catch (e) {
    error.value = e instanceof ApiError ? e.message : 'Не удалось открыть проект.'
  } finally {
    loading.value = false
  }
}

async function add(value: { userId: number; role: string }) {
  if (!canManage.value || memberSubmitting.value) return
  error.value = ''
  memberSubmitting.value = true
  try {
    await addMember(projectId, value)
    toast.push('Участник добавлен.', 'success')
    members.value = await getMembers(projectId)
  } catch (e) {
    error.value = e instanceof ApiError ? e.message : 'Не удалось добавить участника.'
    toast.push(error.value, 'error')
  } finally {
    memberSubmitting.value = false
  }
}

async function remove(member: ProjectMember) {
  if (!canManage.value) return
  if (!confirm(`Удалить ${member.userName || `пользователя ${member.userId}`} из проекта?`)) return
  try {
    await removeMember(projectId, member.userId)
    toast.push('Участник удалён.', 'success')
    members.value = await getMembers(projectId)
  } catch (e) {
    error.value = e instanceof ApiError ? e.message : 'Не удалось удалить участника.'
  }
}

async function save(value: { name: string; description: string | null }) {
  if (!canManage.value) return
  submitting.value = true
  try {
    project.value = await updateProject(projectId, { name: value.name, description: value.description })
    editing.value = false
    toast.push('Проект изменён.', 'success')
  } catch (e) {
    error.value = e instanceof ApiError ? e.message : 'Не удалось изменить проект.'
  } finally {
    submitting.value = false
  }
}

onMounted(load)
</script>

<template>
  <section>
    <LoadingIndicator v-if="loading" />
    <template v-else>
      <ErrorMessage :message="error" />
      <div v-if="project">
        <div class="page-heading">
          <div>
            <div class="project-title-line">
              <span class="eyebrow">#{{ project.id }}</span>
              <span v-if="canManage" class="owner-tag">Мой проект</span>
            </div>
            <h1>{{ project.name }}</h1>
            <p class="muted">{{ project.description || 'Описание не задано' }}</p>
          </div>
          <div class="header-buttons">
            <RouterLink class="btn btn-primary" :to="`/projects/${project.id}/tasks`">Открыть задачи</RouterLink>
            <button v-if="canManage" class="btn btn-secondary" @click="editing = !editing">Изменить проект</button>
          </div>
        </div>

        <div v-if="!canManage" class="read-only-note card">
          Проект доступен для просмотра. Изменения может выполнять только владелец.
        </div>

        <div v-if="editing && canManage" class="card form-panel">
          <ProjectForm :initial="project" :submitting="submitting" @submit="save" @cancel="editing = false" />
        </div>

        <div class="stats-grid">
          <div class="stat card"><span>Владелец</span><strong>{{ owner?.name || `ID ${project.ownerId}` }}</strong></div>
          <div class="stat card"><span>Участники</span><strong>{{ members.length }}</strong></div>
          <div class="stat card"><span>Задачи</span><strong>{{ taskCount }}</strong></div>
          <div class="stat card"><span>В работе</span><strong>{{ statsMap.InProgress || 0 }}</strong></div>
        </div>

        <div class="two-column">
          <ProjectMemberList :members="members" :users="users" :can-edit="canManage" :loading="memberSubmitting" @add="add" @remove="remove" />
          <section class="card">
            <div class="eyebrow">Статистика</div>
            <h2>Задачи по статусам</h2>
            <div class="stats-list">
              <div v-for="s in stats" :key="s.status"><span>{{ s.status }}</span><strong>{{ s.taskCount }}</strong></div>
              <div v-if="!stats.length" class="empty-state">Задач пока нет.</div>
            </div>
            <p class="muted small">Создан: {{ new Date(project.createdAt).toLocaleString('ru-RU') }}</p>
          </section>
        </div>
      </div>
    </template>
  </section>
</template>
