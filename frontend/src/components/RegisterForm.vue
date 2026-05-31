<template>
  <div class="w-full max-w-md mx-auto p-6">
    <h2 class="text-2xl font-bold mb-6 text-center">アカウント登録</h2>

    <!-- Success Message -->
    <div
      v-if="successMessage"
      class="mb-4 p-4 bg-green-100 border border-green-400 text-green-700 rounded"
      role="alert"
    >
      {{ successMessage }}
    </div>

    <!-- Error Message -->
    <div
      v-if="errorMessage"
      class="mb-4 p-4 bg-red-100 border border-red-400 text-red-700 rounded"
      role="alert"
    >
      {{ errorMessage }}
    </div>

    <form @submit.prevent="handleSubmit" class="space-y-4">
      <!-- Email Field -->
      <div>
        <label for="email" class="block text-sm font-medium text-gray-700 mb-1">
          メールアドレス
        </label>
        <input
          id="email"
          v-model="formData.email"
          type="email"
          autocomplete="email"
          class="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          :class="{
            'border-red-500': emailError,
            'border-gray-300': !emailError,
          }"
          @blur="validateEmail"
          @input="validateEmail"
        />
        <p v-if="emailError" class="mt-1 text-sm text-red-600">
          {{ emailError }}
        </p>
      </div>

      <!-- Password Field -->
      <div>
        <label for="password" class="block text-sm font-medium text-gray-700 mb-1">
          パスワード
        </label>
        <input
          id="password"
          v-model="formData.password"
          type="password"
          autocomplete="new-password"
          class="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          :class="{
            'border-red-500': passwordError,
            'border-gray-300': !passwordError,
          }"
          @blur="validatePassword"
          @input="validatePassword"
        />
        <p v-if="passwordError" class="mt-1 text-sm text-red-600">
          {{ passwordError }}
        </p>

        <!-- Password Strength Indicator -->
        <div v-if="formData.password" class="mt-2">
          <div class="flex items-center justify-between mb-1">
            <span class="text-xs text-gray-600">パスワード強度:</span>
            <span
              class="text-xs font-medium"
              :class="{
                'text-red-600': passwordStrength === 'weak',
                'text-yellow-600': passwordStrength === 'medium',
                'text-green-600': passwordStrength === 'strong',
              }"
            >
              {{ passwordStrengthLabel }}
            </span>
          </div>
          <div class="w-full bg-gray-200 rounded-full h-2">
            <div
              class="h-2 rounded-full transition-all duration-300"
              :class="{
                'bg-red-500 w-1/3': passwordStrength === 'weak',
                'bg-yellow-500 w-2/3': passwordStrength === 'medium',
                'bg-green-500 w-full': passwordStrength === 'strong',
              }"
            ></div>
          </div>
          <ul class="mt-2 text-xs text-gray-600 space-y-1">
            <li :class="{ 'text-green-600': hasMinLength }">
              <span v-if="hasMinLength">✓</span>
              <span v-else>○</span>
              8文字以上
            </li>
            <li :class="{ 'text-green-600': hasUpperCase }">
              <span v-if="hasUpperCase">✓</span>
              <span v-else>○</span>
              大文字を含む
            </li>
            <li :class="{ 'text-green-600': hasLowerCase }">
              <span v-if="hasLowerCase">✓</span>
              <span v-else>○</span>
              小文字を含む
            </li>
            <li :class="{ 'text-green-600': hasNumber }">
              <span v-if="hasNumber">✓</span>
              <span v-else>○</span>
              数字を含む
            </li>
            <li :class="{ 'text-green-600': hasSpecialChar }">
              <span v-if="hasSpecialChar">✓</span>
              <span v-else>○</span>
              特殊文字を含む
            </li>
          </ul>
        </div>
      </div>

      <!-- Submit Button -->
      <button
        type="submit"
        :disabled="isSubmitting || !isFormValid"
        class="w-full py-2 px-4 bg-blue-600 text-white font-medium rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 disabled:bg-gray-400 disabled:cursor-not-allowed transition-colors"
      >
        {{ isSubmitting ? '登録中...' : '登録' }}
      </button>
    </form>

    <p class="mt-4 text-center text-sm text-gray-600">
      既にアカウントをお持ちですか？
      <router-link to="/login" class="text-blue-600 hover:text-blue-800 font-medium">
        ログイン
      </router-link>
    </p>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import type { ApiError } from '@/types/auth'

