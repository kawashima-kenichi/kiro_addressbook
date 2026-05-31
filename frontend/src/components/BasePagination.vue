<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  currentPage: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

const props = defineProps<Props>()

const emit = defineEmits<{
  'page-change': [page: number]
  'next-page': []
  'previous-page': []
}>()

// ページ番号のリストを生成（最大7ページ分表示）
const pageNumbers = computed(() => {
  const pages: (number | string)[] = []
  const { currentPage, totalPages } = props

  if (totalPages <= 7) {
    // 7ページ以下の場合は全て表示
    for (let i = 1; i <= totalPages; i++) {
      pages.push(i)
    }
  } else {
    // 7ページより多い場合は省略表示
    if (currentPage <= 3) {
      // 最初の方のページ
      for (let i = 1; i <= 5; i++) {
        pages.push(i)
      }
      pages.push('...')
      pages.push(totalPages)
    } else if (currentPage >= totalPages - 2) {
      // 最後の方のページ
      pages.push(1)
      pages.push('...')
      for (let i = totalPages - 4; i <= totalPages; i++) {
        pages.push(i)
      }
    } else {
      // 中間のページ
      pages.push(1)
      pages.push('...')
      for (let i = currentPage - 1; i <= currentPage + 1; i++) {
        pages.push(i)
      }
      pages.push('...')
      pages.push(totalPages)
    }
  }

  return pages
})

const handlePageClick = (page: number | string) => {
  if (typeof page === 'number' && page !== props.currentPage) {
    emit('page-change', page)
  }
}

const handlePrevious = () => {
  if (props.hasPreviousPage) {
    emit('previous-page')
  }
}

const handleNext = () => {
  if (props.hasNextPage) {
    emit('next-page')
  }
}
</script>

<template>
  <nav
    class="flex items-center justify-between border-t border-gray-200 px-4 sm:px-0"
    aria-label="ページネーション"
    data-testid="pagination"
  >
    <div class="-mt-px flex w-0 flex-1">
      <button
        @click="handlePrevious"
        :disabled="!hasPreviousPage"
        :class="[
          'inline-flex items-center border-t-2 border-transparent pr-1 pt-4 text-sm font-medium transition-colors',
          hasPreviousPage
            ? 'text-gray-500 hover:border-gray-300 hover:text-gray-700'
            : 'cursor-not-allowed text-gray-300',
        ]"
        data-testid="previous-button"
      >
        <svg
          class="mr-3 h-5 w-5"
          fill="currentColor"
          viewBox="0 0 20 20"
          aria-hidden="true"
        >
          <path
            fill-rule="evenodd"
            d="M18 10a.75.75 0 01-.75.75H4.66l2.1 1.95a.75.75 0 11-1.02 1.1l-3.5-3.25a.75.75 0 010-1.1l3.5-3.25a.75.75 0 111.02 1.1l-2.1 1.95h12.59A.75.75 0 0118 10z"
            clip-rule="evenodd"
          />
        </svg>
        前へ
      </button>
    </div>
    <div class="hidden md:-mt-px md:flex">
      <button
        v-for="(page, index) in pageNumbers"
        :key="index"
        @click="handlePageClick(page)"
        :disabled="page === '...'"
        :class="[
          'inline-flex items-center border-t-2 px-4 pt-4 text-sm font-medium transition-colors',
          page === currentPage
            ? 'border-indigo-500 text-indigo-600'
            : page === '...'
              ? 'cursor-default border-transparent text-gray-500'
              : 'border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700',
        ]"
        :data-testid="page === '...' ? 'page-ellipsis' : `page-${page}`"
      >
        {{ page }}
      </button>
    </div>
    <div class="-mt-px flex w-0 flex-1 justify-end">
      <button
        @click="handleNext"
        :disabled="!hasNextPage"
        :class="[
          'inline-flex items-center border-t-2 border-transparent pl-1 pt-4 text-sm font-medium transition-colors',
          hasNextPage
            ? 'text-gray-500 hover:border-gray-300 hover:text-gray-700'
            : 'cursor-not-allowed text-gray-300',
        ]"
        data-testid="next-button"
      >
        次へ
        <svg
          class="ml-3 h-5 w-5"
          fill="currentColor"
          viewBox="0 0 20 20"
          aria-hidden="true"
        >
          <path
            fill-rule="evenodd"
            d="M2 10a.75.75 0 01.75-.75h12.59l-2.1-1.95a.75.75 0 111.02-1.1l3.5 3.25a.75.75 0 010 1.1l-3.5 3.25a.75.75 0 11-1.02-1.1l2.1-1.95H2.75A.75.75 0 012 10z"
            clip-rule="evenodd"
          />
        </svg>
      </button>
    </div>
  </nav>
</template>
