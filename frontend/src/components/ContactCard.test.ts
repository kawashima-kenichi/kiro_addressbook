import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import ContactCard from './ContactCard.vue'
import type { ContactDto } from '@/types/contact'

describe('ContactCard.vue', () => {
  const mockContact: ContactDto = {
    id: '1',
    userId: 'user-1',
    name: '田中太郎',
    address: '東京都渋谷区',
    phoneNumber: '03-1234-5678',
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
  }

  it('連絡先情報を正しく表示する', () => {
    const wrapper = mount(ContactCard, {
      props: { contact: mockContact },
    })

    expect(wrapper.find('[data-testid="contact-name"]').text()).toBe('田中太郎')
    expect(wrapper.find('[data-testid="contact-phone"]').text()).toContain('03-1234-5678')
    expect(wrapper.find('[data-testid="contact-address"]').text()).toContain('東京都渋谷区')
  })

  it('電話番号がない場合は「電話番号なし」と表示する', () => {
    const contactWithoutPhone: ContactDto = {
      ...mockContact,
      phoneNumber: undefined,
    }

    const wrapper = mount(ContactCard, {
      props: { contact: contactWithoutPhone },
    })

    expect(wrapper.find('[data-testid="contact-phone"]').text()).toContain('電話番号なし')
  })

  it('住所がない場合は住所フィールドを表示しない', () => {
    const contactWithoutAddress: ContactDto = {
      ...mockContact,
      address: undefined,
    }

    const wrapper = mount(ContactCard, {
      props: { contact: contactWithoutAddress },
    })

    expect(wrapper.find('[data-testid="contact-address"]').exists()).toBe(false)
  })

  it('クリック時にclickイベントを発火する', async () => {
    const wrapper = mount(ContactCard, {
      props: { contact: mockContact },
    })

    await wrapper.find('[data-testid="contact-card"]').trigger('click')

    expect(wrapper.emitted('click')).toBeTruthy()
    expect(wrapper.emitted('click')?.[0]).toEqual([mockContact])
  })

  it('ホバー時にスタイルが変化する', () => {
    const wrapper = mount(ContactCard, {
      props: { contact: mockContact },
    })

    const card = wrapper.find('[data-testid="contact-card"]')
    expect(card.classes()).toContain('hover:border-indigo-500')
    expect(card.classes()).toContain('hover:shadow-md')
  })
})
