import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { createRouter, createMemoryHistory } from 'vue-router'
import AuthGuard from './AuthGuard.vue'
import { useAuthStore } from '@/stores/auth'

// Mock router
const router = createRouter({
  history: createMemoryHistory(),
  routes: [
    { path: '/', name: 'home', component: { template: '<div>Home</div>' } },
    { path: '/login', name: 'login', component: { template: '<div>Login</div>' } },
  ],
})

describe('AuthGuard.vue', () => {
  beforeEach(() => {
    // Create a fresh pinia instance for each test
    setActivePinia(createPinia())
    vi.useFakeTimers()
  })

  afterEach(() => {
    vi.restoreAllMocks()
    vi.useRealTimers()
  })

  it('認証済みユーザーにはスロットコンテンツを表示する', () => {
    const authStore = useAuthStore()
    
    // Mock authenticated state
    authStore.token = 'valid-token'
    authStore.user = { id: '1', email: 'test@example.com' }
    authStore.expiresAt = new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString()

    const wrapper = mount(AuthGuard, {
      global: {
        plugins: [router],
      },
      slots: {
        default: '<div data-testid="protected-content">Protected Content</div>',
      },
    })

    expect(wrapper.find('[data-testid="protected-content"]').exists()).toBe(true)
    expect(wrapper.text()).toContain('Protected Content')
  })

  it('未認証ユーザーにはローディングメッセージを表示する', () => {
    const authStore = useAuthStore()
    
    // Mock unauthenticated state
    authStore.token = null
    authStore.user = null
    authStore.expiresAt = null

    const wrapper = mount(AuthGuard, {
      global: {
        plugins: [router],
      },
      slots: {
        default: '<div data-testid="protected-content">Protected Content</div>',
      },
    })

    expect(wrapper.find('[data-testid="protected-content"]').exists()).toBe(false)
    expect(wrapper.text()).toContain('認証を確認しています...')
  })

  it('セッション期限切れ時にログインページにリダイレクトする', async () => {
    const authStore = useAuthStore()
    
    // Mock authenticated state with expired token
    authStore.token = 'expired-token'
    authStore.user = { id: '1', email: 'test@example.com' }
    authStore.expiresAt = new Date(Date.now() - 1000).toISOString() // Expired 1 second ago

    // Spy on router.push
    const pushSpy = vi.spyOn(router, 'push')

    // Spy on authStore.logout
    const logoutSpy = vi.spyOn(authStore, 'logout').mockResolvedValue()

    const wrapper = mount(AuthGuard, {
      global: {
        plugins: [router],
      },
      slots: {
        default: '<div data-testid="protected-content">Protected Content</div>',
      },
    })

    // Wait for the component to mount and check session
    await wrapper.vm.$nextTick()

    // Verify logout was called
    expect(logoutSpy).toHaveBeenCalled()

    // Wait for logout to complete
    await vi.runAllTimersAsync()

    // Verify redirect to login with expired query
    expect(pushSpy).toHaveBeenCalledWith({
      name: 'login',
      query: { expired: 'true' },
    })
  })

  it('定期的にセッション期限切れをチェックする', async () => {
    const authStore = useAuthStore()
    
    // Mock authenticated state
    authStore.token = 'valid-token'
    authStore.user = { id: '1', email: 'test@example.com' }
    authStore.expiresAt = new Date(Date.now() + 60 * 1000).toISOString() // Expires in 60 seconds

    // Spy on authStore.isTokenExpired
    const isTokenExpiredSpy = vi.spyOn(authStore, 'isTokenExpired')

    const wrapper = mount(AuthGuard, {
      global: {
        plugins: [router],
      },
      slots: {
        default: '<div data-testid="protected-content">Protected Content</div>',
      },
    })

    // Wait for initial mount
    await wrapper.vm.$nextTick()

    // Clear the initial call
    isTokenExpiredSpy.mockClear()

    // Advance time by 30 seconds (one check interval)
    await vi.advanceTimersByTimeAsync(30 * 1000)

    // Verify that isTokenExpired was called during the interval check
    expect(isTokenExpiredSpy).toHaveBeenCalled()
  })

  it('コンポーネントアンマウント時にタイマーをクリアする', async () => {
    const authStore = useAuthStore()
    
    // Mock authenticated state
    authStore.token = 'valid-token'
    authStore.user = { id: '1', email: 'test@example.com' }
    authStore.expiresAt = new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString()

    const wrapper = mount(AuthGuard, {
      global: {
        plugins: [router],
      },
      slots: {
        default: '<div data-testid="protected-content">Protected Content</div>',
      },
    })

    // Spy on clearInterval
    const clearIntervalSpy = vi.spyOn(global, 'clearInterval')

    // Unmount the component
    wrapper.unmount()

    // Verify that clearInterval was called
    expect(clearIntervalSpy).toHaveBeenCalled()
  })
})
