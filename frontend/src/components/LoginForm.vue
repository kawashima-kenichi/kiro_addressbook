<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()

// Form data
const email = ref('')
const password = ref('')

// Validation errors
const emailError = ref('')
const passwordError = ref('')

// Session expiration message
const sessionExpiredMessage = ref('')

// Computed
const isFormValid = computed(() => {
  return email.value.trim() !== '' && password.value.trim() !== ''
})

// Check for session expiration on mount
onMounted(() => {
  if (route.query.expired === 'true') {
    sessionExpiredMessage.value = 'セッションが期限切れになりました。再度ログインしてください。'
  }
})

// Validate email
const validateEmail = () => {
  emailError.value = ''
  
  if (!email.value.trim()) {
    emailError.value = 'メールアドレスは必須です'
    return false
  }
  
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  if (!emailRegex.test(email.value)) {
    emailError.value = '有効なメールアドレスを入力してください'
    return false
  }
  
  return true
}

// Validate password
const validatePassword = () => {
  passwordError.value = ''
  
  if (!password.value.trim()) {
    passwordError.value = 'パスワードは必須です'
    return false
  }
  
  return true
}

// Handle form submission
const handleSubmit = async () => {
  // Clear previous errors and messages
  authStore.clearError()
  emailError.value = ''
  passwordError.value = ''
  sessionExpiredMessage.value = ''
  
  // Validate fields
  const isEmailValid = validateEmail()
  const isPasswordValid = validatePassword()
  
  if (!isEmailValid || !isPasswordValid) {
    return
  }
  
  try {
    await authStore.login({
      email: email.value,
      password: password.value,
    })
    
    // Redirect to the original destination or home page
    const redirectPath = (route.query.redirect as string) || '/'
    router.push(redirectPath)
  } catch (error) {
    // Error is handled in the store
    console.error('Login failed:', error)
  }
}

// Handle input blur events for validation
const handleEmailBlur = () => {
  if (email.value.trim()) {
    validateEmail()
  }
}

const handlePasswordBlur = () => {
  if (password.value.trim()) {
    validatePassword()
  }
}
</script>

<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
    <div class="max-w-md w-full space-y-8">
      <div>
        <h2 class="mt-6 text-center text-3xl font-extrabold text-gray-900">
          ログイン
        </h2>
        <p class="mt-2 text-center text-sm text-gray-600">
          住所録WEBアプリケーション
        </p>
      </div>
      
      <form class="mt-8 space-y-6" @submit.prevent="handleSubmit">
        <!-- Session expired message -->
        <div
          v-if="sessionExpiredMessage"
          class="rounded-md bg-yellow-50 p-4"
          role="alert"
        >
          <div class="flex">
            <div class="flex-shrink-0">
              <svg
                class="h-5 w-5 text-yellow-400"
                xmlns="http://www.w3.org/2000/svg"
                viewBox="0 0 20 20"
                fill="currentColor"
                aria-hidden="true"
              >
                <path
                  fill-rule="evenodd"
                  d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z"
                  clip-rule="evenodd"
                />
              </svg>
            </div>
            <div class="ml-3">
              <p class="text-sm font-medium text-yellow-800">
                {{ sessionExpiredMessage }}
              </p>
            </div>
          </div>
        </div>

        <!-- Global error message -->
        <div
          v-if="authStore.error"
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
                {{ authStore.error }}
              </p>
            </div>
          </div>
        </div>

        <div class="rounded-md shadow-sm space-y-4">
          <!-- Email field -->
          <div>
            <label for="email" class="block text-sm font-medium text-gray-700 mb-1">
              メールアドレス
            </label>
            <input
              id="email"
              v-model="email"
              type="email"
              name="email"
              autocomplete="email"
              required
              data-testid="email-input"
              class="appearance-none relative block w-full px-3 py-2 border rounded-md placeholder-gray-400 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 focus:z-10 sm:text-sm"
              :class="{
                'border-red-300 text-red-900 focus:ring-red-500 focus:border-red-500': emailError,
                'border-gray-300': !emailError,
              }"
              placeholder="example@example.com"
              @blur="handleEmailBlur"
            />
            <p
              v-if="emailError"
              class="mt-1 text-sm text-red-600"
              data-testid="email-error"
            >
              {{ emailError }}
            </p>
          </div>

          <!-- Password field -->
          <div>
            <label for="password" class="block text-sm font-medium text-gray-700 mb-1">
              パスワード
            </label>
            <input
              id="password"
              v-model="password"
              type="password"
              name="password"
              autocomplete="current-password"
              required
              data-testid="password-input"
              class="appearance-none relative block w-full px-3 py-2 border rounded-md placeholder-gray-400 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 focus:z-10 sm:text-sm"
              :class="{
                'border-red-300 text-red-900 focus:ring-red-500 focus:border-red-500': passwordError,
                'border-gray-300': !passwordError,
              }"
              placeholder="パスワードを入力"
              @blur="handlePasswordBlur"
            />
            <p
              v-if="passwordError"
              class="mt-1 text-sm text-red-600"
              data-testid="password-error"
            >
              {{ passwordError }}
            </p>
          </div>
        </div>

        <div>
          <button
            type="submit"
            data-testid="login-button"
            :disabled="!isFormValid || authStore.loading"
            class="group relative w-full flex justify-center py-2 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            <span v-if="authStore.loading" class="flex items-center">
              <svg
                class="animate-spin -ml-1 mr-3 h-5 w-5 text-white"
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
              ログイン中...
            </span>
            <span v-else>ログイン</span>
          </button>
        </div>
      </form>
    </div>
  </div>
</template>
