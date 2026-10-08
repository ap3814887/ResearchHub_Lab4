<script setup lang="ts">
import type { ProjectMember, TaskPriority, TaskStatus } from '../types'

const props = defineProps<{
  status: TaskStatus | ''
  priority: TaskPriority | ''
  assigneeId: number | null
  search: string
  sort: string
  members: ProjectMember[]
}>()

const emit = defineEmits<{
  (e: 'update:status', value: TaskStatus | ''): void
  (e: 'update:priority', value: TaskPriority | ''): void
  (e: 'update:assigneeId', value: number | null): void
  (e: 'update:search', value: string): void
  (e: 'update:sort', value: string): void
  (e: 'reset'): void
}>()
</script>

<template>
  <div class="filters card">
    <label>
      Поиск
      <input :value="props.search" placeholder="Название или описание" @input="emit('update:search', ($event.target as HTMLInputElement).value)" />
    </label>
    <label>
      Статус
      <select :value="props.status" @change="emit('update:status', ($event.target as HTMLSelectElement).value as TaskStatus | '')">
        <option value="">Все</option>
        <option value="New">New</option>
        <option value="InProgress">InProgress</option>
        <option value="Done">Done</option>
        <option value="Cancelled">Cancelled</option>
      </select>
    </label>
    <label>
      Приоритет
      <select :value="props.priority" @change="emit('update:priority', ($event.target as HTMLSelectElement).value as TaskPriority | '')">
        <option value="">Все</option>
        <option value="Low">Low</option>
        <option value="Normal">Normal</option>
        <option value="High">High</option>
        <option value="Critical">Critical</option>
      </select>
    </label>
    <label>
      Исполнитель
      <select :value="props.assigneeId ?? ''" @change="emit('update:assigneeId', ($event.target as HTMLSelectElement).value ? Number(($event.target as HTMLSelectElement).value) : null)">
        <option value="">Все</option>
        <option v-for="m in members" :key="m.userId" :value="m.userId">{{ m.userName || `ID ${m.userId}` }}</option>
      </select>
    </label>
    <label>
      Сортировка
      <select :value="props.sort" @change="emit('update:sort', ($event.target as HTMLSelectElement).value)">
        <option value="updated-desc">Сначала изменённые</option>
        <option value="due-asc">По сроку</option>
        <option value="title-asc">По названию</option>
        <option value="priority-desc">По приоритету</option>
      </select>
    </label>
    <button class="btn btn-secondary align-end" type="button" @click="emit('reset')">Сбросить</button>
  </div>
</template>
