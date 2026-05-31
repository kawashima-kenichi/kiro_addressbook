<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useContactStore } from '@/stores/contact'
import { useAuthStore } from '@/stores/auth'
import ContactCard from '@/components/ContactCard.vue'
import BasePagination from '@/components/BasePagination.vue'

const router = useRouter()
const contactStore = useContactStore()
const authStore = useAuthStore()

const successMessage = ref('')
const errorMessage = ref('')

// 成功メッセージを表示
const showSuccessMessage = (message: string) => {
  successMessage.value = message
  errorMessage.value = ''
  
  // 5秒後にメッセージを自動的に消す
  setTimeout(() => {
    successMessage.value = ''
  }, 5000)
}

// エラーメッセージを表示
const showErrorMessage = (message: string) => {
  errorMessage.value = message
  successMessage.value = ''
  
  // 5秒後にメッセージを自動的に消す
  setTimeout(() => {
    errorMessage.value = ''
  }, 5000)
}

// 連絡先を削除
const handleDelete = async (id: string) => {
  try {
    // 削除する連絡先の名前を取得
    const contact = contactStore.contacts.find((c) => c.id === id)
    const contactName = contact?.name || '連絡先'
    
    await contactStore.deleteContact(id)
    showSuccessMessage(`連絡先「${contactName}」が正常に削除されました`)
  } catch (error) {
    // エラーメッセージはストアで設定されている
    if (contactStore.error) {
      showErrorMessage(contactStore.error)
    } else {
      showErrorMessage('連絡先を削除できません。もう一度お試しください。')
    }
  }
}

// 連絡先を編集
const handleEdit = (id: string) => {
  router.push(`/contacts/${id}/edit`)
}

// 連絡先詳細を表示
const handleView = (contact: any) => {
  const id = typeof contact === 'string' ? contact : contact.id
  router.push(`/contacts/${id}`)
}

// 新しい連絡先を追加
const handleAddContact = () => {
  router.push('/contacts/new')
}

// ログアウト
const handleLogout = async () => {
  await authStore.logout()
  router.push('/login')
}

// ページネーション情報
const displayRange = computed(() => {
  if (!contactStore.pagination) return ''
  
  const { currentPage, pageSize, totalCount } = contactStore.pagination
  const start = (currentPage - 1) * pageSize + 1
  const end = Math.min(currentPage * pageSize, totalCount)
  
  return `${totalCount}件中${start}件〜${end}件を表示`
})

// ページネーションハンドラー
const handlePageChange = async (page: number) => {
  try {
    await contactStore.goToPage(page)
    // ページ変更後、ページトップにスクロール
    window.scrollTo({ top: 0, behavior: 'smooth' })
  } catch (error) {
    showErrorMessage('ページの読み込みに失敗しました。もう一度お試しください。')
  }
}

const handleNextPage = async () => {
  try {
    await contactStore.nextPage()
    window.scrollTo({ top: 0, behavior: 'smooth' })
  } catch (error) {
    showErrorMessage('ページの読み込みに失敗しました。もう一度お試しください。')
  }
}

const handlePreviousPage = async () => {
  try {
    await contactStore.previousPage()
    window.scrollTo({ top: 0, behavior: 'smooth' })
  } catch (error) {
    showErrorMessage('ページの読み込みに失敗しました。もう一度お試しください。')
  }
}

// コンポーネントマウント時に連絡先を取得
onMounted(async () => {
  try {
    await contactStore.fetchContacts()
  } catch (error) {
    showErrorMessage('連絡先を読み込めません。後でもう一度お試しください。')
  }
})
</script>

