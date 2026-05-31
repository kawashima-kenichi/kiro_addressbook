<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useContactStore } from '@/stores/contact'
import type { ContactDto } from '@/types/contact'

// Props
interface Props {
  contact?: ContactDto | null
  mode?: 'create' | 'edit'
}

const props = withDefaults(defineProps<Props>(), {
  contact: null,
  mode: 'create',
})

// Emits
const emit = defineEmits<{
  success: [contact: ContactDto]
  cancel: []
}>()

const router = useRouter()
const contactStore = useContactStore()

// Form data
const name = ref('')
const address = ref('')
const phoneNumber = ref('')

// Validation errors
const nameError = ref('')
const addressError = ref('')
const phoneNumberError = ref('')

// Success message
const successMessage = ref('')

// Computed
const isFormValid = computed(() => {
  return name.value.trim() !== '' && !nameError.value && !addressError.value && !phoneNumberError.value
})

const isEditMode = computed(() => props.mode === 'edit')

const formTitle = computed(() => isEditMode.value ? '連絡先を編集' : '新しい連絡先を追加')

const submitButtonText = computed(() => isEditMode.value ? '更新' : '追加')

// Initialize form with contact data if in edit mode
onMounted(() => {
  if (props.contact) {
    name.value = props.contact.name
    address.value = props.contact.address || ''
    phoneNumber.value = props.contact.phoneNumber || ''
  }
})

// Watch for contact prop changes (for edit mode)
watch(() => props.contact, (newContact) => {
  if (newContact) {
    name.value = newContact.name
    address.value = newContact.address || ''
    phoneNumber.value = newContact.phoneNumber || ''
  }
}, { immediate: true })

// Validate name
const validateName = (): boolean => {
  nameError.value = ''
  
  if (!name.value.trim()) {
    nameError.value = '名前は必須です'
    return false
  }
  
  if (name.value.length < 1 || name.value.length > 100) {
    nameError.value = '名前は1文字以上100文字以下で入力してください'
    return false
  }
  
  // Check for leading/trailing whitespace
  if (name.value !== name.value.trim()) {
    nameError.value = '名前の前後に空白は使用できません'
    return false
  }
  
  return true
}

// Validate address
const validateAddress = (): boolean => {
  addressError.value = ''
  
  if (address.value && address.value.length > 500) {
    addressError.value = '住所は500文字以下で入力してください'
    return false
  }
  
  return true
}

// Validate phone number
const validatePhoneNumber = (): boolean => {
  phoneNumberError.value = ''
  
  if (!phoneNumber.value) {
    return true // Phone number is optional
  }
  
  if (phoneNumber.value.length > 20) {
    phoneNumberError.value = '電話番号は20文字以下で入力してください'
    return false
  }
  
  // Phone number format validation
  // Accepts: (123) 456-7890, 123-456-7890, 123.456.7890, +1-123-456-7890, 1234567890
  const phoneRegex = /^(\(\d{3}\)\s?\d{3}-\d{4}|\d{3}-\d{3}-\d{4}|\d{3}\.\d{3}\.\d{4}|\+\d{1}-\d{3}-\d{3}-\d{4}|\d{10})$/
  
  if (!phoneRegex.test(phoneNumber.value)) {
    phoneNumberError.value = '有効な電話番号を入力してください'
    return false
  }
  
  return true
}

// Validate all fields
const validateForm = (): boolean => {
  const isNameValid = validateName()
  const isAddressValid = validateAddress()
  const isPhoneNumberValid = validatePhoneNumber()
  
  return isNameValid && isAddressValid && isPhoneNumberValid
}

// Handle form submission
const handleSubmit = async () => {
  // Clear previous errors and messages
  contactStore.clearError()
  successMessage.value = ''
  
  // Validate all fields
  if (!validateForm()) {
    return
  }
  
  try {
    const contactData = {
      name: name.value.trim(),
      address: address.value.trim() || undefined,
      phoneNumber: phoneNumber.value.trim() || undefined,
    }
    
    let savedContact: ContactDto
    
    if (isEditMode.value && props.contact) {
      // Update existing contact
      savedContact = await contactStore.updateContact(props.contact.id, contactData)
      successMessage.value = `連絡先「${savedContact.name}」が正常に更新されました`
    } else {
      // Create new contact
      savedContact = await contactStore.createContact(contactData)
      successMessage.value = `連絡先「${savedContact.name}」が正常に追加されました`
    }
    
    // Emit success event
    emit('success', savedContact)
    
    // Redirect to contact list after a short delay
    setTimeout(() => {
      router.push('/contacts')
    }, 1500)
  } catch (error) {
    // Handle validation errors from backend
    if (contactStore.validationErrors) {
      if (contactStore.validationErrors.name) {
        nameError.value = contactStore.validationErrors.name
      }
      if (contactStore.validationErrors.address) {
        addressError.value = contactStore.validationErrors.address
      }
      if (contactStore.validationErrors.phoneNumber) {
        phoneNumberError.value = contactStore.validationErrors.phoneNumber
      }
    }
    console.error('Failed to save contact:', error)
  }
}

// Handle cancel
const handleCancel = () => {
  emit('cancel')
  router.push('/contacts')
}

// Handle input blur events for validation
const handleNameBlur = () => {
  if (name.value.trim()) {
    validateName()
  }
}

const handleAddressBlur = () => {
  if (address.value.trim()) {
    validateAddress()
  }
}

const handlePhoneNumberBlur = () => {
  if (phoneNumber.value.trim()) {
    validatePhoneNumber()
  }
}
</script>

