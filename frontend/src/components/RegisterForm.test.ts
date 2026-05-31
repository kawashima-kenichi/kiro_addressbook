import { describe, it, expect, beforeEach, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import RegisterForm from './RegisterForm.vue'
import { useAuthStore } from '@/stores/auth'

// Mock vue-router
vi.mock('vue-router', () => ({
  useRouter: () => ({
    push: vi.fn(),
  }),
  RouterLink: {
    name: 'RouterLink',
    template: '<a><slot /></a>',
  },
}))

describe('RegisterForm.vue', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('renders registration form with all fields', () => {
    const wrapper = mount(RegisterForm)
    
    expect(wrapper.find('h2').text()).toBe('アカウント登録')
    expect(wrapper.find('#email').exists()).toBe(true)
    expect(wrapper.find('#password').exists()).toBe(true)
    expect(wrapper.find('button[type="submit"]').exists()).toBe(true)
  })

  it('validates email field on input', async () => {
    const wrapper = mount(RegisterForm)
    const emailInput = wrapper.find('#email')
    
    // Test empty email
    await emailInput.setValue('')
    await emailInput.trigger('blur')
    expect(wrapper.text()).toContain('メールアドレスは必須です')
    
    // Test invalid email format
    await emailInput.setValue('invalid-email')
    await emailInput.trigger('blur')
    expect(wrapper.text()).toContain('有効なメールアドレスを入力してください')
    
    // Test valid email
    await emailInput.setValue('test@example.com')
    await emailInput.trigger('blur')
    expect(wrapper.text()).not.toContain('有効なメールアドレスを入力してください')
  })

  it('validates password field and shows strength indicator', async () => {
    const wrapper = mount(RegisterForm)
    const passwordInput = wrapper.find('#password')
    
    // Test weak password
    await passwordInput.setValue('weak')
    expect(wrapper.text()).toContain('パスワード強度:')
    expect(wrapper.text()).toContain('弱い')
    
    // Test medium password
    await passwordInput.setValue('Medium1')
    expect(wrapper.text()).toContain('普通')
    
    // Test strong password
    await passwordInput.setValue('Strong1@')
    expect(wrapper.text()).toContain('強い')
  })

  it('shows password requirements checklist', async () => {
    const wrapper = mount(RegisterForm)
    const passwordInput = wrapper.find('#password')
    
    await passwordInput.setValue('Test')
    
    expect(wrapper.text()).toContain('8文字以上')
    expect(wrapper.text()).toContain('大文字を含む')
    expect(wrapper.text()).toContain('小文字を含む')
    expect(wrapper.text()).toContain('数字を含む')
    expect(wrapper.text()).toContain('特殊文字を含む')
  })

  it('disables submit button when form is invalid', async () => {
    const wrapper = mount(RegisterForm)
    const submitButton = wrapper.find('button[type="submit"]')
    
    expect(submitButton.attributes('disabled')).toBeDefined()
    
    // Fill with invalid data
    await wrapper.find('#email').setValue('invalid')
    await wrapper.find('#password').setValue('weak')
    
    expect(submitButton.attributes('disabled')).toBeDefined()
  })

  it('enables submit button when form is valid', async () => {
    const wrapper = mount(RegisterForm)
    
    await wrapper.find('#email').setValue('test@example.com')
    await wrapper.find('#password').setValue('ValidPass1@')
    
    const submitButton = wrapper.find('button[type="submit"]')
    expect(submitButton.attributes('disabled')).toBeUndefined()
  })

  it('calls register on form submit with valid data', async () => {
    const wrapper = mount(RegisterForm)
    const authStore = useAuthStore()
    
    // Mock the register method
    authStore.register = vi.fn().mockResolvedValue(undefined)
    
    await wrapper.find('#email').setValue('test@example.com')
    await wrapper.find('#password').setValue('ValidPass1@')
    
    await wrapper.find('form').trigger('submit.prevent')
    
    expect(authStore.register).toHaveBeenCalledWith({
      email: 'test@example.com',
      password: 'ValidPass1@',
    })
  })

  it('displays success message after successful registration', async () => {
    const wrapper = mount(RegisterForm)
    const authStore = useAuthStore()
    
    authStore.register = vi.fn().mockResolvedValue(undefined)
    
    await wrapper.find('#email').setValue('test@example.com')
    await wrapper.find('#password').setValue('ValidPass1@')
    await wrapper.find('form').trigger('submit.prevent')
    
    // Wait for async operations
    await wrapper.vm.$nextTick()
    await new Promise((resolve) => setTimeout(resolve, 0))
    
    expect(wrapper.text()).toContain('アカウントが正常に作成されました')
  })

  it('displays error message when registration fails', async () => {
    const wrapper = mount(RegisterForm)
    const authStore = useAuthStore()
    
    authStore.register = vi.fn().mockRejectedValue({
      response: {
        data: {
          message: 'このメールアドレスは既に使用されています',
        },
      },
    })
    
    await wrapper.find('#email').setValue('test@example.com')
    await wrapper.find('#password').setValue('ValidPass1@')
    await wrapper.find('form').trigger('submit.prevent')
    
    // Wait for async operations
    await wrapper.vm.$nextTick()
    await new Promise((resolve) => setTimeout(resolve, 0))
    
    expect(wrapper.text()).toContain('このメールアドレスは既に使用されています')
  })

  it('displays validation errors from API', async () => {
    const wrapper = mount(RegisterForm)
    const authStore = useAuthStore()
    
    authStore.register = vi.fn().mockRejectedValue({
      response: {
        data: {
          message: 'Validation failed',
          errors: [
            { field: 'email', message: '有効なメールアドレスを入力してください' },
            { field: 'password', message: 'パスワードが弱すぎます' },
          ],
        },
      },
    })
    
    await wrapper.find('#email').setValue('test@example.com')
    await wrapper.find('#password').setValue('ValidPass1@')
    await wrapper.find('form').trigger('submit.prevent')
    
    // Wait for async operations
    await wrapper.vm.$nextTick()
    await new Promise((resolve) => setTimeout(resolve, 0))
    
    expect(wrapper.text()).toContain('有効なメールアドレスを入力してください')
    expect(wrapper.text()).toContain('パスワードが弱すぎます')
  })

  it('validates password meets all requirements', async () => {
    const wrapper = mount(RegisterForm)
    const passwordInput = wrapper.find('#password')
    
    // Password without uppercase
    await passwordInput.setValue('password1@')
    await passwordInput.trigger('blur')
    expect(wrapper.text()).toContain('パスワードは8文字以上で、大文字、小文字、数字、特殊文字を含む必要があります')
    
    // Password without lowercase
    await passwordInput.setValue('PASSWORD1@')
    await passwordInput.trigger('blur')
    expect(wrapper.text()).toContain('パスワードは8文字以上で、大文字、小文字、数字、特殊文字を含む必要があります')
    
    // Password without number
    await passwordInput.setValue('Password@')
    await passwordInput.trigger('blur')
    expect(wrapper.text()).toContain('パスワードは8文字以上で、大文字、小文字、数字、特殊文字を含む必要があります')
    
    // Password without special character
    await passwordInput.setValue('Password1')
    await passwordInput.trigger('blur')
    expect(wrapper.text()).toContain('パスワードは8文字以上で、大文字、小文字、数字、特殊文字を含む必要があります')
    
    // Valid password
    await passwordInput.setValue('Password1@')
    await passwordInput.trigger('blur')
    expect(wrapper.text()).not.toContain('パスワードは8文字以上で、大文字、小文字、数字、特殊文字を含む必要があります')
  })

  it('trims email before submission', async () => {
    const wrapper = mount(RegisterForm)
    const authStore = useAuthStore()
    
    authStore.register = vi.fn().mockResolvedValue(undefined)
    
    await wrapper.find('#email').setValue('  test@example.com  ')
    await wrapper.find('#password').setValue('ValidPass1@')
    await wrapper.find('form').trigger('submit.prevent')
    
    expect(authStore.register).toHaveBeenCalledWith({
      email: 'test@example.com',
      password: 'ValidPass1@',
    })
  })
})
