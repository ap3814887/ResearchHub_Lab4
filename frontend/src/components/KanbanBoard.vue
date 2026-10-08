<script setup lang="ts">
import type { ProjectMember, TaskItem, TaskStatus } from '../types'

const props = defineProps<{ tasks: TaskItem[]; members: ProjectMember[]; canEdit?: boolean }>()
const emit = defineEmits<{ (e: 'status-change', task: TaskItem, status: TaskStatus): void }>()

const columns: { status: TaskStatus; title: string }[] = [
  { status: 'New', title: 'К выполнению' },
  { status: 'InProgress', title: 'В работе' },
  { status: 'Done', title: 'Готово' },
  { status: 'Cancelled', title: 'Отменено' }
]

function tasksFor(status: TaskStatus) {
  return props.tasks.filter(x => x.status === status)
}

function dragStart(event: DragEvent, task: TaskItem) {
  if (!props.canEdit || !event.dataTransfer) return
  event.dataTransfer.effectAllowed = 'move'
  event.dataTransfer.setData('text/plain', String(task.id))
}

function drop(event: DragEvent, status: TaskStatus) {
  if (!props.canEdit) return
  const id = Number(event.dataTransfer?.getData('text/plain'))
  const task = props.tasks.find(x => x.id === id)
  if (task && task.status !== status) emit('status-change', task, status)
}

function memberName(id: number | null) {
  return id ? props.members.find(x => x.userId === id)?.userName || `ID ${id}` : 'Не назначен'
}
</script>

<template>
  <div class="kanban">
    <section
      v-for="column in columns"
      :key="column.status"
      class="kanban-column"
      :data-status="column.status"
      @dragover.prevent
      @drop="drop($event, column.status)"
    >
      <div class="kanban-heading">
        <strong>{{ column.title }}</strong>
        <span class="badge">{{ tasksFor(column.status).length }}</span>
      </div>
      <div class="kanban-stack">
        <article
          v-for="task in tasksFor(column.status)"
          :key="task.id"
          class="kanban-card"
          :draggable="canEdit"
          @dragstart="dragStart($event, task)"
        >
          <div class="kanban-card-top">
            <span class="kanban-id">#{{ task.id }}</span>
            <span class="priority" :data-priority="task.priority">{{ task.priority }}</span>
          </div>
          <strong>{{ task.title }}</strong>
          <small>{{ memberName(task.assigneeId) }}</small>
        </article>
        <div v-if="!tasksFor(column.status).length" class="kanban-empty">Перетащите задачу сюда</div>
      </div>
    </section>
  </div>
</template>
