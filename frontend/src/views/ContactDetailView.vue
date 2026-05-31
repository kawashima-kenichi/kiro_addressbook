<script setup lang="ts">
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import ContactDetail from '@/components/ContactDetail.vue'
import BaseModal from '@/components/BaseModal.vue'
import { useAuthStore } from '@/stores/auth'
import { useContactStore } from '@/stores/contact'

/**
 * ContactDetailView
 * 連絡先詳細ページ
 */

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const contactStore = useContactStore()

const contactId = route.params.id as string

// 削除確認モーダル
const showDeleteModal = ref(false)
const deleteError = ref<string | null>(null)

const handleLogout = async () => {
  await authStore.logout()
  router.push('/login')
}

const handleUpdated = () => {
  // 更新後の処理（必要に応じて）
  console.log('連絡先が更新されました')
}

const handleCancelled = () => {
  // キャンセル後の処理（必要に応じて一覧に戻る）
  console.log('編集がキャンセルされました')
}

const goBack = () => {
  router.push('/')
}

// 削除ボタンをクリック
const handleDeleteClick = () => {
  showDeleteModal.value = true
  deleteError.value = null
}

// 削除を確認
const handleDeleteConfirm = async () => {
  try {
    await contactStore.deleteContact(contactId)
    showDeleteModal.value = false
    // 削除成功後、一覧ページに戻る
    router.push('/')
  } catch (error) {
    deleteError.value = '連絡先を削除できません。もう一度お試しください。'
  }
}

// 削除をキャンセル
const handleDeleteCancel = () => {
  showDeleteModal.value = false
  deleteError.value = null
}

// 連絡先名を取得
const getContactName = () => {
  return contactStore.currentContact?.name || '連絡先'
}

</script>

<template>
  <div class="min-h-screen bg-gray-50">
    <!-- Header -->
    <header class="bg-white shadow">
      <div class="mx-auto max-w-7xl px-4 py-6 sm:px-6 lg:px-8">
        <div class="flex items-center justify-between">
          <h1 class="text-3xl font-bold text-gray-900">連絡先詳細</h1>
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
      <!-- エラーメッセージ -->
      <div
        v-if="deleteError"
        class="mb-4 rounded-md bg-red-50 p-4"
        role="alert"
      >
        <div class="flex">
          <div class="flex-shrink-0">
            <svg
              class="h-5 w-5 text-red-400"
              viewBox="0 0 20 20"
              fill="currentColor"
            >
              <path
                fill-rule="evenodd"
                d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.28 7.22a.75.75 0 00-1.06 1.06L8.94 10l-1.72 1.72a.75.75 0 101.06 1.06L10 11.06l1.72 1.72a.75.75 0 101.06-1.06L11.06 10l1.72-1.72a.75.75 0 00-1.06-1.06L10 8.94 8.28 7.22z"
                clip-rule="evenodd"
              />
            </svg>
          </div>
          <div class="ml-3">
            <p class="text-sm font-medium text-red-800">
              {{ deleteError }}
            </p>
          </div>
        </div>
      </div>

      <!-- 戻るボタンと削除ボタン -->
      <div class="mb-4 flex items-center justify-between">
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
        
        <button
          @click="handleDeleteClick"
          class="inline-flex items-center rounded-md bg-red-600 px-4 py-2 text-sm font-medium text-white hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2"
          data-testid="delete-button"
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
              d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
            />
          </svg>
          削除
        </button>
      </div>

      <!-- 連絡先詳細コンポーネント -->
      <ContactDetail
        :contact-id="contactId"
        @updated="handleUpdated"
        @cancelled="handleCancelled"
      />
    </main>

    <!-- 削除確認モーダル -->
    <BaseModal
      :is-open="showDeleteModal"
      title="連絡先を削除"
      :message="`「${getContactName()}」を削除してもよろしいですか？この操作は元に戻せません。`"
      confirm-text="削除"
      cancel-text="キャンセル"
      @confirm="handleDeleteConfirm"
      @cancel="handleDeleteCancel"
      @close="handleDeleteCancel"
    />
  </div>
</template>
