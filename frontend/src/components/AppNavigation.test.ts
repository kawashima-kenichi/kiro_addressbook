import { describe, it, expect, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import AppNavigation from './AppNavigation.vue'
import { useAuthStore } from '@/stores/auth'

// Mock vue-router
const mockRoute = {
  path: '/contacts',
}

vi.mock('vue-router', () => ({
  useRoute: () => mockRoute,
}))

describe('AppNavigation.vue', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    mockRoute.path = '/contacts'
  })

  describe('未認証状態', () => {
    it('ナビゲーションが表示されない', () => {
      const wrapper = mount(AppNavigation, {
        global: {
          stubs: {
            RouterLink: {
              template: '<a><slot /></a>',
              props: ['to'],
            },
          },
        },
      })

      expect(wrapper.find('nav').exists()).toBe(false)
    })
  })

  describe('認証済み状態', () => {
    beforeEach(() => {
      const authStore = useAuthStore()
      authStore.user = {
        id: '1',
        email: 'test@example.com',
      }
      authStore.token = 'test-token'
      authStore.expiresAt = new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString()
    })

    it('ナビゲーションが表示される', () => {
      const wrapper = mount(AppNavigation, {
        global: {
          stubs: {
            RouterLink: {
              template: '<a><slot /></a>',
              props: ['to'],
            },
          },
        },
      })

      expect(wrapper.find('nav').exists()).toBe(true)
    })

    it('連絡先一覧リンクが表示される', () => {
      const wrapper = mount(AppNavigation, {
        global: {
          stubs: {
            RouterLink: {
              template: '<a><slot /></a>',
              props: ['to'],
            },
          },
        },
      })

      expect(wrapper.text()).toContain('連絡先一覧')
    })

    it('現在のページがアクティブ状態になる', () => {
      mockRoute.path = '/contacts'

      const wrapper = mount(AppNavigation, {
        global: {
          stubs: {
            RouterLink: {
              template: '<a :class="$attrs.class"><slot /></a>',
              props: ['to'],
            },
          },
        },
      })

      const activeLink = wrapper.find('a')
      expect(activeLink.classes()).toContain('text-indigo-600')
      expect(activeLink.classes()).toContain('bg-indigo-50')
    })

    it('非アクティブページのスタイルが適用される', () => {
      mockRoute.path = '/other'

      const wrapper = mount(AppNavigation, {
        global: {
          stubs: {
            RouterLink: {
              template: '<a :class="$attrs.class"><slot /></a>',
              props: ['to'],
            },
          },
        },
      })

      const link = wrapper.find('a')
      expect(link.classes()).toContain('text-gray-700')
      expect(link.classes()).not.toContain('text-indigo-600')
    })
  })

  describe('アクセシビリティ', () => {
    beforeEach(() => {
      const authStore = useAuthStore()
      authStore.user = {
        id: '1',
        email: 'test@example.com',
      }
      authStore.token = 'test-token'
      authStore.expiresAt = new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString()
    })

    it('nav要素にaria-label属性が設定されている', () => {
      const wrapper = mount(AppNavigation, {
        global: {
          stubs: {
            RouterLink: {
              template: '<a><slot /></a>',
              props: ['to'],
            },
          },
        },
      })

      expect(wrapper.find('nav').attributes('aria-label')).toBe('メインナビゲーション')
    })

    it('アクティブページにaria-current属性が設定される', () => {
      mockRoute.path = '/contacts'

      const wrapper = mount(AppNavigation, {
        global: {
          stubs: {
            RouterLink: {
              template: '<a :aria-current="$attrs[\'aria-current\']"><slot /></a>',
              props: ['to'],
            },
          },
        },
      })

      const activeLink = wrapper.find('a')
      expect(activeLink.attributes('aria-current')).toBe('page')
    })

    it('各リンクにaria-label属性が設定されている', () => {
      const wrapper = mount(AppNavigation, {
        global: {
          stubs: {
            RouterLink: {
              template: '<a :aria-label="$attrs[\'aria-label\']"><slot /></a>',
              props: ['to'],
            },
          },
        },
      })

      const link = wrapper.find('a')
      expect(link.attributes('aria-label')).toBe('連絡先一覧ページへ移動')
    })

    it('アイコンにaria-hidden属性が設定されている', () => {
      const wrapper = mount(AppNavigation, {
        global: {
          stubs: {
            RouterLink: {
              template: '<a><slot /></a>',
              props: ['to'],
            },
          },
        },
      })

      const icon = wrapper.find('svg')
      expect(icon.attributes('aria-hidden')).toBe('true')
    })
  })

  describe('レスポンシブデザイン', () => {
    beforeEach(() => {
      const authStore = useAuthStore()
      authStore.user = {
        id: '1',
        email: 'test@example.com',
      }
      authStore.token = 'test-token'
      authStore.expiresAt = new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString()
    })

    it('レスポンシブなパディングクラスが適用されている', () => {
      const wrapper = mount(AppNavigation, {
        global: {
          stubs: {
            RouterLink: {
              template: '<a><slot /></a>',
              props: ['to'],
            },
          },
        },
      })

      const container = wrapper.find('.mx-auto')
      expect(container.classes()).toContain('px-4')
      expect(container.classes()).toContain('sm:px-6')
      expect(container.classes()).toContain('lg:px-8')
    })
  })
})
