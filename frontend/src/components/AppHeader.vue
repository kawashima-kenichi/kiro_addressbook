<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const isAuthenticated = computed(() => authStore.isAuthenticated)
const userEmail = computed(() => authStore.user?.email)

const handleLogout = async () => {
  await authStore.logout()
  router.push('/login')
}

const handleLogoClick = () => {
  if (isAuthenticated.value) {
    router.push('/contacts')
  } else {
    router.push('/')
  }
}
</script>

<template>
  <header class="bg-white shadow-sm" role="banner">
    <div class="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
      <div class="flex h-16 items-center justify-between">
        <!-- Logo / Brand -->
        <div class="flex items-center">
          <button
            type="button"
            class="text-xl font-bold text-indigo-600 hover:text-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 rounded-md px-2 py-1 transition-colors"
            @click="handleLogoClick"
            aria-label="ホームに戻る"
          >
            住所録アプリ
          </button>
        </div>

        <!-- User info and actions -->
        <div v-if="isAuthenticated" class="flex items-center gap-4">
          <!-- User email -->
          <span class="hidden sm:block text-sm text-gray-700" aria-label="ログイン中のユーザー">
            {{ userEmail }}
          </span>

          <!-- Logout button -->
          <button
            type="button"
            class="inline-flex items-center px-3 py-2 text-sm font-medium text-gray-700 hover:text-gray-900 hover:bg-gray-100 rounded-md focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 transition-colors"
            @click="handleLogout"
            aria-label="ログアウト"
          >
            <svg
              class="h-5 w-5 mr-1"
              fill="none"
              viewBox="0 0 24 24"
              stroke-width="1.5"
              stroke="currentColor"
              aria-hidden="true"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                d="M15.75 9V5.25A2.25 2.25 0 0013.5 3h-6a2.25 2.25 0 00-2.25 2.25v13.5A2.25 2.25 0 007.5 21h6a2.25 2.25 0 002.25-2.25V15M12 9l-3 3m0 0l3 3m-3-3h12.75"
              />
            </svg>
            <span class="hidden sm:inline">ログアウト</span>
          </button>
        </div>

        <!-- Login/Register links for unauthenticated users -->
        <nav v-else class="flex items-center gap-4" aria-label="認証ナビゲーション">
          <router-link
            to="/login"
            class="text-sm font-medium text-gray-700 hover:text-indigo-600 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 rounded-md px-2 py-1 transition-colors"
          >
            ログイン
          </router-link>
          <router-link
            to="/register"
            class="inline-flex items-center px-3 py-2 text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 rounded-md focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 transition-colors"
          >
            新規登録
          </router-link>
        </nav>
      </div>
    </div>
  </header>
</template>
