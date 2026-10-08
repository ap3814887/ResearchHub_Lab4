<script setup lang="ts">
import type { ProjectMember, TaskItem, TaskPriority, TaskStatus } from '../types'

const props = defineProps<{
  task: TaskItem
  members: ProjectMember[]
  canEdit?: boolean
}>()

const emit = defineEmits<{
  (e: 'edit', task: TaskItem): void
  (e: 'delete', task: TaskItem): void
  (e: 'status', task: TaskItem, status: TaskStatus): void
  (e: 'priority', task: TaskItem, priority: TaskPriority): void
  (e: 'assign', task: TaskItem, assigneeId: number | null): void
}>()

function memberName(id: number | null) {
  if (!id) return 'Не назначен'
  return props.members.find(x => x.userId === id)?.userName || `ID ${id}`
}
</script>

<template>
  <article class="card task-card">
    <div class="task-title-row">
      <div>
        <div class="eyebrow">#{{ task.id }}</div>
        <h3>{{ task.title }}</h3>
      </div>
      <span class="priority" :data-priority="task.priority">{{ task.priority }}</span>
    </div>
    <p class="muted">{{ task.description || 'Без описания' }}</p>
    <div class="task-meta">
      <span class="status-pill" :data-status="task.status">{{ task.status }}</span>
      <span>Исполнитель: <strong>{{ memberName(task.assigneeId) }}</strong></span>
      <span>Срок: <strong>{{ task.dueDate ? new Date(`${task.dueDate}T00:00:00`).toLocaleDateString('ru-RU') : 'не задан' }}</strong></span>
    </div>
    <div v-if="canEdit" class="task-controls">
      <select :value="task.status" @change="emit('status', task, ($event.target as HTMLSelectElement).value as TaskStatus)">
        <option value="New">New</option>
        <option value="InProgress">InProgress</option>
        <option value="Done">Done</option>
        <option value="Cancelled">Cancelled</option>
      </select>
      <select :value="task.priority" @change="emit('priority', task, ($event.target as HTMLSelectElement).value as TaskPriority)">
        <option value="Low">Low</option>
        <option value="Normal">Normal</option>
        <option value="High">High</option>
        <option value="Critical">Critical</option>
      </select>
      <select :value="task.assigneeId ?? ''" @change="emit('assign', task, ($event.target as HTMLSelectElement).value ? Number(($event.target as HTMLSelectElement).value) : null)">
        <option value="">Не назначен</option>
        <option v-for="m in members" :key="m.userId" :value="m.userId">{{ m.userName || `ID ${m.userId}` }}</option>
      </select>
      <button class="btn btn-secondary" @click="emit('edit', task)">Изменить</button>
      <button class="btn btn-danger ghost" @click="emit('delete', task)">Удалить</button>
    </div>
  </article>
</template>
