import { mount } from '@vue/test-utils'
import { describe, expect, it } from 'vitest'
import ErrorMessage from './ErrorMessage.vue'

describe('ErrorMessage', () => {
  it('показывает сообщение API', () => {
    const wrapper = mount(ErrorMessage, { props: { message: 'Ошибка PostgreSQL' } })
    expect(wrapper.get('[role="alert"]').text()).toContain('Ошибка PostgreSQL')
  })

  it('не создаёт alert без сообщения', () => {
    const wrapper = mount(ErrorMessage)
    expect(wrapper.find('[role="alert"]').exists()).toBe(false)
  })
})
