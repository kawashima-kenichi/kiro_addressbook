/**
 * Pinia連絡先ストア
 * 連絡先の状態管理、CRUD操作、ページネーション、エラーハンドリング
 */

import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { contactService } from '@/services/contactService'
import type {
  ContactDto,
  CreateContactRequest,
  UpdateContactRequest,
  PaginationDto,
  ContactErrorResponse,
} from '@/types/contact'

export const useContactStore = defineStore('contact', () => {
  // State
  const contacts = ref<ContactDto[]>([])
  const currentContact = ref<ContactDto | null>(null)
  const pagination = ref<PaginationDto | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)
  const validationErrors = ref<Record<string, string>>({})

  // Getters
  const hasContacts = computed(() => contacts.value.length > 0)
  const totalCount = computed(() => pagination.value?.totalCount ?? 0)
  const currentPage = computed(() => pagination.value?.currentPage ?? 1)
  const totalPages = computed(() => pagination.value?.totalPages ?? 1)
  const hasNextPage = computed(() => pagination.value?.hasNextPage ?? false)
  const hasPreviousPage = computed(() => pagination.value?.hasPreviousPage ?? false)

  /**
   * 連絡先一覧を取得
   * @param page ページ番号（デフォルト: 1）
   * @param limit 1ページあたりの件数（デフォルト: 50）
   */
  async function fetchContacts(page: number = 1, limit: number = 50): Promise<void> {
    loading.value = true
    error.value = null
    validationErrors.value = {}

    try {
      const response = await contactService.getContacts(page, limit)
      contacts.value = response.contacts
      pagination.value = response.pagination
    } catch (err: unknown) {
      handleError(err, '連絡先を読み込めません。後でもう一度お試しください。')
      throw err
    } finally {
      loading.value = false
    }
  }

  /**
   * 連絡先詳細を取得
   * @param id 連絡先ID
   */
  async function fetchContactById(id: string): Promise<void> {
    loading.value = true
    error.value = null
    validationErrors.value = {}

    try {
      const contact = await contactService.getContactById(id)
      currentContact.value = contact
    } catch (err: unknown) {
      handleError(err, '連絡先を読み込めません。後でもう一度お試しください。')
      throw err
    } finally {
      loading.value = false
    }
  }

  /**
   * 連絡先を作成
   * @param data 連絡先作成データ
   */
  async function createContact(data: CreateContactRequest): Promise<ContactDto> {
    loading.value = true
    error.value = null
    validationErrors.value = {}

    try {
      const newContact = await contactService.createContact(data)
      
      // 連絡先リストに追加（ソート順を維持するため、再取得が望ましい）
      // ここでは簡易的に追加のみ行う
      contacts.value.push(newContact)
      
      return newContact
    } catch (err: unknown) {
      handleError(err, '連絡先を保存できません。もう一度お試しください。')
      throw err
    } finally {
      loading.value = false
    }
  }

  /**
   * 連絡先を更新
   * @param id 連絡先ID
   * @param data 連絡先更新データ
   */
  async function updateContact(id: string, data: UpdateContactRequest): Promise<ContactDto> {
    loading.value = true
    error.value = null
    validationErrors.value = {}

    try {
      const updatedContact = await contactService.updateContact(id, data)
      
      // 連絡先リストを更新
      const index = contacts.value.findIndex((c) => c.id === id)
      if (index !== -1) {
        contacts.value[index] = updatedContact
      }
      
      // 現在の連絡先を更新
      if (currentContact.value?.id === id) {
        currentContact.value = updatedContact
      }
      
      return updatedContact
    } catch (err: unknown) {
      handleError(err, '連絡先を更新できません。もう一度お試しください。')
      throw err
    } finally {
      loading.value = false
    }
  }

  /**
   * 連絡先を削除
   * @param id 連絡先ID
   */
  async function deleteContact(id: string): Promise<void> {
    loading.value = true
    error.value = null
    validationErrors.value = {}

    try {
      await contactService.deleteContact(id)
      
      // 連絡先リストから削除
      contacts.value = contacts.value.filter((c) => c.id !== id)
      
      // 現在の連絡先をクリア
      if (currentContact.value?.id === id) {
        currentContact.value = null
      }
      
      // ページネーション情報を更新
      if (pagination.value) {
        pagination.value.totalCount -= 1
      }
    } catch (err: unknown) {
      handleError(err, '連絡先を削除できません。もう一度お試しください。')
      throw err
    } finally {
      loading.value = false
    }
  }

  /**
   * 次のページに移動
   */
  async function nextPage(): Promise<void> {
    if (hasNextPage.value && pagination.value) {
      await fetchContacts(pagination.value.currentPage + 1, pagination.value.pageSize)
    }
  }

  /**
   * 前のページに移動
   */
  async function previousPage(): Promise<void> {
    if (hasPreviousPage.value && pagination.value) {
      await fetchContacts(pagination.value.currentPage - 1, pagination.value.pageSize)
    }
  }

  /**
   * 指定したページに移動
   * @param page ページ番号
   */
  async function goToPage(page: number): Promise<void> {
    if (pagination.value && page >= 1 && page <= pagination.value.totalPages) {
      await fetchContacts(page, pagination.value.pageSize)
    }
  }

  /**
   * エラーハンドリング
   * @param err エラーオブジェクト
   * @param defaultMessage デフォルトエラーメッセージ
   */
  function handleError(err: unknown, defaultMessage: string): void {
    // Type guard to check if err has response property
    const hasResponse = (error: unknown): error is { response: { status: number; data: ContactErrorResponse } } => {
      return (
        typeof error === 'object' &&
        error !== null &&
        'response' in error &&
        typeof (error as { response?: unknown }).response === 'object' &&
        (error as { response?: { status?: unknown } }).response !== null
      )
    }

    if (hasResponse(err)) {
      const status = err.response.status
      const errorData = err.response.data

      if (status === 400) {
        // バリデーションエラー
        if (errorData.errors && errorData.errors.length > 0) {
          // フィールド固有のエラーを設定
          errorData.errors.forEach((validationError) => {
            validationErrors.value[validationError.field] = validationError.message
          })
          error.value = '入力内容に誤りがあります。'
        } else {
          error.value = errorData.message || defaultMessage
        }
      } else if (status === 404) {
        error.value = '連絡先が見つかりません。'
      } else if (status === 409) {
        // 重複エラー
        error.value = errorData.message || 'この名前の連絡先は既に存在します'
      } else if (status === 500) {
        error.value = 'システムエラーが発生しました。もう一度お試しください。'
      } else {
        error.value = defaultMessage
      }
    } else {
      error.value = defaultMessage
    }
  }

  /**
   * エラーをクリア
   */
  function clearError(): void {
    error.value = null
    validationErrors.value = {}
  }

  /**
   * 現在の連絡先をクリア
   */
  function clearCurrentContact(): void {
    currentContact.value = null
  }

  /**
   * ストアをリセット
   */
  function reset(): void {
    contacts.value = []
    currentContact.value = null
    pagination.value = null
    loading.value = false
    error.value = null
    validationErrors.value = {}
  }

  return {
    // State
    contacts,
    currentContact,
    pagination,
    loading,
    error,
    validationErrors,

    // Getters
    hasContacts,
    totalCount,
    currentPage,
    totalPages,
    hasNextPage,
    hasPreviousPage,

    // Actions
    fetchContacts,
    fetchContactById,
    createContact,
    updateContact,
    deleteContact,
    nextPage,
    previousPage,
    goToPage,
    clearError,
    clearCurrentContact,
    reset,
  }
})
