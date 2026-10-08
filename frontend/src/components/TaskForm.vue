<script setup lang="ts">
import { reactive, watch } from 'vue'
import type { ProjectMember, TaskItem, TaskPriority, TaskStatus } from '../types'

const props = defineProps<{
  projectId: number
  members: ProjectMember[]
  initial?: TaskItem | null
  submitting?: boolean
}>()

const emit = defineEmits<{
  (e: 'submit', value: {
    projectId: number
    title: string
    description: string | null
    assigneeId: number | null
    status: TaskStatus
    priority: TaskPriority
    dueDate: string | null
  }): void
  (e: 'cancel'): void
}>()

const form = reactive({
  title: '',
  description: '',
  assigneeId: null as number | null,
  status: 'New' as TaskStatus,
  priority: 'Normal' as TaskPriority,
  dueDate: ''
})

watch(() => props.initial, t => {
  form.title = t?.title || ''
  form.description = t?.description || ''
  form.assigneeId = t?.assigneeId ?? null
  form.status = t?.status || 'New'
  form.priority = t?.priority || 'Normal'
  form.dueDate = t?.dueDate || ''
}, { immediate: true })

function submit() {
  if (!form.title.trim()) return
  emit('submit', {
    projectId: props.projectId,
    title: form.title.trim(),
    description: form.description.trim() || null,
    assigneeId: form.assigneeId,
    status: form.status,
    priority: form.priority,
    dueDate: form.dueDate || null
  })
}
</script>

<template>
  <form class="form-grid" @submit.prevent="submit">
    <label class="span-2">
      Название *
      <input v-model.trim="form.title" maxlength="250" required placeholder="Что нужно сделать?" />
    </label>
    <label class="span-2">
      Описание
      <textarea v-model="form.description" rows="3"></textarea>
    </label>
    <label>
      Исполнитель
      <select v-model="form.assigneeId">
        <option :value="null">Не назначен</option>
        <option v-for="m in members" :key="m.userId" :value="m.userId">{{ m.userName || `ID ${m.userId}` }}</option>
      </select>
    </label>
    <label v-if="!initial">
      Статус
      <select v-model="form.status">
        <option value="New">New</option>
        <option value="InProgress">InProgress</option>
        <option value="Done">Done</option>
        <option value="Cancelled">Cancelled</option>
      </select>
    </label>
    <label>
      Приоритет
      <select v-model="form.priority">
        <option value="Low">Low</option>
        <option value="Normal">Normal</option>
        <option value="High">High</option>
        <option value="Critical">Critical</option>
      </select>
    </label>
    <label>
      Срок
      <input v-model="form.dueDate" type="date" />
    </label>
    <div class="form-actions span-2">
      <button class="btn btn-primary" :disabled="submitting">{{ submitting ? 'Сохранение...' : (initial ? 'Сохранить изменения' : 'Создать задачу') }}</button>
      <button v-if="initial" class="btn btn-secondary" type="button" @click="$emit('cancel')">Отмена</button>
    </div>
  </form>
</template>
