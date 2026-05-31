<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useContactStore } from '@/stores/contact'
import type { UpdateContactRequest } from '@/types/contact'

/**
 * ContactDetail コンポーネント
 * 連絡先の詳細表示と編集機能を提供
 * 
 * Requirements: 3.4, 4.1, 4.2, 4.3, 4.4, 4.5, 4.6
 */

interface Props {
  contactId: string
}

const props = defineProps<Props>()

const emit = defineEmits<{
  updated: []
  cancelled: []
}>()

const contactStore = useContactStore()

// State
const isEditMode = ref(false)
const formData = ref<UpdateContactRequest>({
  name: '',
  address: '',
  phoneNumber: '',
})
const originalData = ref<UpdateContactRequest>({
  name: '',
  address: '',
  phoneNumber: '',
})
const successMessage = ref<string | null>(null)
const localValidationErrors = ref<Record<string, string>>({})

// Computed
const contact = computed(() => contactStore.currentContact)
const loading = computed(() => contactStore.loading)
const error = computed(() => contactStore.error)
const validationErrors = computed(() => contactStore.validationErrors)

/**
 * 連絡先データを読み込む
 */
const loadContact = async () => {
  try {
    await contactStore.fetchContactById(props.contactId)
    if (contact.value) {
      // フォームデータを初期化
      formData.value = {
        name: contact.value.name,
        address: contact.value.address || '',
        phoneNumber: contact.value.phoneNumber || '',
      }
      // オリジナルデータを保存
      originalData.value = { ...formData.value }
    }
  } catch (err) {
    console.error('連絡先の読み込みに失敗しました:', err)
  }
}

/**
 * 編集モードに切り替え
 * Requirement 4.1: ユーザーが「編集」ボタンをクリックした場合、既存データでフォームを入力
 */
const enterEditMode = () => {
  if (contact.value) {
    formData.value = {
      name: contact.value.name,
      address: contact.value.address || '',
      phoneNumber: contact.value.phoneNumber || '',
    }
    originalData.value = { ...formData.value }
  }
  isEditMode.value = true
  successMessage.value = null
  contactStore.clearError()
  localValidationErrors.value = {}
}

/**
 * フロントエンドバリデーション
 */
const validateForm = (): boolean => {
  localValidationErrors.value = {}
  let isValid = true

  // 名前のバリデーション (Requirement 4.3)
  if (!formData.value.name || formData.value.name.trim().length === 0) {
    localValidationErrors.value.name = '名前は必須です'
    isValid = false
  } else if (formData.value.name.length > 100) {
    localValidationErrors.value.name = '名前は100文字以下で入力してください'
    isValid = false
  } else if (formData.value.name.length < 1) {
    localValidationErrors.value.name = '名前は1文字以上で入力してください'
    isValid = false
  }

  // 住所のバリデーション (Requirement 4.3)
  if (formData.value.address && formData.value.address.length > 500) {
    localValidationErrors.value.address = '住所は500文字以下で入力してください'
    isValid = false
  }

  // 電話番号のバリデーション (Requirement 4.3)
  if (formData.value.phoneNumber) {
    if (formData.value.phoneNumber.length > 20) {
      localValidationErrors.value.phoneNumber = '電話番号は20文字以下で入力してください'
      isValid = false
    } else {
      // 電話番号形式のバリデーション
      const phoneRegex = /^(\(\d{3}\)\s?\d{3}-\d{4}|\d{3}-\d{3}-\d{4}|\d{3}\.\d{3}\.\d{4}|\+\d{1}-\d{3}-\d{3}-\d{4}|\d{10})$/
      if (!phoneRegex.test(formData.value.phoneNumber)) {
        localValidationErrors.value.phoneNumber = '有効な電話番号を入力してください'
        isValid = false
      }
    }
  }

  return isValid
}

/**
 * 連絡先を更新
 * Requirement 4.2: 有効な変更を送信した場合、連絡先を更新
 * Requirement 4.4: 正常に更新された場合、確認メッセージを表示
 * Requirement 4.6: 更新中にエラーが発生した場合、エラーメッセージを表示
 */
const handleUpdate = async () => {
  // フロントエンドバリデーション
  if (!validateForm()) {
    return
  }

  try {
    const updatedContact = await contactStore.updateContact(props.contactId, formData.value)
    
    // 成功メッセージを表示 (Requirement 4.4)
    successMessage.value = `連絡先「${updatedContact.name}」が正常に更新されました`
    
    // 編集モードを終了
    isEditMode.value = false
    
    // オリジナルデータを更新
    originalData.value = { ...formData.value }
    
    // 親コンポーネントに通知
    emit('updated')
    
    // 3秒後に成功メッセージをクリア
    setTimeout(() => {
      successMessage.value = null
    }, 3000)
  } catch (err) {
    // エラーは contactStore で処理される (Requirement 4.6)
    console.error('連絡先の更新に失敗しました:', err)
  }
}

/**
 * 編集をキャンセル
 * Requirement 4.5: 「キャンセル」をクリックした場合、変更を破棄して前のビューに戻る
 */
const handleCancel = () => {
  // フォームデータをオリジナルに戻す
  formData.value = { ...originalData.value }
  
  // 編集モードを終了
  isEditMode.value = false
  
  // エラーをクリア
  contactStore.clearError()
  localValidationErrors.value = {}
  successMessage.value = null
  
  // 親コンポーネントに通知
  emit('cancelled')
}

/**
 * フィールドのエラーメッセージを取得
 */
const getFieldError = (field: string): string | null => {
  return localValidationErrors.value[field] || validationErrors.value[field] || null
}

/**
 * 電話番号の表示用フォーマット
 */
