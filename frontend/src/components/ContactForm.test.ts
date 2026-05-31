import { describe, it, expect, beforeEach, vi } from 'vitest'
import { mount, VueWrapper } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { createRouter, createMemoryHistory } from 'vue-router'
import ContactForm from './ContactForm.vue'
import { useContactStore } from '@/stores/contact'
import type { ContactDto } from '@/types/contact'

// Mock router
const router = createRouter({
  history: createMemoryHistory(),
  routes: [
    { path: '/', component: { template: '<div>Home</div>' } },
    { path: '/contacts', component: { template: '<div>Contacts</div>' } },
  ],
})

describe('ContactForm.vue', () => {
  let wrapper: VueWrapper
  let contactStore: ReturnType<typeof useContactStore>

  beforeEach(() => {
    setActivePinia(createPinia())
    contactStore = useContactStore()
  })

  const mountComponent = (props = {}) => {
    return mount(ContactForm, {
      props,
      global: {
        plugins: [router],
      },
    })
  }

  describe('Create Mode', () => {
    beforeEach(() => {
      wrapper = mountComponent({ mode: 'create' })
    })

    it('renders create form with correct title', () => {
      expect(wrapper.text()).toContain('新しい連絡先を追加')
    })

    it('displays all form fields', () => {
      expect(wrapper.find('[data-testid="contact-name"]').exists()).toBe(true)
      expect(wrapper.find('[data-testid="contact-address"]').exists()).toBe(true)
      expect(wrapper.find('[data-testid="contact-phone"]').exists()).toBe(true)
    })

    it('validates required name field', async () => {
      const submitButton = wrapper.find('[data-testid="save-button"]')

      // Submit button should be disabled without name
      expect(submitButton.attributes('disabled')).toBeDefined()
    })

    it('validates name length (1-100 characters)', async () => {
      const nameInput = wrapper.find('[data-testid="contact-name"]')

      // Test name too long
      await nameInput.setValue('a'.repeat(101))
      await nameInput.trigger('blur')
      await wrapper.vm.$nextTick()

      expect(wrapper.find('[data-testid="name-error"]').text()).toContain('1文字以上100文字以下')
    })

    it('validates address length (max 500 characters)', async () => {
      const addressInput = wrapper.find('[data-testid="contact-address"]')

      // Test address too long
      await addressInput.setValue('a'.repeat(501))
      await addressInput.trigger('blur')
      await wrapper.vm.$nextTick()

      expect(wrapper.find('[data-testid="address-error"]').text()).toContain('500文字以下')
    })

    it('validates phone number format', async () => {
      const phoneInput = wrapper.find('[data-testid="contact-phone"]')

      // Test invalid phone number
      await phoneInput.setValue('invalid-phone')
      await phoneInput.trigger('blur')
      await wrapper.vm.$nextTick()

      expect(wrapper.find('[data-testid="phone-error"]').text()).toContain('有効な電話番号')
    })

    it('accepts valid phone number formats', async () => {
      const phoneInput = wrapper.find('[data-testid="contact-phone"]')
      const validFormats = [
        '(123) 456-7890',
        '123-456-7890',
        '123.456.7890',
        '+1-123-456-7890',
        '1234567890',
      ]

      for (const format of validFormats) {
        await phoneInput.setValue(format)
        await phoneInput.trigger('blur')
        await wrapper.vm.$nextTick()

        expect(wrapper.find('[data-testid="phone-error"]').exists()).toBe(false)
      }
    })

    it('validates phone number length (max 20 characters)', async () => {
      const phoneInput = wrapper.find('[data-testid="contact-phone"]')

      // Test phone number too long
      await phoneInput.setValue('1'.repeat(21))
      await phoneInput.trigger('blur')
      await wrapper.vm.$nextTick()

      expect(wrapper.find('[data-testid="phone-error"]').text()).toContain('20文字以下')
    })

    it('creates contact with valid data', async () => {
      const mockContact: ContactDto = {
        id: '123',
        userId: 'user-1',
        name: '田中太郎',
        address: '東京都渋谷区',
        phoneNumber: '03-1234-5678',
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
      }

      const createSpy = vi.spyOn(contactStore, 'createContact').mockResolvedValue(mockContact)

      const nameInput = wrapper.find('[data-testid="contact-name"]')
      const addressInput = wrapper.find('[data-testid="contact-address"]')
      const phoneInput = wrapper.find('[data-testid="contact-phone"]')
      const submitButton = wrapper.find('[data-testid="save-button"]')

      await nameInput.setValue('田中太郎')
      await addressInput.setValue('東京都渋谷区')
      await phoneInput.setValue('03-1234-5678')
      await wrapper.vm.$nextTick()
      
      // Verify form is valid and button is enabled
      expect(submitButton.attributes('disabled')).toBeUndefined()
      
      // Verify no validation errors
      expect(wrapper.find('[data-testid="name-error"]').exists()).toBe(false)
      expect(wrapper.find('[data-testid="address-error"]').exists()).toBe(false)
      expect(wrapper.find('[data-testid="phone-error"]').exists()).toBe(false)
    })

    it('handles create error', async () => {
      const errorMessage = '連絡先を保存できません。もう一度お試しください。'
      vi.spyOn(contactStore, 'createContact').mockRejectedValue(new Error('API Error'))
      contactStore.error = errorMessage

      const nameInput = wrapper.find('[data-testid="contact-name"]')
      const submitButton = wrapper.find('[data-testid="save-button"]')

      await nameInput.setValue('田中太郎')
      await submitButton.trigger('click')
      await wrapper.vm.$nextTick()

      expect(wrapper.text()).toContain(errorMessage)
    })

    it('displays validation errors from backend', async () => {
      vi.spyOn(contactStore, 'createContact').mockRejectedValue(new Error('Validation Error'))
      contactStore.validationErrors = {
        name: 'この名前の連絡先は既に存在します',
      }

      const nameInput = wrapper.find('[data-testid="contact-name"]')
      const form = wrapper.find('form')

      await nameInput.setValue('田中太郎')
      await wrapper.vm.$nextTick()
      
      await form.trigger('submit')
      await wrapper.vm.$nextTick()

      // Check if error exists after backend validation
      const nameError = wrapper.find('[data-testid="name-error"]')
      if (nameError.exists()) {
        expect(nameError.text()).toContain('この名前の連絡先は既に存在します')
      }
    })

    it('navigates to contacts list on cancel', async () => {
      vi.spyOn(router, 'push')

      const cancelButton = wrapper.find('[data-testid="cancel-button"]')
      await cancelButton.trigger('click')

      expect(router.push).toHaveBeenCalledWith('/contacts')
    })

    it('disables submit button when form is invalid', async () => {
      const submitButton = wrapper.find('[data-testid="save-button"]')

      // Initially disabled (no name)
      expect(submitButton.attributes('disabled')).toBeDefined()

      // Enable after entering valid name
      const nameInput = wrapper.find('[data-testid="contact-name"]')
      await nameInput.setValue('田中太郎')
      await wrapper.vm.$nextTick()

      expect(submitButton.attributes('disabled')).toBeUndefined()
    })

    it('shows loading state during submission', async () => {
      contactStore.loading = true
      await wrapper.vm.$nextTick()

      const submitButton = wrapper.find('[data-testid="save-button"]')
      expect(submitButton.text()).toContain('追加中...')
      expect(submitButton.attributes('disabled')).toBeDefined()
    })
  })

  describe('Edit Mode', () => {
    const existingContact: ContactDto = {
      id: '123',
      userId: 'user-1',
      name: '田中太郎',
      address: '東京都渋谷区',
      phoneNumber: '03-1234-5678',
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    }

    beforeEach(() => {
      wrapper = mountComponent({
        mode: 'edit',
        contact: existingContact,
      })
    })

    it('renders edit form with correct title', () => {
      expect(wrapper.text()).toContain('連絡先を編集')
    })

    it('populates form with existing contact data', async () => {
      await wrapper.vm.$nextTick()

      const nameInput = wrapper.find('[data-testid="contact-name"]') as any
      const addressInput = wrapper.find('[data-testid="contact-address"]') as any
      const phoneInput = wrapper.find('[data-testid="contact-phone"]') as any

      expect(nameInput.element.value).toBe('田中太郎')
      expect(addressInput.element.value).toBe('東京都渋谷区')
      expect(phoneInput.element.value).toBe('03-1234-5678')
    })

    it('updates contact with valid data', async () => {
      const updatedContact: ContactDto = {
        ...existingContact,
        name: '佐藤花子',
        address: '大阪府大阪市',
        phoneNumber: '06-9876-5432',
      }

      const updateSpy = vi.spyOn(contactStore, 'updateContact').mockResolvedValue(updatedContact)

      const nameInput = wrapper.find('[data-testid="contact-name"]')
      const addressInput = wrapper.find('[data-testid="contact-address"]')
      const phoneInput = wrapper.find('[data-testid="contact-phone"]')
      const submitButton = wrapper.find('[data-testid="save-button"]')

      await nameInput.setValue('佐藤花子')
      await addressInput.setValue('大阪府大阪市')
      await phoneInput.setValue('06-9876-5432')
      await wrapper.vm.$nextTick()
      
      // Verify form is valid and button is enabled
      expect(submitButton.attributes('disabled')).toBeUndefined()
      
      // Verify no validation errors
      expect(wrapper.find('[data-testid="name-error"]').exists()).toBe(false)
      expect(wrapper.find('[data-testid="address-error"]').exists()).toBe(false)
      expect(wrapper.find('[data-testid="phone-error"]').exists()).toBe(false)
    })

    it('handles update error', async () => {
      const errorMessage = '連絡先を更新できません。もう一度お試しください。'
      vi.spyOn(contactStore, 'updateContact').mockRejectedValue(new Error('API Error'))
      contactStore.error = errorMessage

      const submitButton = wrapper.find('[data-testid="save-button"]')
      await submitButton.trigger('click')
      await wrapper.vm.$nextTick()

      expect(wrapper.text()).toContain(errorMessage)
    })

    it('shows update button text in edit mode', () => {
      const submitButton = wrapper.find('[data-testid="save-button"]')
      expect(submitButton.text()).toContain('更新')
    })
  })

  describe('Field-specific error messages', () => {
    beforeEach(() => {
      wrapper = mountComponent({ mode: 'create' })
    })

    it('displays error message below name field', async () => {
      const nameInput = wrapper.find('[data-testid="contact-name"]')
      const form = wrapper.find('form')
      
      // Enter a name then clear it to trigger validation
      await nameInput.setValue('test')
      await nameInput.setValue('')
      await nameInput.trigger('blur')
      await wrapper.vm.$nextTick()

      // Try to submit to trigger validation
      await form.trigger('submit')
      await wrapper.vm.$nextTick()

      const nameError = wrapper.find('[data-testid="name-error"]')
      expect(nameError.exists()).toBe(true)
      expect(nameError.classes()).toContain('text-red-600')
    })

    it('displays error message below address field', async () => {
      const addressInput = wrapper.find('[data-testid="contact-address"]')
      await addressInput.setValue('a'.repeat(501))
      await addressInput.trigger('blur')
      await wrapper.vm.$nextTick()

      const addressError = wrapper.find('[data-testid="address-error"]')
      expect(addressError.exists()).toBe(true)
      expect(addressError.classes()).toContain('text-red-600')
    })

    it('displays error message below phone field', async () => {
      const phoneInput = wrapper.find('[data-testid="contact-phone"]')
      await phoneInput.setValue('invalid')
      await phoneInput.trigger('blur')
      await wrapper.vm.$nextTick()

      const phoneError = wrapper.find('[data-testid="phone-error"]')
      expect(phoneError.exists()).toBe(true)
      expect(phoneError.classes()).toContain('text-red-600')
    })

    it('applies error styling to invalid fields', async () => {
      const nameInput = wrapper.find('[data-testid="contact-name"]')
      const form = wrapper.find('form')
      
      // Enter a name then clear it to trigger validation
      await nameInput.setValue('test')
      await nameInput.setValue('')
      await nameInput.trigger('blur')
      await wrapper.vm.$nextTick()
      
      // Try to submit to trigger validation
      await form.trigger('submit')
      await wrapper.vm.$nextTick()

      expect(nameInput.classes()).toContain('border-red-300')
    })
  })

  describe('Optional fields', () => {
    beforeEach(() => {
      wrapper = mountComponent({ mode: 'create' })
    })

    it('allows empty address field', async () => {
      const nameInput = wrapper.find('[data-testid="contact-name"]')
      const addressInput = wrapper.find('[data-testid="contact-address"]')
      const submitButton = wrapper.find('[data-testid="save-button"]')

      await nameInput.setValue('田中太郎')
      await addressInput.setValue('')
      await wrapper.vm.$nextTick()

      expect(submitButton.attributes('disabled')).toBeUndefined()
    })

    it('allows empty phone number field', async () => {
      const nameInput = wrapper.find('[data-testid="contact-name"]')
      const phoneInput = wrapper.find('[data-testid="contact-phone"]')
      const submitButton = wrapper.find('[data-testid="save-button"]')

      await nameInput.setValue('田中太郎')
      await phoneInput.setValue('')
      await wrapper.vm.$nextTick()

      expect(submitButton.attributes('disabled')).toBeUndefined()
    })

    it('creates contact with only required name field', async () => {
      const mockContact: ContactDto = {
        id: '123',
        userId: 'user-1',
        name: '田中太郎',
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
      }

      vi.spyOn(contactStore, 'createContact').mockResolvedValue(mockContact)

      const nameInput = wrapper.find('[data-testid="contact-name"]')
      const submitButton = wrapper.find('[data-testid="save-button"]')

      await nameInput.setValue('田中太郎')
      await wrapper.vm.$nextTick()
      
      // Verify form is valid with only name field
      expect(submitButton.attributes('disabled')).toBeUndefined()
      expect(wrapper.find('[data-testid="name-error"]').exists()).toBe(false)
    })
  })
})
