<script setup lang="ts">
const props = defineProps<{
  page: number
  totalPages: number
  pageSize: number
}>()
const emit = defineEmits<{
  (e: 'update:page', value: number): void
  (e: 'update:pageSize', value: number): void
}>()
</script>

<template>
  <div class="pagination">
    <button class="btn btn-secondary" :disabled="props.page <= 1" @click="emit('update:page', props.page - 1)">← Назад</button>
    <span>Страница {{ props.page }} из {{ Math.max(props.totalPages, 1) }}</span>
    <button class="btn btn-secondary" :disabled="props.totalPages === 0 || props.page >= props.totalPages" @click="emit('update:page', props.page + 1)">Вперёд →</button>
    <label class="inline-field">
      На странице
      <select :value="props.pageSize" @change="emit('update:pageSize', Number(($event.target as HTMLSelectElement).value))">
        <option :value="5">5</option>
        <option :value="10">10</option>
        <option :value="20">20</option>
        <option :value="50">50</option>
      </select>
    </label>
  </div>
</template>