<template>
  <div class="min-h-screen bg-gray-50">
    <!-- Header -->
    <header class="bg-white shadow">
      <div class="mx-auto max-w-7xl px-4 py-6 sm:px-6 lg:px-8">
        <div class="flex items-center justify-between">
          <h1 class="text-3xl font-bold text-gray-900">住所録</h1>
          <div class="flex items-center space-x-4">
            <span class="text-sm text-gray-600">
              {{ authStore.user?.email }}
            </span>
            <button
              @click="handleLogout"
              class="rounded-md bg-gray-600 px-4 py-2 text-sm font-medium text-white hover:bg-gray-700 focus:outline-none focus:ring-2 focus:ring-gray-500 focus:ring-offset-2"
            >
              ログアウト
            </button>
          </div>
        </div>
      </div>
    </header>

    <!-- Main content -->
    <main class="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      <!-- Success message -->
      <div
        v-if="successMessage"
        class="mb-4 rounded-md bg-green-50 p-4"
        role="alert"
        data-testid="success-message"
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
                d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.857-9.809a.75.75 0 00-1.214-.882l-3.483 4.79-1.88-1.88a.75.75 0 10-1.06 1.061l2.5 2.5a.75.75 0 001.137-.089l4-5.5z"
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

      <!-- Error message -->
      <div
        v-if="errorMessage"
        class="mb-4 rounded-md bg-red-50 p-4"
        role="alert"
        data-testid="error-message"
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
                d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.28 7.22a.75.75 0 00-1.06 1.06L8.94 10l-1.72 1.72a.75.75 0 101.06 1.06L10 11.06l1.72 1.72a.75.75 0 101.06-1.06L11.06 10l1.72-1.72a.75.75 0 00-1.06-1.06L10 8.94 8.28 7.22z"
                clip-rule="evenodd"
              />
            </svg>
          </div>
          <div class="ml-3">
            <p class="text-sm font-medium text-red-800">
              {{ errorMessage }}
            </p>
          </div>
        </div>
      </div>

      <!-- Action bar -->
      <div class="mb-6 flex items-center justify-between">
        <div class="text-sm text-gray-600">
          {{ displayRange }}
        </div>
        <button
          @click="handleAddContact"
          data-testid="add-contact-button"
          class="inline-flex items-center rounded-md bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
        >
          <svg
            class="mr-2 h-5 w-5"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M12 4v16m8-8H4"
            />
          </svg>
          連絡先を追加
        </button>
      </div>

      <!-- Loading state -->
      <div v-if="contactStore.loading" class="flex justify-center py-12">
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
      </div>

      <!-- Empty state -->
      <div
        v-else-if="!contactStore.hasContacts"
        class="rounded-lg bg-white p-12 text-center shadow"
        data-testid="empty-state"
      >
        <svg
          class="mx-auto h-12 w-12 text-gray-400"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
          xmlns="http://www.w3.org/2000/svg"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"
          />
        </svg>
        <h3 class="mt-2 text-sm font-semibold text-gray-900">連絡先がありません</h3>
        <p class="mt-1 text-sm text-gray-500">
          連絡先が見つかりません。最初の連絡先を追加して始めましょう。
        </p>
        <div class="mt-6">
          <button
            @click="handleAddContact"
            class="inline-flex items-center rounded-md bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
          >
            <svg
              class="mr-2 h-5 w-5"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
              xmlns="http://www.w3.org/2000/svg"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M12 4v16m8-8H4"
              />
            </svg>
            連絡先を追加
          </button>
        </div>
      </div>

      <!-- Contact list -->
      <div
        v-else
        class="grid gap-4 sm:grid-cols-2 lg:grid-cols-3"
        data-testid="contact-list"
      >
        <ContactCard
          v-for="contact in contactStore.contacts"
          :key="contact.id"
          :contact="contact"
          @click="handleView"
        />
      </div>

      <!-- Pagination -->
      <div
        v-if="contactStore.hasContacts && contactStore.pagination && contactStore.pagination.totalPages > 1"
        class="mt-8"
      >
        <BasePagination
          :current-page="contactStore.currentPage"
          :total-pages="contactStore.totalPages"
          :has-next-page="contactStore.hasNextPage"
          :has-previous-page="contactStore.hasPreviousPage"
          @page-change="handlePageChange"
          @next-page="handleNextPage"
          @previous-page="handlePreviousPage"
        />
      </div>
    </main>
  </div>
</template>