const router = useRouter()
const authStore = useAuthStore()

const formData = ref({
  email: '',
  password: '',
})

const emailError = ref('')
const passwordError = ref('')
const errorMessage = ref('')
const successMessage = ref('')
const isSubmitting = ref(false)

// Email validation
const validateEmail = () => {
  const email = formData.value.email.trim()
  
  if (!email) {
    emailError.value = 'メールアドレスは必須です'
    return false
  }
  
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  if (!emailRegex.test(email)) {
    emailError.value = '有効なメールアドレスを入力してください'
    return false
  }
  
  if (email.length > 255) {
    emailError.value = 'メールアドレスは255文字以下で入力してください'
    return false
  }
  
  emailError.value = ''
  return true
}

// Password validation
const hasMinLength = computed(() => formData.value.password.length >= 8)
const hasUpperCase = computed(() => /[A-Z]/.test(formData.value.password))
const hasLowerCase = computed(() => /[a-z]/.test(formData.value.password))
const hasNumber = computed(() => /\d/.test(formData.value.password))
const hasSpecialChar = computed(() => /[@$!%*?&]/.test(formData.value.password))

const passwordStrength = computed(() => {
  const checks = [
    hasMinLength.value,
    hasUpperCase.value,
    hasLowerCase.value,
    hasNumber.value,
    hasSpecialChar.value,
  ]
  const passedChecks = checks.filter(Boolean).length
  
  if (passedChecks <= 2) return 'weak'
  if (passedChecks <= 4) return 'medium'
  return 'strong'
})

const passwordStrengthLabel = computed(() => {
  switch (passwordStrength.value) {
    case 'weak':
      return '弱い'
    case 'medium':
      return '普通'
    case 'strong':
      return '強い'
    default:
      return ''
  }
})

const validatePassword = () => {
  const password = formData.value.password
  
  if (!password) {
    passwordError.value = 'パスワードは必須です'
    return false
  }
  
  if (!hasMinLength.value || !hasUpperCase.value || !hasLowerCase.value || !hasNumber.value || !hasSpecialChar.value) {
    passwordError.value = 'パスワードは8文字以上で、大文字、小文字、数字、特殊文字を含む必要があります'
    return false
  }
  
  passwordError.value = ''
  return true
}

const isFormValid = computed(() => {
  return (
    formData.value.email &&
    formData.value.password &&
    !emailError.value &&
    !passwordError.value &&
    hasMinLength.value &&
    hasUpperCase.value &&
    hasLowerCase.value &&
    hasNumber.value &&
    hasSpecialChar.value
  )
})

const handleSubmit = async () => {
  // Clear previous messages
  errorMessage.value = ''
  successMessage.value = ''
  
  // Validate all fields
  const isEmailValid = validateEmail()
  const isPasswordValid = validatePassword()
  
  if (!isEmailValid || !isPasswordValid) {
    return
  }
  
  isSubmitting.value = true
  
  try {
    await authStore.register({
      email: formData.value.email.trim(),
      password: formData.value.password,
    })
    
    successMessage.value = 'アカウントが正常に作成されました'
    
    // Clear form
    formData.value.email = ''
    formData.value.password = ''
    
    // Redirect to login page after 2 seconds
    setTimeout(() => {
      router.push('/login')
    }, 2000)
  } catch (error: any) {
    // The auth store already sets the error message
    if (authStore.error) {
      errorMessage.value = authStore.error
    } else if (error.response?.data) {
      const apiError = error.response.data as ApiError
      
      // Handle validation errors
      if (apiError.errors && apiError.errors.length > 0) {
        apiError.errors.forEach((err: any) => {
          if (err.field.toLowerCase() === 'email') {
            emailError.value = err.message
          } else if (err.field.toLowerCase() === 'password') {
            passwordError.value = err.message
          }
        })
      }
      
      // Handle specific error messages
      if (apiError.message) {
        errorMessage.value = apiError.message
      }
    } else {
      errorMessage.value = 'アカウントを作成できません。もう一度お試しください。'
    }
  } finally {
    isSubmitting.value = false
  }
}
</script>
