<script setup lang="ts">
import { onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useContactStore } from '@/stores/contact'
import ContactCard from './ContactCard.vue'
import BasePagination from './BasePagination.vue'
import type { ContactDto } from '@/types/contact'

const contactStore = useContactStore()
const router = useRouter()

// Computed properties
const contacts = computed(() => contactStore.contacts)
const loading = computed(() => contactStore.loading)
const error = computed(() => contactStore.error)
const hasContacts = computed(() => contactStore.hasContacts)
const totalCount = computed(() => contactStore.totalCount)
const currentPage = computed(() => contactStore.currentPage)
const totalPages = computed(() => contactStore.totalPages)
const hasNextPage = computed(() => contactStore.hasNextPage)
const hasPreviousPage = computed(() => contactStore.hasPreviousPage)

// 連絡先数インジケーター（「Y件中X件を表示」）
const contactCountIndicator = computed(() => {
  if (!contactStore.pagination) return ''
  
  const { currentPage, pageSize, totalCount } = contactStore.pagination
  const startIndex = (currentPage - 1) * pageSize + 1
  const endIndex = Math.min(currentPage * pageSize, totalCount)
  
  return `${totalCount}件中${startIndex}-${endIndex}件を表示`
})

// 連絡先をクリックした時の処理
const handleContactClick = (contact: ContactDto) => {
  // TODO: 連絡先詳細ページに遷移（タスク9.4で実装）
  console.log('Contact clicked:', contact)
  // router.push({ name: 'contact-detail', params: { id: contact.id } })
}

// ページネーション処理
const handlePageChange = (page: number) => {
  contactStore.goToPage(page)
}

const handleNextPage = () => {
  contactStore.nextPage()
}

const handlePreviousPage = () => {
  contactStore.previousPage()
}

// 連絡先を追加するページに遷移
const handleAddContact = () => {
  // TODO: 連絡先作成ページに遷移（タスク9.3で実装）
  console.log('Add contact clicked')
  // router.push({ name: 'contact-create' })
}

// コンポーネントマウント時に連絡先一覧を取得
onMounted(async () => {
  try {
    await contactStore.fetchContacts()
  } catch (err) {
    console.error('Failed to fetch contacts:', err)
  }
})
</script>

<template>
  <div class="space-y-6" data-testid="contact-list">
    <!-- ヘッダー -->
    <div class="flex items-center justify-between">
      <div>
        <h2 class="text-2xl font-bold text-gray-900">連絡先一覧</h2>
        <p v-if="hasContacts" class="mt-1 text-sm text-gray-600" data-testid="contact-count">
          {{ contactCountIndicator }}
        </p>
      </div>
      <button
        @click="handleAddContact"
        class="inline-flex items-center rounded-md bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
        data-testid="add-contact-button"
      >
        <svg
          class="-ml-1 mr-2 h-5 w-5"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
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

    <!-- エラーメッセージ -->
    <div
      v-if="error"
      class="rounded-md bg-red-50 p-4"
      role="alert"
      data-testid="error-message"
    >
      <div class="flex">
        <div class="flex-shrink-0">
          <svg
            class="h-5 w-5 text-red-400"
            fill="currentColor"
            viewBox="0 0 20 20"
          >
            <path
              fill-rule="evenodd"
              d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
              clip-rule="evenodd"
            />
          </svg>
        </div>
        <div class="ml-3">
          <p class="text-sm font-medium text-red-800">{{ error }}</p>
        </div>
      </div>
    </div>

    <!-- ローディング状態 -->
    <div
      v-if="loading"
      class="flex items-center justify-center py-12"
      data-testid="loading-spinner"
    >
      <div class="h-12 w-12 animate-spin rounded-full border-b-2 border-t-2 border-indigo-600"></div>
    </div>

    <!-- 空状態メッセージ -->
    <div
      v-else-if="!hasContacts && !error"
      class="rounded-lg border-2 border-dashed border-gray-300 bg-white p-12 text-center"
      data-testid="empty-state"
    >
      <svg
        class="mx-auto h-12 w-12 text-gray-400"
        fill="none"
        stroke="currentColor"
        viewBox="0 0 24 24"
      >
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          stroke-width="2"
          d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"
        />
      </svg>
      <h3 class="mt-4 text-lg font-medium text-gray-900">連絡先が見つかりません</h3>
      <p class="mt-2 text-sm text-gray-500">
        最初の連絡先を追加して始めましょう。
      </p>
      <button
        @click="handleAddContact"
        class="mt-6 inline-flex items-center rounded-md bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
      >
        <svg
          class="-ml-1 mr-2 h-5 w-5"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
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

    <!-- 連絡先リスト -->
    <div
      v-else-if="hasContacts"
      class="space-y-4"
      data-testid="contact-cards-container"
    >
      <ContactCard
        v-for="contact in contacts"
        :key="contact.id"
        :contact="contact"
        @click="handleContactClick"
      />
    </div>

    <!-- ページネーション（50件を超える場合のみ表示） -->
    <div v-if="totalPages > 1" class="mt-8">
      <BasePagination
        :current-page="currentPage"
        :total-pages="totalPages"
        :has-next-page="hasNextPage"
        :has-previous-page="hasPreviousPage"
        @page-change="handlePageChange"
        @next-page="handleNextPage"
        @previous-page="handlePreviousPage"
      />
    </div>
  </div>
</template>
