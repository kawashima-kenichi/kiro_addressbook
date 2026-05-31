import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import ContactListView from './ContactListView.vue'
import { useContactStore } from '@/stores/contact'
import { useAuthStore } from '@/stores/auth'
import type { ContactDto } from '@/types/contact'

// Mock router
const mockPush = vi.fn()
vi.mock('vue-router', () => ({
  useRouter: () => ({
    push: mockPush,
  }),
}))

// Mock ContactCard component
vi.mock('@/components/ContactCard.vue', () => ({
  default: {
    name: 'ContactCard',
    props: ['contact'],
    emits: ['edit', 'delete', 'view'],
    template: '<div class="contact-card">{{ contact.name }}</div>',
  },
}))

const mockContacts: ContactDto[] = [
  {
    id: '1',
    userId: 'user-1',
    name: '田中太郎',
    address: '東京都渋谷区',
    phoneNumber: '03-1234-5678',
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
  },
  {
    id: '2',
    userId: 'user-1',
    name: '佐藤花子',
    phoneNumber: '090-1234-5678',
    createdAt: '2024-01-02T00:00:00Z',
    updatedAt: '2024-01-02T00:00:00Z',
  },
]

describe('ContactListView.vue', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    mockPush.mockClear()
    
    // Mock fetchContacts to prevent API calls on mount
    const contactStore = useContactStore()
    vi.spyOn(contactStore, 'fetchContacts').mockResolvedValue(undefined)
  })

  it('連絡先一覧を表示する', async () => {
    const contactStore = useContactStore()
    contactStore.contacts = mockContacts
    contactStore.pagination = {
      currentPage: 1,
      pageSize: 50,
      totalCount: 2,
      totalPages: 1,
      hasNextPage: false,
      hasPreviousPage: false,
    }
    contactStore.loading = false

    const wrapper = mount(ContactListView)
    await flushPromises()

    expect(wrapper.text()).toContain('田中太郎')
    expect(wrapper.text()).toContain('佐藤花子')
  })

  it('連絡先がない場合は空状態メッセージを表示する', async () => {
    const contactStore = useContactStore()
    contactStore.contacts = []
    contactStore.loading = false

    const wrapper = mount(ContactListView)
    await flushPromises()

    expect(wrapper.find('[data-testid="empty-state"]').exists()).toBe(true)
    expect(wrapper.text()).toContain('連絡先が見つかりません。最初の連絡先を追加して始めましょう。')
  })

  it('ローディング中はスピナーを表示する', async () => {
    const contactStore = useContactStore()
    contactStore.loading = true

    const wrapper = mount(ContactListView)
    await flushPromises()

    expect(wrapper.find('.animate-spin').exists()).toBe(true)
  })

  it('連絡先数インジケーターを表示する', async () => {
    const contactStore = useContactStore()
    contactStore.contacts = mockContacts
    contactStore.pagination = {
      currentPage: 1,
      pageSize: 50,
      totalCount: 2,
      totalPages: 1,
      hasNextPage: false,
      hasPreviousPage: false,
    }
    contactStore.loading = false

    const wrapper = mount(ContactListView)
    await flushPromises()

    expect(wrapper.text()).toContain('2件中1件〜2件を表示')
  })

  it('連絡先を削除すると成功メッセージが表示される', async () => {
    const contactStore = useContactStore()
    contactStore.contacts = mockContacts
    contactStore.pagination = {
      currentPage: 1,
      pageSize: 50,
      totalCount: 2,
      totalPages: 1,
      hasNextPage: false,
      hasPreviousPage: false,
    }
    contactStore.loading = false
    contactStore.deleteContact = vi.fn().mockResolvedValue(undefined)

    const wrapper = mount(ContactListView)
    await flushPromises()

    // 削除イベントを発火
    const contactCards = wrapper.findAllComponents({ name: 'ContactCard' })
    expect(contactCards.length).toBeGreaterThan(0)
    await contactCards[0].vm.$emit('delete', '1')
    await flushPromises()

    expect(wrapper.find('[data-testid="success-message"]').exists()).toBe(true)
    expect(wrapper.text()).toContain('連絡先田中太郎が正常に削除されました')
  })

  it('連絡先削除に失敗するとエラーメッセージが表示される', async () => {
    const contactStore = useContactStore()
    contactStore.contacts = mockContacts
    contactStore.pagination = {
      currentPage: 1,
      pageSize: 50,
      totalCount: 2,
      totalPages: 1,
      hasNextPage: false,
      hasPreviousPage: false,
    }
    contactStore.loading = false
    contactStore.deleteContact = vi.fn().mockRejectedValue(new Error('削除失敗'))
    contactStore.error = '連絡先を削除できません。もう一度お試しください。'

    const wrapper = mount(ContactListView)
    await flushPromises()

    // 削除イベントを発火
    const contactCards = wrapper.findAllComponents({ name: 'ContactCard' })
    expect(contactCards.length).toBeGreaterThan(0)
    await contactCards[0].vm.$emit('delete', '1')
    await flushPromises()

    expect(wrapper.find('[data-testid="error-message"]').exists()).toBe(true)
    expect(wrapper.text()).toContain('連絡先を削除できません。もう一度お試しください。')
  })

  it('連絡先追加ボタンをクリックすると新規作成ページに遷移する', async () => {
    const contactStore = useContactStore()
    contactStore.contacts = mockContacts
    contactStore.loading = false

    const wrapper = mount(ContactListView)
    await flushPromises()

    await wrapper.find('[data-testid="add-contact-button"]').trigger('click')

    expect(mockPush).toHaveBeenCalledWith('/contacts/new')
  })

  it('編集イベントを受け取ると編集ページに遷移する', async () => {
    const contactStore = useContactStore()
    contactStore.contacts = mockContacts
    contactStore.pagination = {
      currentPage: 1,
      pageSize: 50,
      totalCount: 2,
      totalPages: 1,
      hasNextPage: false,
      hasPreviousPage: false,
    }
    contactStore.loading = false

    const wrapper = mount(ContactListView)
    await flushPromises()

    // 編集イベントを発火
    const contactCards = wrapper.findAllComponents({ name: 'ContactCard' })
    expect(contactCards.length).toBeGreaterThan(0)
    await contactCards[0].vm.$emit('edit', '1')

    expect(mockPush).toHaveBeenCalledWith('/contacts/1/edit')
  })

  it('表示イベントを受け取ると詳細ページに遷移する', async () => {
    const contactStore = useContactStore()
    contactStore.contacts = mockContacts
    contactStore.pagination = {
      currentPage: 1,
      pageSize: 50,
      totalCount: 2,
      totalPages: 1,
      hasNextPage: false,
      hasPreviousPage: false,
    }
    contactStore.loading = false

    const wrapper = mount(ContactListView)
    await flushPromises()

    // 表示イベントを発火
    const contactCards = wrapper.findAllComponents({ name: 'ContactCard' })
    expect(contactCards.length).toBeGreaterThan(0)
    await contactCards[0].vm.$emit('view', '1')

    expect(mockPush).toHaveBeenCalledWith('/contacts/1')
  })

  it('ログアウトボタンをクリックするとログアウトしてログインページに遷移する', async () => {
    const authStore = useAuthStore()
    authStore.logout = vi.fn().mockResolvedValue(undefined)

    const contactStore = useContactStore()
    contactStore.contacts = mockContacts
    contactStore.loading = false

    const wrapper = mount(ContactListView)
    await flushPromises()

    // Find the logout button by its text content
    const buttons = wrapper.findAll('button')
    const logoutButton = buttons.find(btn => btn.text().includes('ログアウト'))
    expect(logoutButton).toBeDefined()
    
    await logoutButton!.trigger('click')
    await flushPromises()

    expect(authStore.logout).toHaveBeenCalled()
    expect(mockPush).toHaveBeenCalledWith('/login')
  })
})