const formatPhoneNumber = (phone: string | undefined): string => {
  return phone || '電話番号なし'
}

// コンポーネントマウント時に連絡先を読み込む
onMounted(() => {
  loadContact()
})
</script>

<template>
  <div class="contact-detail">
    <!-- ローディング状態 -->
    <div v-if="loading" class="flex justify-center py-8">
      <div class="h-8 w-8 animate-spin rounded-full border-4 border-indigo-600 border-t-transparent"></div>
    </div>

    <!-- エラーメッセージ -->
    <div
      v-if="error"
      class="mb-4 rounded-md bg-red-50 p-4"
      role="alert"
      data-testid="error-message"
    >
      <div class="flex">
        <div class="flex-shrink-0">
          <svg class="h-5 w-5 text-red-400" viewBox="0 0 20 20" fill="currentColor">
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

    <!-- 成功メッセージ -->
    <div
      v-if="successMessage"
      class="mb-4 rounded-md bg-green-50 p-4"
      role="alert"
      data-testid="success-message"
    >
      <div class="flex">
        <div class="flex-shrink-0">
          <svg class="h-5 w-5 text-green-400" viewBox="0 0 20 20" fill="currentColor">
            <path
              fill-rule="evenodd"
              d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z"
              clip-rule="evenodd"
            />
          </svg>
        </div>
        <div class="ml-3">
          <p class="text-sm font-medium text-green-800">{{ successMessage }}</p>
        </div>
      </div>
    </div>

    <!-- 連絡先詳細 -->
    <div v-if="contact && !loading" class="rounded-lg bg-white shadow">
      <!-- 表示モード -->
      <div v-if="!isEditMode" class="p-6">
        <div class="mb-6 flex items-center justify-between">
          <h2 class="text-2xl font-bold text-gray-900" data-testid="contact-name">
            {{ contact.name }}
          </h2>
          <button
            @click="enterEditMode"
            class="rounded-md bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
            data-testid="edit-button"
          >
            編集
          </button>
        </div>

        <dl class="space-y-4">
          <div>
            <dt class="text-sm font-medium text-gray-500">電話番号</dt>
            <dd class="mt-1 text-sm text-gray-900" data-testid="contact-phone">
              {{ formatPhoneNumber(contact.phoneNumber) }}
            </dd>
          </div>

          <div>
            <dt class="text-sm font-medium text-gray-500">住所</dt>
            <dd class="mt-1 text-sm text-gray-900" data-testid="contact-address">
              {{ contact.address || '住所なし' }}
            </dd>
          </div>

          <div>
            <dt class="text-sm font-medium text-gray-500">作成日</dt>
            <dd class="mt-1 text-sm text-gray-900">
              {{ new Date(contact.createdAt).toLocaleString('ja-JP') }}
            </dd>
          </div>

          <div>
            <dt class="text-sm font-medium text-gray-500">最終更新日</dt>
            <dd class="mt-1 text-sm text-gray-900">
              {{ new Date(contact.updatedAt).toLocaleString('ja-JP') }}
            </dd>
          </div>
        </dl>
      </div>

      <!-- 編集モード -->
      <div v-else class="p-6">
        <h2 class="mb-6 text-2xl font-bold text-gray-900">連絡先を編集</h2>

        <form @submit.prevent="handleUpdate" class="space-y-6">
          <!-- 名前フィールド -->
          <div>
            <label for="name" class="block text-sm font-medium text-gray-700">
              名前 <span class="text-red-500">*</span>
            </label>
            <input
              id="name"
              v-model="formData.name"
              type="text"
              required
              maxlength="100"
              class="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-indigo-500 sm:text-sm"
              :class="{ 'border-red-500': getFieldError('name') }"
              data-testid="name-input"
            />
            <p
              v-if="getFieldError('name')"
              class="mt-1 text-sm text-red-600"
              data-testid="name-error"
            >
              {{ getFieldError('name') }}
            </p>
          </div>

          <!-- 電話番号フィールド -->
          <div>
            <label for="phoneNumber" class="block text-sm font-medium text-gray-700">
              電話番号
            </label>
            <input
              id="phoneNumber"
              v-model="formData.phoneNumber"
              type="text"
              maxlength="20"
              placeholder="例: 03-1234-5678"
              class="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-indigo-500 sm:text-sm"
              :class="{ 'border-red-500': getFieldError('phoneNumber') }"
              data-testid="phone-input"
            />
            <p
              v-if="getFieldError('phoneNumber')"
              class="mt-1 text-sm text-red-600"
              data-testid="phone-error"
            >
              {{ getFieldError('phoneNumber') }}
            </p>
          </div>

          <!-- 住所フィールド -->
          <div>
            <label for="address" class="block text-sm font-medium text-gray-700">
              住所
            </label>
            <textarea
              id="address"
              v-model="formData.address"
              rows="3"
              maxlength="500"
              class="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-indigo-500 sm:text-sm"
              :class="{ 'border-red-500': getFieldError('address') }"
              data-testid="address-input"
            />
            <p
              v-if="getFieldError('address')"
              class="mt-1 text-sm text-red-600"
              data-testid="address-error"
            >
              {{ getFieldError('address') }}
            </p>
          </div>

          <!-- アクションボタン -->
          <div class="flex justify-end space-x-3">
            <button
              type="button"
              @click="handleCancel"
              class="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
              data-testid="cancel-button"
            >
              キャンセル
            </button>
            <button
              type="submit"
              :disabled="loading"
              class="rounded-md bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 disabled:opacity-50"
              data-testid="save-button"
            >
              {{ loading ? '保存中...' : '保存' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* アニメーション用のスタイル */
@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.animate-spin {
  animation: spin 1s linear infinite;
}
</style>
