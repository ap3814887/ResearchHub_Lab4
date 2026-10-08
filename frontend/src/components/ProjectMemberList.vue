<script setup lang="ts">
import { reactive } from 'vue'
import type { ProjectMember, User } from '../types'

defineProps<{
  members: ProjectMember[]
  users: User[]
  loading?: boolean
  canEdit?: boolean
}>()

const emit = defineEmits<{
  (e: 'add', value: { userId: number; role: string }): void
  (e: 'remove', member: ProjectMember): void
}>()

const form = reactive({ userId: 0, role: 'Member' })

function add() {
  if (form.userId < 1) return
  emit('add', { userId: form.userId, role: form.role })
}
</script>

<template>
  <section class="card">
    <div class="section-heading">
      <div>
        <div class="eyebrow">Команда</div>
        <h2>Участники проекта</h2>
      </div>
      <span class="badge">{{ members.length }}</span>
    </div>

    <form v-if="canEdit" class="member-add" @submit.prevent="add">
      <select
        v-model.number="form.userId"
        :disabled="loading"
        required
      >
        <option :value="0" disabled>Выберите пользователя</option>
        <option v-for="u in users" :key="u.id" :value="u.id">{{ u.name }} — {{ u.email }}</option>
      </select>
      <select v-model="form.role" :disabled="loading">
        <option value="Member">Member</option>
      </select>
      <button
        class="btn btn-primary"
        type="submit"
        :disabled="loading || form.userId < 1"
      >
        {{ loading ? 'Добавление...' : 'Добавить' }}
      </button>
    </form>

    <p v-if="!members.length" class="empty-state">Участников пока нет.</p>
    <div v-else class="member-list">
      <div v-for="m in members" :key="`${m.projectId}-${m.userId}`" class="member-row">
        <div>
          <strong>{{ m.userName || `Пользователь ${m.userId}` }}</strong>
          <div class="muted small">{{ m.email || 'email не указан' }}</div>
        </div>
        <span class="badge">{{ m.role }}</span>
        <button v-if="canEdit && m.role !== 'Owner'" class="btn btn-danger ghost" @click="$emit('remove', m)">Удалить</button>
      </div>
    </div>
  </section>
</template>
