/**
 * 連絡先APIサービス
 * 連絡先のCRUD操作とページネーション機能を提供
 */

import apiClient from './api'
import type {
  ContactDto,
  CreateContactRequest,
  UpdateContactRequest,
  ContactListResponse,
} from '@/types/contact'

export const contactService = {
  /**
   * 連絡先一覧を取得（ページネーション付き）
   * @param page ページ番号（1から開始）
   * @param limit 1ページあたりの件数（デフォルト: 50）
   */
  async getContacts(page: number = 1, limit: number = 50): Promise<ContactListResponse> {
    const response = await apiClient.get<ContactListResponse>('/api/contacts', {
      params: { page, limit },
    })
    return response.data
  },

  /**
   * 連絡先詳細を取得
   * @param id 連絡先ID
   */
  async getContactById(id: string): Promise<ContactDto> {
    const response = await apiClient.get<ContactDto>(`/api/contacts/${id}`)
    return response.data
  },

  /**
   * 連絡先を作成
   * @param data 連絡先作成データ
   */
  async createContact(data: CreateContactRequest): Promise<ContactDto> {
    const response = await apiClient.post<{ contact: ContactDto }>('/api/contacts', data)
    return response.data.contact
  },

  /**
   * 連絡先を更新
   * @param id 連絡先ID
   * @param data 連絡先更新データ
   */
  async updateContact(id: string, data: UpdateContactRequest): Promise<ContactDto> {
    const response = await apiClient.put<{ contact: ContactDto }>(`/api/contacts/${id}`, data)
    return response.data.contact
  },

  /**
   * 連絡先を削除
   * @param id 連絡先ID
   */
  async deleteContact(id: string): Promise<void> {
    await apiClient.delete(`/api/contacts/${id}`)
  },
}
