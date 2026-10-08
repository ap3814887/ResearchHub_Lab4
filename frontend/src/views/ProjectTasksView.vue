<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import { getMembers, getProject, getProjectTasks } from '../api/projectsApi'
import { assignTask, changeTaskPriority, changeTaskStatus, createTask, deleteTask, updateTask } from '../api/tasksApi'
import ErrorMessage from '../components/ErrorMessage.vue'
import KanbanBoard from '../components/KanbanBoard.vue'
import LoadingIndicator from '../components/LoadingIndicator.vue'
import PaginationControls from '../components/PaginationControls.vue'
import TaskFilters from '../components/TaskFilters.vue'
import TaskForm from '../components/TaskForm.vue'
import TaskList from '../components/TaskList.vue'
import { ApiError } from '../api/client'
import { useAuthStore } from '../stores/auth'
import { useToastStore } from '../stores/toast'
import type { Project, ProjectMember, TaskItem, TaskPriority, TaskStatus } from '../types'

const route = useRoute()
const projectId = Number(route.params.id)
const auth = useAuthStore()
const toast = useToastStore()

const project = ref<Project | null>(null)
const members = ref<ProjectMember[]>([])
const tasks = ref<TaskItem[]>([])
const loading = ref(true)
const submitting = ref(false)
const error = ref('')
const status = ref<TaskStatus | ''>('')
const priority = ref<TaskPriority | ''>('')
const assigneeId = ref<number | null>(null)
const search = ref('')
const sort = ref('updated-desc')
const page = ref(1)
const pageSize = ref(10)
const totalPages = ref(0)
const totalCount = ref(0)
const showForm = ref(false)
const editing = ref<TaskItem | null>(null)
const viewMode = ref<'list' | 'kanban'>('list')

const priorityRank: Record<TaskPriority, number> = { Low: 1, Normal: 2, High: 3, Critical: 4 }
const canManage = computed(() => Boolean(project.value && auth.user && project.value.ownerId === auth.user.userId))

const visibleTasks = computed(() => {
  const term = search.value.trim().toLowerCase()
  let result = tasks.value.filter(t => !term || t.title.toLowerCase().includes(term) || (t.description || '').toLowerCase().includes(term))
  result = [...result]
  if (sort.value === 'title-asc') result.sort((a, b) => a.title.localeCompare(b.title, 'ru'))
  if (sort.value === 'due-asc') result.sort((a, b) => (a.dueDate || '9999-12-31').localeCompare(b.dueDate || '9999-12-31'))
  if (sort.value === 'priority-desc') result.sort((a, b) => priorityRank[b.priority] - priorityRank[a.priority])
  if (sort.value === 'updated-desc') result.sort((a, b) => b.updatedAt.localeCompare(a.updatedAt))
  return result
})

async function load() {
  loading.value = true
  error.value = ''
  try {
    const [p, m, result] = await Promise.all([
      getProject(projectId),
      getMembers(projectId),
      getProjectTasks(projectId, { status: status.value, priority: priority.value, assigneeId: assigneeId.value, page: page.value, pageSize: pageSize.value })
    ])
    project.value = p
    members.value = m
    tasks.value = result.items
    totalPages.value = result.totalPages
    totalCount.value = result.totalCount
  } catch (e) {
    error.value = e instanceof ApiError ? e.message : 'Не удалось загрузить задачи.'
  } finally {
    loading.value = false
  }
}

async function submit(value: {
  projectId: number
  title: string
  description: string | null
  assigneeId: number | null
  status: TaskStatus
  priority: TaskPriority
  dueDate: string | null
}) {
  if (!canManage.value) return
  submitting.value = true
  error.value = ''
  try {
    if (editing.value) {
      await updateTask(editing.value.id, {
        title: value.title,
        description: value.description,
        assigneeId: value.assigneeId,
        priority: value.priority,
        dueDate: value.dueDate
      })
      toast.push('Задача изменена.', 'success')
    } else {
      await createTask(value)
      toast.push('Задача создана.', 'success')
    }
    editing.value = null
    showForm.value = false
    await load()
  } catch (e) {
    error.value = e instanceof ApiError ? e.message : 'Не удалось сохранить задачу.'
  } finally {
    submitting.value = false
  }
}

