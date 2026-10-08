import { mount } from '@vue/test-utils'
import { describe, expect, it } from 'vitest'
import LoadingIndicator from './LoadingIndicator.vue'

describe('LoadingIndicator', () => {
  it('использует текст по умолчанию', () => {
    const wrapper = mount(LoadingIndicator)
    expect(wrapper.get('[role="status"]').text()).toContain('Загрузка...')
  })

  it('принимает пользовательский текст', () => {
    const wrapper = mount(LoadingIndicator, { props: { text: 'Получение проектов...' } })
    expect(wrapper.text()).toContain('Получение проектов...')
  })
})
