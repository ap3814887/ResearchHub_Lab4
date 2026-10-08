<script setup lang="ts">
defineProps<{ message?: string }>()
</script>

<template>
  <div v-if="message" class="alert alert-error" role="alert">{{ message }}</div>
</template>
