<script setup lang="ts">
import TaskCard from './TaskCard.vue'
import type { ProjectMember, TaskItem, TaskPriority, TaskStatus } from '../types'

defineProps<{ tasks: TaskItem[]; members: ProjectMember[]; canEdit?: boolean }>()
defineEmits<{
  (e: 'edit', task: TaskItem): void
  (e: 'delete', task: TaskItem): void
  (e: 'status', task: TaskItem, status: TaskStatus): void
  (e: 'priority', task: TaskItem, priority: TaskPriority): void
  (e: 'assign', task: TaskItem, assigneeId: number | null): void
}>()
</script>

<template>
  <div v-if="tasks.length" class="stack">
    <TaskCard
      v-for="task in tasks"
      :key="task.id"
      :task="task"
      :members="members"
      :can-edit="canEdit"
      @edit="$emit('edit', $event)"
      @delete="$emit('delete', $event)"
      @status="(task, status) => $emit('status', task, status)"
      @priority="(task, priority) => $emit('priority', task, priority)"
      @assign="(task, assigneeId) => $emit('assign', task, assigneeId)"
    />
  </div>
  <div v-else class="empty-state card">По выбранным условиям задач нет.</div>
</template>
