import { describe, it, expect, beforeEach, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { createRouter, createWebHistory } from 'vue-router'
import LoginForm from './LoginForm.vue'
import { useAuthStore } from '@/stores/auth'

// Mock authService
vi.mock('@/services/authService', () => ({
  authService: {
    login: vi.fn(),
    register: vi.fn(),
    logout: vi.fn(),
  },
}))

// Create a mock router
const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', name: 'home', component: { template: '<div>Home</div>' } },
    { path: '/login', name: 'login', component: { template: '<div>Login</div>' } },
  ],
})

describe('LoginForm.vue', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    localStorage.clear()
  })

  it('renders login form with email and password fields', () => {
    const wrapper = mount(LoginForm, {
      global: {
        plugins: [router],
      },
    })

    expect(wrapper.find('[data-testid="email-input"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="password-input"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="login-button"]').exists()).toBe(true)
  })

  it('displays validation error for invalid email format', async () => {
    const wrapper = mount(LoginForm, {
      global: {
        plugins: [router],
      },
    })

    const emailInput = wrapper.find('[data-testid="email-input"]')
    await emailInput.setValue('invalid-email')
    await emailInput.trigger('blur')

    expect(wrapper.find('[data-testid="email-error"]').text()).toBe(
      '有効なメールアドレスを入力してください'
    )
  })

  it('displays validation error for empty email', async () => {
    const wrapper = mount(LoginForm, {
      global: {
        plugins: [router],
      },
    })

    const emailInput = wrapper.find('[data-testid="email-input"]')
    await emailInput.setValue('')
    await emailInput.trigger('blur')

    // Submit form to trigger validation
    await wrapper.find('form').trigger('submit.prevent')

    expect(wrapper.find('[data-testid="email-error"]').text()).toBe(
      'メールアドレスは必須です'
    )
  })

  it('displays validation error for empty password', async () => {
    const wrapper = mount(LoginForm, {
      global: {
        plugins: [router],
      },
    })

    const passwordInput = wrapper.find('[data-testid="password-input"]')
    await passwordInput.setValue('')
    await passwordInput.trigger('blur')

    // Submit form to trigger validation
    await wrapper.find('form').trigger('submit.prevent')

    expect(wrapper.find('[data-testid="password-error"]').text()).toBe(
      'パスワードは必須です'
    )
  })

  it('disables submit button when form is invalid', async () => {
    const wrapper = mount(LoginForm, {
      global: {
        plugins: [router],
      },
    })

    const submitButton = wrapper.find('[data-testid="login-button"]')
    expect(submitButton.attributes('disabled')).toBeDefined()
  })

  it('enables submit button when form is valid', async () => {
    const wrapper = mount(LoginForm, {
      global: {
        plugins: [router],
      },
    })

    await wrapper.find('[data-testid="email-input"]').setValue('test@example.com')
    await wrapper.find('[data-testid="password-input"]').setValue('password123')

    const submitButton = wrapper.find('[data-testid="login-button"]')
    expect(submitButton.attributes('disabled')).toBeUndefined()
  })

  it('calls authStore.login when form is submitted with valid data', async () => {
    const wrapper = mount(LoginForm, {
      global: {
        plugins: [router],
      },
    })
    const authStore = useAuthStore()
    
    // Mock the login method
    authStore.login = vi.fn().mockResolvedValue({})

    await wrapper.find('[data-testid="email-input"]').setValue('test@example.com')
    await wrapper.find('[data-testid="password-input"]').setValue('password123')
    await wrapper.find('form').trigger('submit.prevent')

    expect(authStore.login).toHaveBeenCalledWith({
      email: 'test@example.com',
      password: 'password123',
    })
  })

  it('displays error message from authStore', async () => {
    const wrapper = mount(LoginForm, {
      global: {
        plugins: [router],
      },
    })
    const authStore = useAuthStore()
    
    authStore.error = 'ユーザー名またはパスワードが無効です'
    await wrapper.vm.$nextTick()

    expect(wrapper.text()).toContain('ユーザー名またはパスワードが無効です')
  })

  it('displays account locked error message', async () => {
    const wrapper = mount(LoginForm, {
      global: {
        plugins: [router],
      },
    })
    const authStore = useAuthStore()
    
    authStore.error = 'アカウントがロックされています。30分後に再試行してください。'
    await wrapper.vm.$nextTick()

    expect(wrapper.text()).toContain('アカウントがロックされています')
  })

  it('shows loading state when loading is true', async () => {
    const wrapper = mount(LoginForm, {
      global: {
        plugins: [router],
      },
    })
    const authStore = useAuthStore()
    
    authStore.loading = true
    await wrapper.vm.$nextTick()

    expect(wrapper.find('[data-testid="login-button"]').text()).toContain('ログイン中')
  })
})
