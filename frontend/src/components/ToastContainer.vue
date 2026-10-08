<script setup lang="ts">
import { useToastStore } from '../stores/toast'
const toast = useToastStore()
</script>

<template>
  <div class="toast-stack" aria-live="polite">
    <button v-for="item in toast.items" :key="item.id" class="toast" :data-kind="item.kind" @click="toast.remove(item.id)">
      {{ item.text }}
    </button>
  </div>
</template>
