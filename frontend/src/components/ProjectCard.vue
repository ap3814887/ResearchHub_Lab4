<script setup lang="ts">
import type { Project } from '../types'

defineProps<{ project: Project; ownerName?: string; canManage?: boolean }>()
defineEmits<{ (e: 'delete', project: Project): void }>()
</script>

<template>
  <article class="card project-card">
    <div>
      <div class="project-card-kicker">
        <span class="eyebrow">#{{ project.id }}</span>
        <span v-if="canManage" class="owner-tag">Мой проект</span>
      </div>
      <h3>{{ project.name }}</h3>
      <p class="muted clamp">{{ project.description || 'Описание не задано' }}</p>
    </div>
    <dl class="compact-meta">
      <div><dt>Владелец</dt><dd>{{ ownerName || `ID ${project.ownerId}` }}</dd></div>
      <div><dt>Создан</dt><dd>{{ new Date(project.createdAt).toLocaleString('ru-RU') }}</dd></div>
    </dl>
    <div class="card-actions">
      <RouterLink class="btn btn-primary" :to="`/projects/${project.id}`">Открыть</RouterLink>
      <RouterLink class="btn btn-secondary" :to="`/projects/${project.id}/tasks`">Задачи</RouterLink>
      <button v-if="canManage" class="btn btn-danger ghost" @click="$emit('delete', project)">Удалить</button>
    </div>
  </article>
</template>
