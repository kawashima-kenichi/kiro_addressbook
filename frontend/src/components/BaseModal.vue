<script setup lang="ts">
import { ref, watch, onUnmounted } from 'vue'

interface Props {
  isOpen: boolean
  title?: string
  message: string
  confirmText?: string
  cancelText?: string
  autoCloseTimeout?: number // タイムアウト時間（ミリ秒）、デフォルト: 30秒
}

const props = withDefaults(defineProps<Props>(), {
  title: '確認',
  confirmText: '確認',
  cancelText: 'キャンセル',
  autoCloseTimeout: 30000, // 30秒
})

const emit = defineEmits<{
  confirm: []
  cancel: []
  close: []
}>()

const timeoutId = ref<number | null>(null)
const remainingTime = ref(props.autoCloseTimeout / 1000) // 秒単位
const countdownInterval = ref<number | null>(null)

// タイムアウトとカウントダウンを開始
const startTimeout = () => {
  // 既存のタイマーをクリア
  clearTimers()
  
  // 残り時間をリセット
  remainingTime.value = props.autoCloseTimeout / 1000
  
  // カウントダウンを開始（1秒ごとに更新）
  countdownInterval.value = window.setInterval(() => {
    remainingTime.value -= 1
    if (remainingTime.value <= 0) {
      clearTimers()
    }
  }, 1000)
  
  // 自動クローズタイマーを開始
  timeoutId.value = window.setTimeout(() => {
    handleCancel()
  }, props.autoCloseTimeout)
}

// タイマーをクリア
const clearTimers = () => {
  if (timeoutId.value !== null) {
    clearTimeout(timeoutId.value)
    timeoutId.value = null
  }
  if (countdownInterval.value !== null) {
    clearInterval(countdownInterval.value)
    countdownInterval.value = null
  }
}

// モーダルが開いたときにタイムアウトを開始
watch(() => props.isOpen, (newValue) => {
  if (newValue) {
    startTimeout()
  } else {
    clearTimers()
  }
})

// コンポーネントがアンマウントされたときにタイマーをクリア
onUnmounted(() => {
  clearTimers()
})

const handleConfirm = () => {
  clearTimers()
  emit('confirm')
  emit('close')
}

const handleCancel = () => {
  clearTimers()
  emit('cancel')
  emit('close')
}

// Escapeキーでモーダルを閉じる
const handleKeydown = (event: KeyboardEvent) => {
  if (event.key === 'Escape') {
    handleCancel()
  }
}
</script>

<template>
  <Teleport to="body">
    <Transition name="modal">
      <div
        v-if="isOpen"
        class="fixed inset-0 z-50 overflow-y-auto"
        aria-labelledby="modal-title"
        role="dialog"
        aria-modal="true"
        @keydown="handleKeydown"
      >
        <!-- Background overlay -->
        <div
          class="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity"
          @click="handleCancel"
        ></div>

        <!-- Modal panel -->
        <div class="flex min-h-full items-center justify-center p-4 text-center sm:p-0">
          <div
            class="relative transform overflow-hidden rounded-lg bg-white text-left shadow-xl transition-all sm:my-8 sm:w-full sm:max-w-lg"
          >
            <div class="bg-white px-4 pb-4 pt-5 sm:p-6 sm:pb-4">
              <div class="sm:flex sm:items-start">
                <!-- Warning icon -->
                <div
                  class="mx-auto flex h-12 w-12 flex-shrink-0 items-center justify-center rounded-full bg-red-100 sm:mx-0 sm:h-10 sm:w-10"
                >
                  <svg
                    class="h-6 w-6 text-red-600"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke-width="1.5"
                    stroke="currentColor"
                    aria-hidden="true"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      d="M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126zM12 15.75h.007v.008H12v-.008z"
                    />
                  </svg>
                </div>

                <!-- Content -->
                <div class="mt-3 text-center sm:ml-4 sm:mt-0 sm:text-left flex-1">
                  <h3
                    id="modal-title"
                    class="text-base font-semibold leading-6 text-gray-900"
                  >
                    {{ title }}
                  </h3>
                  <div class="mt-2">
                    <p class="text-sm text-gray-500">
                      {{ message }}
                    </p>
                  </div>
                  
                  <!-- Countdown timer -->
                  <div class="mt-3">
                    <p class="text-xs text-gray-400">
                      {{ remainingTime }}秒後に自動的に閉じます
                    </p>
                  </div>
                </div>
              </div>
            </div>

            <!-- Action buttons -->
            <div class="bg-gray-50 px-4 py-3 sm:flex sm:flex-row-reverse sm:px-6">
              <button
                type="button"
                data-testid="confirm-button"
                class="inline-flex w-full justify-center rounded-md bg-red-600 px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-red-500 sm:ml-3 sm:w-auto focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2"
                @click="handleConfirm"
              >
                {{ confirmText }}
              </button>
              <button
                type="button"
                data-testid="cancel-button"
                class="mt-3 inline-flex w-full justify-center rounded-md bg-white px-3 py-2 text-sm font-semibold text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 hover:bg-gray-50 sm:mt-0 sm:w-auto focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
                @click="handleCancel"
              >
                {{ cancelText }}
              </button>
            </div>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.modal-enter-active,
.modal-leave-active {
  transition: opacity 0.3s ease;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}

.modal-enter-active .transform,
.modal-leave-active .transform {
  transition: all 0.3s ease;
}

.modal-enter-from .transform,
.modal-leave-to .transform {
  transform: scale(0.95);
}
</style>
