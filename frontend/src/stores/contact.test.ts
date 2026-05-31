import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useContactStore } from './contact'
import { contactService } from '@/services/contactService'
import type { ContactDto, ContactListResponse } from '@/types/contact'

// Mock contactService
vi.mock('@/services/contactService', () => ({
  contactService: {
    getContacts: vi.fn(),
    getContactById: vi.fn(),
    createContact: vi.fn(),
    updateContact: vi.fn(),
    deleteContact: vi.fn(),
  },
}))

describe('useContactStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  describe('fetchContacts', () => {
    it('正常に連絡先一覧を取得できる', async () => {
      const mockResponse: ContactListResponse = {
        contacts: [
          {
            id: '1',
            userId: 'user1',
            name: '田中太郎',
            address: '東京都渋谷区',
            phoneNumber: '03-1234-5678',
            createdAt: '2024-01-01T00:00:00Z',
            updatedAt: '2024-01-01T00:00:00Z',
          },
        ],
        pagination: {
          currentPage: 1,
          pageSize: 50,
          totalCount: 1,
          totalPages: 1,
          hasNextPage: false,
          hasPreviousPage: false,
        },
      }

      vi.mocked(contactService.getContacts).mockResolvedValue(mockResponse)

      const store = useContactStore()
      await store.fetchContacts()

      expect(store.contacts).toEqual(mockResponse.contacts)
      expect(store.pagination).toEqual(mockResponse.pagination)
      expect(store.loading).toBe(false)
      expect(store.error).toBeNull()
    })

    it('エラー時に適切なエラーメッセージを設定する', async () => {
      vi.mocked(contactService.getContacts).mockRejectedValue(new Error('Network error'))

      const store = useContactStore()
      
      try {
        await store.fetchContacts()
      } catch (err) {
        // エラーが投げられることを期待
      }

      expect(store.error).toBe('連絡先を読み込めません。後でもう一度お試しください。')
      expect(store.loading).toBe(false)
    })
  })

  describe('fetchContactById', () => {
    it('正常に連絡先詳細を取得できる', async () => {
      const mockContact: ContactDto = {
        id: '1',
        userId: 'user1',
        name: '田中太郎',
        address: '東京都渋谷区',
        phoneNumber: '03-1234-5678',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      }

      vi.mocked(contactService.getContactById).mockResolvedValue(mockContact)

      const store = useContactStore()
      await store.fetchContactById('1')

      expect(store.currentContact).toEqual(mockContact)
      expect(store.loading).toBe(false)
      expect(store.error).toBeNull()
    })

    it('404エラー時に適切なエラーメッセージを設定する', async () => {
      const error = {
        response: {
          status: 404,
          data: { message: 'Contact not found' },
        },
      }
      vi.mocked(contactService.getContactById).mockRejectedValue(error)

      const store = useContactStore()
      
      try {
        await store.fetchContactById('999')
      } catch (err) {
        // エラーが投げられることを期待
      }

      expect(store.error).toBe('連絡先が見つかりません。')
      expect(store.loading).toBe(false)
    })
  })

  describe('createContact', () => {
    it('正常に連絡先を作成できる', async () => {
      const newContactData = {
        name: '佐藤花子',
        address: '大阪府大阪市',
        phoneNumber: '06-9876-5432',
      }

      const mockCreatedContact: ContactDto = {
        id: '2',
        userId: 'user1',
        ...newContactData,
        createdAt: '2024-01-02T00:00:00Z',
        updatedAt: '2024-01-02T00:00:00Z',
      }

      vi.mocked(contactService.createContact).mockResolvedValue(mockCreatedContact)

      const store = useContactStore()
      const result = await store.createContact(newContactData)

      expect(result).toEqual(mockCreatedContact)
      expect(store.contacts).toContainEqual(mockCreatedContact)
      expect(store.loading).toBe(false)
      expect(store.error).toBeNull()
    })

    it('バリデーションエラー時にフィールド固有のエラーを設定する', async () => {
      const error = {
        response: {
          status: 400,
          data: {
            message: '入力内容に誤りがあります',
            errors: [
              { field: 'name', message: '名前は必須です' },
              { field: 'phoneNumber', message: '有効な電話番号を入力してください' },
            ],
          },
        },
      }
      vi.mocked(contactService.createContact).mockRejectedValue(error)

      const store = useContactStore()
      
      try {
        await store.createContact({ name: '' })
      } catch (err) {
        // エラーが投げられることを期待
      }

      expect(store.error).toBe('入力内容に誤りがあります。')
      expect(store.validationErrors).toEqual({
        name: '名前は必須です',
        phoneNumber: '有効な電話番号を入力してください',
      })
      expect(store.loading).toBe(false)
    })

    it('重複エラー時に適切なエラーメッセージを設定する', async () => {
      const error = {
        response: {
          status: 409,
          data: { message: 'この名前の連絡先は既に存在します' },
        },
      }
      vi.mocked(contactService.createContact).mockRejectedValue(error)

      const store = useContactStore()
      
      try {
        await store.createContact({ name: '田中太郎' })
      } catch (err) {
        // エラーが投げられることを期待
      }

      expect(store.error).toBe('この名前の連絡先は既に存在します')
      expect(store.loading).toBe(false)
    })
  })

  describe('updateContact', () => {
    it('正常に連絡先を更新できる', async () => {
      const updateData = {
        name: '田中太郎',
        address: '東京都新宿区',
        phoneNumber: '03-1111-2222',
      }

      const mockUpdatedContact: ContactDto = {
        id: '1',
        userId: 'user1',
        ...updateData,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-02T00:00:00Z',
      }

      vi.mocked(contactService.updateContact).mockResolvedValue(mockUpdatedContact)

      const store = useContactStore()
      // 既存の連絡先を設定
      store.contacts = [
        {
          id: '1',
          userId: 'user1',
          name: '田中太郎',
          address: '東京都渋谷区',
          phoneNumber: '03-1234-5678',
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
      ]
      store.currentContact = store.contacts[0]

      const result = await store.updateContact('1', updateData)

      expect(result).toEqual(mockUpdatedContact)
      expect(store.contacts[0]).toEqual(mockUpdatedContact)
      expect(store.currentContact).toEqual(mockUpdatedContact)
      expect(store.loading).toBe(false)
      expect(store.error).toBeNull()
    })

    it('エラー時に適切なエラーメッセージを設定する', async () => {
      vi.mocked(contactService.updateContact).mockRejectedValue(new Error('Update failed'))

      const store = useContactStore()
      
      try {
        await store.updateContact('1', { name: '田中太郎' })
      } catch (err) {
        // エラーが投げられることを期待
      }

      expect(store.error).toBe('連絡先を更新できません。もう一度お試しください。')
      expect(store.loading).toBe(false)
    })
  })

  describe('deleteContact', () => {
    it('正常に連絡先を削除できる', async () => {
      vi.mocked(contactService.deleteContact).mockResolvedValue()

      const store = useContactStore()
      // 既存の連絡先を設定
      store.contacts = [
        {
          id: '1',
          userId: 'user1',
          name: '田中太郎',
          address: '東京都渋谷区',
          phoneNumber: '03-1234-5678',
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
      ]
      store.currentContact = store.contacts[0]
      store.pagination = {
        currentPage: 1,
        pageSize: 50,
        totalCount: 1,
        totalPages: 1,
        hasNextPage: false,
        hasPreviousPage: false,
      }

      await store.deleteContact('1')

      expect(store.contacts).toHaveLength(0)
      expect(store.currentContact).toBeNull()
      expect(store.pagination.totalCount).toBe(0)
      expect(store.loading).toBe(false)
      expect(store.error).toBeNull()
    })

    it('エラー時に適切なエラーメッセージを設定する', async () => {
      vi.mocked(contactService.deleteContact).mockRejectedValue(new Error('Delete failed'))

      const store = useContactStore()
      
      try {
        await store.deleteContact('1')
      } catch (err) {
        // エラーが投げられることを期待
      }

      expect(store.error).toBe('連絡先を削除できません。もう一度お試しください。')
      expect(store.loading).toBe(false)
    })
  })

  describe('pagination', () => {
    beforeEach(() => {
      const mockResponse: ContactListResponse = {
        contacts: [],
        pagination: {
          currentPage: 2,
          pageSize: 50,
          totalCount: 150,
          totalPages: 3,
          hasNextPage: true,
          hasPreviousPage: true,
        },
      }
      vi.mocked(contactService.getContacts).mockResolvedValue(mockResponse)
    })

    it('次のページに移動できる', async () => {
      const store = useContactStore()
      await store.fetchContacts(2, 50)
      
      await store.nextPage()

      expect(contactService.getContacts).toHaveBeenCalledWith(3, 50)
    })

    it('前のページに移動できる', async () => {
      const store = useContactStore()
      await store.fetchContacts(2, 50)
      
      await store.previousPage()

      expect(contactService.getContacts).toHaveBeenCalledWith(1, 50)
    })

    it('指定したページに移動できる', async () => {
      const store = useContactStore()
      await store.fetchContacts(2, 50)
      
      await store.goToPage(3)

      expect(contactService.getContacts).toHaveBeenCalledWith(3, 50)
    })

    it('無効なページ番号では移動しない', async () => {
      const store = useContactStore()
      await store.fetchContacts(2, 50)
      
      vi.clearAllMocks()
      await store.goToPage(0) // 無効なページ
      await store.goToPage(4) // 存在しないページ

      expect(contactService.getContacts).not.toHaveBeenCalled()
    })
  })

  describe('utility methods', () => {
    it('clearError でエラーをクリアできる', () => {
      const store = useContactStore()
      store.error = 'エラーメッセージ'
      store.validationErrors = { name: '名前は必須です' }

      store.clearError()

      expect(store.error).toBeNull()
      expect(store.validationErrors).toEqual({})
    })

    it('clearCurrentContact で現在の連絡先をクリアできる', () => {
      const store = useContactStore()
      store.currentContact = {
        id: '1',
        userId: 'user1',
        name: '田中太郎',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      }

      store.clearCurrentContact()

      expect(store.currentContact).toBeNull()
    })

    it('reset でストアを初期状態にリセットできる', () => {
      const store = useContactStore()
      store.contacts = [
        {
          id: '1',
          userId: 'user1',
          name: '田中太郎',
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
      ]
      store.error = 'エラー'
      store.loading = true

      store.reset()

      expect(store.contacts).toEqual([])
      expect(store.currentContact).toBeNull()
      expect(store.pagination).toBeNull()
      expect(store.loading).toBe(false)
      expect(store.error).toBeNull()
      expect(store.validationErrors).toEqual({})
    })
  })

  describe('computed properties', () => {
    it('hasContacts は連絡先の有無を正しく返す', () => {
      const store = useContactStore()
      
      expect(store.hasContacts).toBe(false)
      
      store.contacts = [
        {
          id: '1',
          userId: 'user1',
          name: '田中太郎',
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
      ]
      
      expect(store.hasContacts).toBe(true)
    })

    it('pagination computed properties が正しく動作する', () => {
      const store = useContactStore()
      
      expect(store.totalCount).toBe(0)
      expect(store.currentPage).toBe(1)
      expect(store.totalPages).toBe(1)
      expect(store.hasNextPage).toBe(false)
      expect(store.hasPreviousPage).toBe(false)
      
      store.pagination = {
        currentPage: 2,
        pageSize: 50,
        totalCount: 150,
        totalPages: 3,
        hasNextPage: true,
        hasPreviousPage: true,
      }
      
      expect(store.totalCount).toBe(150)
      expect(store.currentPage).toBe(2)
      expect(store.totalPages).toBe(3)
      expect(store.hasNextPage).toBe(true)
      expect(store.hasPreviousPage).toBe(true)
    })
  })
})