function edit(task: TaskItem) {
  if (!canManage.value) return
  editing.value = task
  showForm.value = true
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

async function remove(task: TaskItem) {
  if (!canManage.value) return
  if (!confirm(`Удалить задачу «${task.title}»?`)) return
  try {
    await deleteTask(task.id)
    toast.push('Задача удалена.', 'success')
    await load()
  } catch (e) {
    error.value = e instanceof ApiError ? e.message : 'Не удалось удалить задачу.'
  }
}

async function setStatus(task: TaskItem, value: TaskStatus) {
  if (!canManage.value) return
  try {
    await changeTaskStatus(task.id, value)
    toast.push(`Статус: ${task.status} → ${value}`, 'success')
    await load()
  } catch (e) {
    error.value = e instanceof ApiError ? e.message : 'Не удалось изменить статус.'
    toast.push(error.value, 'error')
    await load()
  }
}

async function setPriority(task: TaskItem, value: TaskPriority) {
  if (!canManage.value) return
  try {
    await changeTaskPriority(task.id, value)
    toast.push(`Приоритет изменён на ${value}.`, 'success')
    await load()
  } catch (e) {
    error.value = e instanceof ApiError ? e.message : 'Не удалось изменить приоритет.'
  }
}

async function setAssignee(task: TaskItem, value: number | null) {
  if (!canManage.value) return
  try {
    await assignTask(task.id, value)
    toast.push('Исполнитель изменён.', 'success')
    await load()
  } catch (e) {
    error.value = e instanceof ApiError ? e.message : 'Не удалось назначить исполнителя.'
    toast.push(error.value, 'error')
    await load()
  }
}

function resetFilters() {
  status.value = ''
  priority.value = ''
  assigneeId.value = null
  search.value = ''
  sort.value = 'updated-desc'
  page.value = 1
}

watch([status, priority, assigneeId, page, pageSize], () => load())
onMounted(load)
</script>

<template>
  <section>
    <div class="page-heading">
      <div>
        <div class="project-title-line" v-if="project">
          <span v-if="canManage" class="owner-tag">Мой проект</span>
        </div>
        <h1>{{ project?.name || 'Задачи проекта' }}</h1>
        <p class="muted">Найдено задач: {{ totalCount }}</p>
      </div>
      <div class="header-buttons">
        <RouterLink class="btn btn-secondary" :to="`/projects/${projectId}`">О проекте</RouterLink>
        <button class="btn btn-secondary" @click="viewMode = viewMode === 'list' ? 'kanban' : 'list'">{{ viewMode === 'list' ? 'Доска' : 'Список' }}</button>
        <button v-if="canManage" class="btn btn-primary" @click="showForm = !showForm; if (!showForm) editing = null">+ Задача</button>
      </div>
    </div>

    <ErrorMessage :message="error" />

    <div v-if="project && !canManage" class="read-only-note card">
      Задачи доступны для просмотра и фильтрации. Изменять их может только владелец проекта.
    </div>

    <div v-if="showForm && canManage" class="card form-panel">
      <h2>{{ editing ? 'Изменение задачи' : 'Новая задача' }}</h2>
      <TaskForm :project-id="projectId" :members="members" :initial="editing" :submitting="submitting" @submit="submit" @cancel="editing = null; showForm = false" />
    </div>

    <TaskFilters
      v-model:status="status"
      v-model:priority="priority"
      v-model:assignee-id="assigneeId"
      v-model:search="search"
      v-model:sort="sort"
      :members="members"
      @reset="resetFilters"
    />

    <LoadingIndicator v-if="loading" />
    <TaskList
      v-else-if="viewMode === 'list'"
      :tasks="visibleTasks"
      :members="members"
      :can-edit="canManage"
      @edit="edit"
      @delete="remove"
      @status="setStatus"
      @priority="setPriority"
      @assign="setAssignee"
    />
    <KanbanBoard v-else :tasks="visibleTasks" :members="members" :can-edit="canManage" @status-change="setStatus" />

    <PaginationControls
      :page="page"
      :page-size="pageSize"
      :total-pages="totalPages"
      @update:page="page = $event"
      @update:page-size="pageSize = $event; page = 1"
    />
  </section>
</template>
