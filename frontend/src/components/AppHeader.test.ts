import { describe, it, expect, beforeEach, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import AppHeader from './AppHeader.vue'
import { useAuthStore } from '@/stores/auth'

// Mock vue-router
const mockPush = vi.fn()
vi.mock('vue-router', () => ({
  useRouter: () => ({
    push: mockPush,
  }),
}))

describe('AppHeader.vue', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    mockPush.mockClear()
  })

  describe('未認証状態', () => {
    it('ログイン/新規登録リンクが表示される', () => {
      const wrapper = mount(AppHeader, {
        global: {
          stubs: {
            RouterLink: {
              template: '<a><slot /></a>',
              props: ['to'],
            },
          },
        },
      })

      expect(wrapper.text()).toContain('ログイン')
      expect(wrapper.text()).toContain('新規登録')
    })

    it('ユーザー情報が表示されない', () => {
      const wrapper = mount(AppHeader, {
        global: {
          stubs: {
            RouterLink: {
              template: '<a><slot /></a>',
              props: ['to'],
            },
          },
        },
      })

      expect(wrapper.text()).not.toContain('ログアウト')
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
      // Set expiration to future date
      authStore.expiresAt = new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString()
    })

    it('ユーザーメールアドレスが表示される', () => {
      const wrapper = mount(AppHeader, {
        global: {
          stubs: {
            RouterLink: {
              template: '<a><slot /></a>',
              props: ['to'],
            },
          },
        },
      })

      expect(wrapper.text()).toContain('test@example.com')
    })

    it('ログアウトボタンが表示される', () => {
      const wrapper = mount(AppHeader, {
        global: {
          stubs: {
            RouterLink: {
              template: '<a><slot /></a>',
              props: ['to'],
            },
          },
        },
      })

      expect(wrapper.text()).toContain('ログアウト')
    })

    it('ログイン/新規登録リンクが表示されない', () => {
      const wrapper = mount(AppHeader, {
        global: {
          stubs: {
            RouterLink: {
              template: '<a><slot /></a>',
              props: ['to'],
            },
          },
        },
      })

      const text = wrapper.text()
      // ログアウトボタンのテキストは含まれるが、ログインリンクは含まれない
      expect(text).toContain('ログアウト')
      // ログインとログアウトを区別するため、ログインリンクの存在を確認
      const loginLinks = wrapper.findAll('a').filter((link) => link.text() === 'ログイン')
      expect(loginLinks.length).toBe(0)
    })
  })

  describe('ロゴクリック', () => {
    it('未認証時にロゴをクリックするとホームに遷移する', async () => {
      const wrapper = mount(AppHeader, {
        global: {
          stubs: {
            RouterLink: {
              template: '<a><slot /></a>',
              props: ['to'],
            },
          },
        },
      })

      await wrapper.find('button[aria-label="ホームに戻る"]').trigger('click')
      expect(mockPush).toHaveBeenCalledWith('/')
    })

    it('認証済み時にロゴをクリックすると連絡先一覧に遷移する', async () => {
      const authStore = useAuthStore()
      authStore.user = {
        id: '1',
        email: 'test@example.com',
      }
      authStore.token = 'test-token'
      authStore.expiresAt = new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString()

      const wrapper = mount(AppHeader, {
        global: {
          stubs: {
            RouterLink: {
              template: '<a><slot /></a>',
              props: ['to'],
            },
          },
        },
      })

      await wrapper.find('button[aria-label="ホームに戻る"]').trigger('click')
      expect(mockPush).toHaveBeenCalledWith('/contacts')
    })
  })

  describe('ログアウト', () => {
    it('ログアウトボタンをクリックするとログアウト処理が実行される', async () => {
      const authStore = useAuthStore()
      authStore.user = {
        id: '1',
        email: 'test@example.com',
      }
      authStore.token = 'test-token'
      authStore.expiresAt = new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString()

      const wrapper = mount(AppHeader, {
        global: {
          stubs: {
            RouterLink: {
              template: '<a><slot /></a>',
              props: ['to'],
            },
          },
        },
      })

      await wrapper.find('button[aria-label="ログアウト"]').trigger('click')
      
      // Wait for logout to complete
      await new Promise(resolve => setTimeout(resolve, 100))

      // ログアウト後、ストアがクリアされる
      expect(authStore.user).toBeNull()
      expect(authStore.token).toBeNull()

      // ログインページに遷移する
      expect(mockPush).toHaveBeenCalledWith('/login')
    })
  })

  describe('アクセシビリティ', () => {
    it('role="banner"属性が設定されている', () => {
      const wrapper = mount(AppHeader, {
        global: {
          stubs: {
            RouterLink: {
              template: '<a><slot /></a>',
              props: ['to'],
            },
          },
        },
      })

      expect(wrapper.find('header').attributes('role')).toBe('banner')
    })

    it('ロゴボタンにaria-label属性が設定されている', () => {
      const wrapper = mount(AppHeader, {
        global: {
          stubs: {
            RouterLink: {
              template: '<a><slot /></a>',
              props: ['to'],
            },
          },
        },
      })

      expect(wrapper.find('button').attributes('aria-label')).toBe('ホームに戻る')
    })

    it('認証済み時にログアウトボタンにaria-label属性が設定されている', () => {
      const authStore = useAuthStore()
      authStore.user = {
        id: '1',
        email: 'test@example.com',
      }
      authStore.token = 'test-token'
      authStore.expiresAt = new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString()

      const wrapper = mount(AppHeader, {
        global: {
          stubs: {
            RouterLink: {
              template: '<a><slot /></a>',
              props: ['to'],
            },
          },
        },
      })

      expect(wrapper.find('button[aria-label="ログアウト"]').exists()).toBe(true)
    })

    it('未認証時にナビゲーションにaria-label属性が設定されている', () => {
      const wrapper = mount(AppHeader, {
        global: {
          stubs: {
            RouterLink: {
              template: '<a><slot /></a>',
              props: ['to'],
            },
          },
        },
      })

      expect(wrapper.find('nav').attributes('aria-label')).toBe('認証ナビゲーション')
    })
  })

  describe('レスポンシブデザイン', () => {
    it('モバイルでユーザーメールが非表示になる', () => {
      const authStore = useAuthStore()
      authStore.user = {
        id: '1',
        email: 'test@example.com',
      }
      authStore.token = 'test-token'
      authStore.expiresAt = new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString()

      const wrapper = mount(AppHeader, {
        global: {
          stubs: {
            RouterLink: {
              template: '<a><slot /></a>',
              props: ['to'],
            },
          },
        },
      })

      const emailSpan = wrapper.find('span[aria-label="ログイン中のユーザー"]')
      expect(emailSpan.classes()).toContain('hidden')
      expect(emailSpan.classes()).toContain('sm:block')
    })

    it('モバイルでログアウトボタンのテキストが非表示になる', () => {
      const authStore = useAuthStore()
      authStore.user = {
        id: '1',
        email: 'test@example.com',
      }
      authStore.token = 'test-token'
      authStore.expiresAt = new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString()

      const wrapper = mount(AppHeader, {
        global: {
          stubs: {
            RouterLink: {
              template: '<a><slot /></a>',
              props: ['to'],
            },
          },
        },
      })

      const logoutButton = wrapper.find('button[aria-label="ログアウト"]')
      const logoutText = logoutButton.find('span')
      expect(logoutText.classes()).toContain('hidden')
      expect(logoutText.classes()).toContain('sm:inline')
    })
  })
})
