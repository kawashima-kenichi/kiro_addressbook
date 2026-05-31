import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import ContactList from './ContactList.vue'
import ContactCard from './ContactCard.vue'
import BasePagination from './BasePagination.vue'
import { useContactStore } from '@/stores/contact'
import type { ContactDto, PaginationDto } from '@/types/contact'

// Vue Routerのモック
vi.mock('vue-router', () => ({
  useRouter: () => ({
    push: vi.fn(),
  }),
}))

describe('ContactList.vue', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

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

  const mockPagination: PaginationDto = {
    currentPage: 1,
    pageSize: 50,
    totalCount: 2,
    totalPages: 1,
    hasNextPage: false,
    hasPreviousPage: false,
  }

  it('ローディング中はスピナーを表示する', async () => {
    const wrapper = mount(ContactList, {
      global: {
        stubs: {
          ContactCard: true,
          BasePagination: true,
        },
      },
    })

    const contactStore = useContactStore()
    contactStore.loading = true

    await wrapper.vm.$nextTick()

    expect(wrapper.find('[data-testid="loading-spinner"]').exists()).toBe(true)
  })

  it('連絡先がない場合は空状態メッセージを表示する', async () => {
    const wrapper = mount(ContactList, {
      global: {
        stubs: {
          ContactCard: true,
          BasePagination: true,
        },
      },
    })

    const contactStore = useContactStore()
    contactStore.contacts = []
    contactStore.loading = false
    contactStore.pagination = {
      ...mockPagination,
      totalCount: 0,
    }

    await wrapper.vm.$nextTick()

    expect(wrapper.find('[data-testid="empty-state"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="empty-state"]').text()).toContain(
      '連絡先が見つかりません'
    )
    expect(wrapper.find('[data-testid="empty-state"]').text()).toContain(
      '最初の連絡先を追加して始めましょう'
    )
  })

  it('エラーがある場合はエラーメッセージを表示する', async () => {
    const wrapper = mount(ContactList, {
      global: {
        stubs: {
          ContactCard: true,
          BasePagination: true,
        },
      },
    })

    const contactStore = useContactStore()
    contactStore.error = '連絡先を読み込めません。後でもう一度お試しください。'
    contactStore.loading = false

    await wrapper.vm.$nextTick()

    expect(wrapper.find('[data-testid="error-message"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="error-message"]').text()).toContain(
      '連絡先を読み込めません'
    )
  })

  it('連絡先がある場合はContactCardコンポーネントを表示する', async () => {
    const wrapper = mount(ContactList, {
      global: {
        components: {
          ContactCard,
          BasePagination,
        },
      },
    })

    const contactStore = useContactStore()
    contactStore.contacts = mockContacts
    contactStore.pagination = mockPagination
    contactStore.loading = false

    await wrapper.vm.$nextTick()

    const contactCards = wrapper.findAllComponents(ContactCard)
    expect(contactCards).toHaveLength(2)
  })

  it('連絡先数インジケーターを正しく表示する', async () => {
    const wrapper = mount(ContactList, {
      global: {
        stubs: {
          ContactCard: true,
          BasePagination: true,
        },
      },
    })

    const contactStore = useContactStore()
    contactStore.contacts = mockContacts
    contactStore.pagination = mockPagination
    contactStore.loading = false

    await wrapper.vm.$nextTick()

    expect(wrapper.find('[data-testid="contact-count"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="contact-count"]').text()).toBe('2件中1-2件を表示')
  })

  it('ページネーションが複数ページある場合はBasePaginationを表示する', async () => {
    const wrapper = mount(ContactList, {
      global: {
        components: {
          ContactCard,
          BasePagination,
        },
      },
    })

    const contactStore = useContactStore()
    contactStore.contacts = mockContacts
    contactStore.pagination = {
      ...mockPagination,
      totalPages: 3,
      hasNextPage: true,
    }
    contactStore.loading = false

    await wrapper.vm.$nextTick()

    expect(wrapper.findComponent(BasePagination).exists()).toBe(true)
  })

  it('ページネーションが1ページのみの場合はBasePaginationを表示しない', async () => {
    const wrapper = mount(ContactList, {
      global: {
        components: {
          ContactCard,
          BasePagination,
        },
      },
    })

    const contactStore = useContactStore()
    contactStore.contacts = mockContacts
    contactStore.pagination = mockPagination
    contactStore.loading = false

    await wrapper.vm.$nextTick()

    expect(wrapper.findComponent(BasePagination).exists()).toBe(false)
  })

  it('連絡先を追加ボタンをクリックできる', async () => {
    const wrapper = mount(ContactList, {
      global: {
        stubs: {
          ContactCard: true,
          BasePagination: true,
        },
      },
    })

    const contactStore = useContactStore()
    contactStore.loading = false

    await wrapper.vm.$nextTick()

    const addButton = wrapper.find('[data-testid="add-contact-button"]')
    expect(addButton.exists()).toBe(true)

    // クリックイベントをテスト（実際のルーティングはモックされている）
    await addButton.trigger('click')
  })

  it('連絡先数インジケーターが複数ページの場合に正しく計算される', async () => {
    const wrapper = mount(ContactList, {
      global: {
        stubs: {
          ContactCard: true,
          BasePagination: true,
        },
      },
    })

    const contactStore = useContactStore()
    contactStore.contacts = mockContacts
    contactStore.pagination = {
      currentPage: 2,
      pageSize: 50,
      totalCount: 120,
      totalPages: 3,
      hasNextPage: true,
      hasPreviousPage: true,
    }
    contactStore.loading = false

    await wrapper.vm.$nextTick()

    expect(wrapper.find('[data-testid="contact-count"]').text()).toBe('120件中51-100件を表示')
  })
})
