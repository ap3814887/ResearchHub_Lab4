<script setup lang="ts">
import { reactive, watch } from 'vue'
import type { Project } from '../types'

const props = defineProps<{
  submitting?: boolean
  initial?: Project | null
  defaultOwnerId?: number
}>()

const emit = defineEmits<{
  (e: 'submit', value: { name: string; description: string | null; ownerId: number }): void
  (e: 'cancel'): void
}>()

const form = reactive({
  name: '',
  description: '',
  ownerId: props.defaultOwnerId || props.initial?.ownerId || 1
})

watch(() => props.initial, value => {
  if (value) {
    form.name = value.name
    form.description = value.description || ''
    form.ownerId = value.ownerId
  }
}, { immediate: true })

watch(() => props.defaultOwnerId, value => {
  if (!props.initial && value) form.ownerId = value
})

function submit() {
  if (!form.name.trim() || form.ownerId < 1) return
  emit('submit', {
    name: form.name.trim(),
    description: form.description.trim() || null,
    ownerId: form.ownerId
  })
}
</script>

<template>
  <form class="form-grid" @submit.prevent="submit">
    <label class="span-2">
      Название *
      <input v-model.trim="form.name" maxlength="200" required placeholder="Например, Исследование гибридного поиска" />
    </label>
    <label class="span-2">
      Описание
      <textarea v-model="form.description" rows="3" placeholder="Краткое описание проекта"></textarea>
    </label>
    <div class="form-actions span-2">
      <button class="btn btn-primary" type="submit" :disabled="submitting">{{ submitting ? 'Сохранение...' : (initial ? 'Сохранить' : 'Создать проект') }}</button>
      <button v-if="initial" class="btn btn-secondary" type="button" @click="$emit('cancel')">Отмена</button>
    </div>
  </form>
</template>