<template>
  <div class="min-h-screen bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
    <div class="max-w-2xl mx-auto">
      <div class="bg-white shadow-md rounded-lg p-6">
        <div class="mb-6">
          <h2 class="text-2xl font-bold text-gray-900">
            {{ formTitle }}
          </h2>
          <p class="mt-1 text-sm text-gray-600">
            {{ isEditMode ? '連絡先情報を更新します' : '新しい連絡先を追加します' }}
          </p>
        </div>
        
        <form @submit.prevent="handleSubmit" class="space-y-6">
          <!-- Success message -->
          <div
            v-if="successMessage"
            class="rounded-md bg-green-50 p-4"
            role="alert"
          >
            <div class="flex">
              <div class="flex-shrink-0">
                <svg
                  class="h-5 w-5 text-green-400"
                  xmlns="http://www.w3.org/2000/svg"
                  viewBox="0 0 20 20"
                  fill="currentColor"
                  aria-hidden="true"
                >
                  <path
                    fill-rule="evenodd"
                    d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z"
                    clip-rule="evenodd"
                  />
                </svg>
              </div>
              <div class="ml-3">
                <p class="text-sm font-medium text-green-800">
                  {{ successMessage }}
                </p>
              </div>
            </div>
          </div>

          <!-- Global error message -->
          <div
            v-if="contactStore.error"
            class="rounded-md bg-red-50 p-4"
            role="alert"
          >
            <div class="flex">
              <div class="flex-shrink-0">
                <svg
                  class="h-5 w-5 text-red-400"
                  xmlns="http://www.w3.org/2000/svg"
                  viewBox="0 0 20 20"
                  fill="currentColor"
                  aria-hidden="true"
                >
                  <path
                    fill-rule="evenodd"
                    d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
                    clip-rule="evenodd"
                  />
                </svg>
              </div>
              <div class="ml-3">
                <p class="text-sm font-medium text-red-800">
                  {{ contactStore.error }}
                </p>
              </div>
            </div>
          </div>

          <!-- Name field (required) -->
          <div>
            <label for="name" class="block text-sm font-medium text-gray-700 mb-1">
              名前 <span class="text-red-500">*</span>
            </label>
            <input
              id="name"
              v-model="name"
              type="text"
              name="name"
              required
              maxlength="100"
              data-testid="contact-name"
              class="appearance-none block w-full px-3 py-2 border rounded-md placeholder-gray-400 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
              :class="{
                'border-red-300 text-red-900 focus:ring-red-500 focus:border-red-500': nameError,
                'border-gray-300': !nameError,
              }"
              placeholder="田中太郎"
              @blur="handleNameBlur"
            />
            <p
              v-if="nameError"
              class="mt-1 text-sm text-red-600"
              data-testid="name-error"
            >
              {{ nameError }}
            </p>
            <p class="mt-1 text-xs text-gray-500">
              1文字以上100文字以下で入力してください
            </p>
          </div>

          <!-- Address field (optional) -->
          <div>
            <label for="address" class="block text-sm font-medium text-gray-700 mb-1">
              住所
            </label>
            <textarea
              id="address"
              v-model="address"
              name="address"
              rows="3"
              maxlength="500"
              data-testid="contact-address"
              class="appearance-none block w-full px-3 py-2 border rounded-md placeholder-gray-400 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
              :class="{
                'border-red-300 text-red-900 focus:ring-red-500 focus:border-red-500': addressError,
                'border-gray-300': !addressError,
              }"
              placeholder="東京都渋谷区..."
              @blur="handleAddressBlur"
            ></textarea>
            <p
              v-if="addressError"
              class="mt-1 text-sm text-red-600"
              data-testid="address-error"
            >
              {{ addressError }}
            </p>
            <p class="mt-1 text-xs text-gray-500">
              最大500文字まで入力できます（任意）
            </p>
          </div>

          <!-- Phone number field (optional) -->
          <div>
            <label for="phoneNumber" class="block text-sm font-medium text-gray-700 mb-1">
              電話番号
            </label>
            <input
              id="phoneNumber"
              v-model="phoneNumber"
              type="tel"
              name="phoneNumber"
              maxlength="20"
              data-testid="contact-phone"
              class="appearance-none block w-full px-3 py-2 border rounded-md placeholder-gray-400 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
              :class="{
                'border-red-300 text-red-900 focus:ring-red-500 focus:border-red-500': phoneNumberError,
                'border-gray-300': !phoneNumberError,
              }"
              placeholder="03-1234-5678"
              @blur="handlePhoneNumberBlur"
            />
            <p
              v-if="phoneNumberError"
              class="mt-1 text-sm text-red-600"
              data-testid="phone-error"
            >
              {{ phoneNumberError }}
            </p>
            <p class="mt-1 text-xs text-gray-500">
              形式: (123) 456-7890, 123-456-7890, 123.456.7890, +1-123-456-7890, 1234567890（任意）
            </p>
          </div>

          <!-- Form actions -->
          <div class="flex justify-end space-x-3 pt-4">
            <button
              type="button"
              data-testid="cancel-button"
              @click="handleCancel"
              class="px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
            >
              キャンセル
            </button>
            <button
              type="submit"
              data-testid="save-button"
              :disabled="!isFormValid || contactStore.loading"
              class="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <span v-if="contactStore.loading" class="flex items-center">
                <svg
                  class="animate-spin -ml-1 mr-2 h-4 w-4 text-white"
                  xmlns="http://www.w3.org/2000/svg"
                  fill="none"
                  viewBox="0 0 24 24"
                >
                  <circle
                    class="opacity-25"
                    cx="12"
                    cy="12"
                    r="10"
                    stroke="currentColor"
                    stroke-width="4"
                  ></circle>
                  <path
                    class="opacity-75"
                    fill="currentColor"
                    d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                  ></path>
                </svg>
                {{ isEditMode ? '更新中...' : '追加中...' }}
              </span>
              <span v-else>{{ submitButtonText }}</span>
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>
