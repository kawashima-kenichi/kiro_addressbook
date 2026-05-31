<script setup lang="ts">
import { useRouter } from 'vue-router'
import ContactForm from '@/components/ContactForm.vue'
import { useAuthStore } from '@/stores/auth'
import type { ContactDto } from '@/types/contact'

/**
 * ContactCreateView
 * 連絡先作成ページ
 */

const router = useRouter()
const authStore = useAuthStore()

const handleSuccess = (contact: ContactDto) => {
  console.log('連絡先が作成されました:', contact)
  // ContactForm component handles redirect
}

const handleCancel = () => {
  console.log('作成がキャンセルされました')
  // ContactForm component handles redirect
}

const handleLogout = async () => {
  await authStore.logout()
  router.push('/login')
}

const goBack = () => {
  router.push('/contacts')
}
</script>

<template>
  <div class="min-h-screen bg-gray-50">
    <!-- Header -->
    <header class="bg-white shadow">
      <div class="mx-auto max-w-7xl px-4 py-6 sm:px-6 lg:px-8">
        <div class="flex items-center justify-between">
          <h1 class="text-3xl font-bold text-gray-900">新しい連絡先を追加</h1>
          <div class="flex items-center space-x-4">
            <span class="text-sm text-gray-600">
              {{ authStore.user?.email }}
            </span>
            <button
              @click="handleLogout"
              class="rounded-md bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
            >
              ログアウト
            </button>
          </div>
        </div>
      </div>
    </header>

    <!-- Main content -->
    <main class="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      <!-- 戻るボタン -->
      <div class="mb-4">
        <button
          @click="goBack"
          class="inline-flex items-center text-sm font-medium text-indigo-600 hover:text-indigo-500"
        >
          <svg
            class="mr-2 h-5 w-5"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M10 19l-7-7m0 0l7-7m-7 7h18"
            />
          </svg>
          連絡先一覧に戻る
        </button>
      </div>

      <!-- 連絡先作成フォーム -->
      <ContactForm
        mode="create"
        @success="handleSuccess"
        @cancel="handleCancel"
      />
    </main>
  </div>
</template>
