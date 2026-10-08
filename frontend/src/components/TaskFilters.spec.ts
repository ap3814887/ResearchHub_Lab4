import { mount } from '@vue/test-utils'
import { describe, expect, it } from 'vitest'
import TaskFilters from './TaskFilters.vue'

describe('TaskFilters', () => {
  const props = {
    status: '' as const,
    priority: '' as const,
    assigneeId: null,
    search: '',
    sort: 'updated-desc',
    members: [{ projectId: 1, userId: 2, userName: 'Мария', email: 'maria@example.test', role: 'Member', joinedAt: '2026-01-01T00:00:00Z' }]
  }

  it('показывает участника в фильтре исполнителя', () => {
    const wrapper = mount(TaskFilters, { props })
    expect(wrapper.text()).toContain('Мария')
  })

  it('эмитит reset', async () => {
    const wrapper = mount(TaskFilters, { props })
    await wrapper.get('button').trigger('click')
    expect(wrapper.emitted('reset')).toHaveLength(1)
  })
})
