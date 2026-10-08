<script setup lang="ts">
withDefaults(defineProps<{ text?: string }>(), { text: 'Загрузка...' })
</script>

<template>
  <div class="loading" role="status">
    <span class="spinner" aria-hidden="true"></span>
    <span>{{ text }}</span>
  </div>
</template>
