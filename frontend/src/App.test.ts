import { describe, it, expect, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { createRouter, createMemoryHistory } from 'vue-router'
import App from './App.vue'

describe('App', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('renders router-view', () => {
    const router = createRouter({
      history: createMemoryHistory(),
      routes: [
        { path: '/', component: { template: '<div>Home</div>' } },
      ],
    })

    const wrapper = mount(App, {
      global: {
        plugins: [router],
        stubs: ['router-view'],
      },
    })
    expect(wrapper.find('router-view-stub').exists()).toBe(true)
  })
})
