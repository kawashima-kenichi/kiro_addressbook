<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import ContactForm from '@/components/ContactForm.vue'
import { useAuthStore } from '@/stores/auth'
import { useContactStore } from '@/stores/contact'
import type { ContactDto } from '@/types/contact'

/**
 * ContactEditView
 * 連絡先編集ページ
 */

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const contactStore = useContactStore()

const contactId = route.params.id as string
const loading = ref(true)

onMounted(async () => {
  try {
    await contactStore.fetchContactById(contactId)
  } catch (error) {
    console.error('連絡先の読み込みに失敗しました:', error)
    router.push('/contacts')
  } finally {
    loading.value = false
  }
})

const handleSuccess = (contact: ContactDto) => {
  console.log('連絡先が更新されました:', contact)
  // ContactForm component handles redirect
}

const handleCancel = () => {
  console.log('編集がキャンセルされました')
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
          <h1 class="text-3xl font-bold text-gray-900">連絡先を編集</h1>
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

      <!-- Loading state -->
      <div v-if="loading" class="flex justify-center items-center py-12">
        <svg
          class="animate-spin h-8 w-8 text-indigo-600"
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
        <span class="ml-3 text-gray-600">読み込み中...</span>
      </div>

      <!-- 連絡先編集フォーム -->
      <ContactForm
        v-if="!loading && contactStore.currentContact"
        mode="edit"
        :contact="contactStore.currentContact"
        @success="handleSuccess"
        @cancel="handleCancel"
      />

      <!-- Error state -->
      <div
        v-if="!loading && !contactStore.currentContact"
        class="rounded-md bg-red-50 p-4"
      >
        <div class="flex">
          <div class="flex-shrink-0">
            <svg
              class="h-5 w-5 text-red-400"
              xmlns="http://www.w3.org/2000/svg"
              viewBox="0 0 20 20"
              fill="currentColor"
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
              連絡先が見つかりません。
            </p>
          </div>
        </div>
      </div>
    </main>
  </div>
</template>
