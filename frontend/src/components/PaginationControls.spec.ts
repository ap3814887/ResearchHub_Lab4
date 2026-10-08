import { mount } from '@vue/test-utils'
import { describe, expect, it } from 'vitest'
import PaginationControls from './PaginationControls.vue'

describe('PaginationControls', () => {
  it('блокирует Назад на первой странице', () => {
    const wrapper = mount(PaginationControls, { props: { page: 1, totalPages: 3, pageSize: 10 } })
    expect(wrapper.findAll('button')[0].attributes('disabled')).toBeDefined()
  })

  it('эмитит переход на следующую страницу', async () => {
    const wrapper = mount(PaginationControls, { props: { page: 1, totalPages: 3, pageSize: 10 } })
    await wrapper.findAll('button')[1].trigger('click')
    expect(wrapper.emitted('update:page')?.[0]).toEqual([2])
  })
})
