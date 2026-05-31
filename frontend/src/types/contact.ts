/**
 * 連絡先関連の型定義
 */

export interface ContactDto {
  id: string
  userId: string
  name: string
  address?: string
  phoneNumber?: string
  createdAt: string
  updatedAt: string
}

export interface CreateContactRequest {
  name: string
  address?: string
  phoneNumber?: string
}

export interface UpdateContactRequest {
  name: string
  address?: string
  phoneNumber?: string
}

export interface ContactListResponse {
  contacts: ContactDto[]
  pagination: PaginationDto
}

export interface PaginationDto {
  currentPage: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

export interface ContactErrorResponse {
  message: string
  errors?: ValidationError[]
}

export interface ValidationError {
  field: string
  message: string
}
