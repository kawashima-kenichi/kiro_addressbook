import { describe, it, expect, beforeEach, afterEach } from 'vitest'
import { mount } from '@vue/test-utils'
import BaseModal from './BaseModal.vue'

describe('BaseModal.vue', () => {
  beforeEach(() => {
    // Create a target element for Teleport
    const el = document.createElement('div')
    el.id = 'modal-target'
    document.body.appendChild(el)
  })

  afterEach(() => {
    // Clean up the target element
    const el = document.getElementById('modal-target')
    if (el) {
      document.body.removeChild(el)
    }
  })

  it('モーダルが開いているときに表示される', async () => {
    const wrapper = mount(BaseModal, {
      props: {
        isOpen: true,
        message: 'テストメッセージ',
      },
      attachTo: document.body,
    })

    await wrapper.vm.$nextTick()

    expect(document.querySelector('[role="dialog"]')).toBeTruthy()
    expect(document.body.textContent).toContain('テストメッセージ')
    
    wrapper.unmount()
  })

  it('モーダルが閉じているときに表示されない', async () => {
    const wrapper = mount(BaseModal, {
      props: {
        isOpen: false,
        message: 'テストメッセージ',
      },
      attachTo: document.body,
    })

    await wrapper.vm.$nextTick()

    expect(document.querySelector('[role="dialog"]')).toBeFalsy()
    
    wrapper.unmount()
  })

  it('カスタムタイトルとボタンテキストを表示する', async () => {
    const wrapper = mount(BaseModal, {
      props: {
        isOpen: true,
        title: 'カスタムタイトル',
        message: 'テストメッセージ',
        confirmText: 'はい',
        cancelText: 'いいえ',
      },
      attachTo: document.body,
    })

    await wrapper.vm.$nextTick()

    expect(document.body.textContent).toContain('カスタムタイトル')
    const confirmButton = document.querySelector('[data-testid="confirm-button"]')
    const cancelButton = document.querySelector('[data-testid="cancel-button"]')
    expect(confirmButton?.textContent?.trim()).toBe('はい')
    expect(cancelButton?.textContent?.trim()).toBe('いいえ')
    
    wrapper.unmount()
  })

  it('確認ボタンをクリックするとconfirmイベントが発火される', async () => {
    const wrapper = mount(BaseModal, {
      props: {
        isOpen: true,
        message: 'テストメッセージ',
      },
      attachTo: document.body,
    })

    await wrapper.vm.$nextTick()

    const confirmButton = document.querySelector('[data-testid="confirm-button"]') as HTMLElement
    confirmButton?.click()
    await wrapper.vm.$nextTick()

    expect(wrapper.emitted('confirm')).toBeTruthy()
    expect(wrapper.emitted('close')).toBeTruthy()
    
    wrapper.unmount()
  })

  it('キャンセルボタンをクリックするとcancelイベントが発火される', async () => {
    const wrapper = mount(BaseModal, {
      props: {
        isOpen: true,
        message: 'テストメッセージ',
      },
      attachTo: document.body,
    })

    await wrapper.vm.$nextTick()

    const cancelButton = document.querySelector('[data-testid="cancel-button"]') as HTMLElement
    cancelButton?.click()
    await wrapper.vm.$nextTick()

    expect(wrapper.emitted('cancel')).toBeTruthy()
    expect(wrapper.emitted('close')).toBeTruthy()
    
    wrapper.unmount()
  })

  it('背景オーバーレイをクリックするとcancelイベントが発火される', async () => {
    const wrapper = mount(BaseModal, {
      props: {
        isOpen: true,
        message: 'テストメッセージ',
      },
      attachTo: document.body,
    })

    await wrapper.vm.$nextTick()

    const overlay = document.querySelector('.bg-gray-500') as HTMLElement
    overlay?.click()
    await wrapper.vm.$nextTick()

    expect(wrapper.emitted('cancel')).toBeTruthy()
    expect(wrapper.emitted('close')).toBeTruthy()
    
    wrapper.unmount()
  })

  it('タイムアウトが設定されている', async () => {
    const wrapper = mount(BaseModal, {
      props: {
        isOpen: true,
        message: 'テストメッセージ',
        autoCloseTimeout: 30000,
      },
      attachTo: document.body,
    })

    await wrapper.vm.$nextTick()

    // タイムアウトが設定されていることを確認（カウントダウンが表示される）
    expect(document.body.textContent).toContain('秒後に自動的に閉じます')
    
    wrapper.unmount()
  })

  it('カウントダウンタイマーが表示される', async () => {
    const wrapper = mount(BaseModal, {
      props: {
        isOpen: true,
        message: 'テストメッセージ',
        autoCloseTimeout: 30000,
      },
      attachTo: document.body,
    })

    await wrapper.vm.$nextTick()

    // 初期状態では30秒
    expect(document.body.textContent).toContain('30秒後に自動的に閉じます')
    
    wrapper.unmount()
  })

  it('カスタムタイムアウト時間が設定できる', async () => {
    const wrapper = mount(BaseModal, {
      props: {
        isOpen: true,
        message: 'テストメッセージ',
        autoCloseTimeout: 5000, // 5秒
      },
      attachTo: document.body,
    })

    await wrapper.vm.$nextTick()

    // 初期状態では5秒
    expect(document.body.textContent).toContain('5秒後に自動的に閉じます')
    
    wrapper.unmount()
  })

  it('モーダルが閉じられたときにタイマーがクリアされる', async () => {
    const wrapper = mount(BaseModal, {
      props: {
        isOpen: true,
        message: 'テストメッセージ',
        autoCloseTimeout: 100,
      },
      attachTo: document.body,
    })

    await wrapper.vm.$nextTick()

    // モーダルを閉じる
    await wrapper.setProps({ isOpen: false })
    await wrapper.vm.$nextTick()

    // Wait longer than timeout
    await new Promise(resolve => setTimeout(resolve, 150))
    await wrapper.vm.$nextTick()

    // イベントは発火されない（タイマーがクリアされた）
    expect(wrapper.emitted('cancel')).toBeFalsy()
    
    wrapper.unmount()
  })

  it('ボタンをクリックするとモーダルが閉じる', async () => {
    const wrapper = mount(BaseModal, {
      props: {
        isOpen: true,
        message: 'テストメッセージ',
      },
      attachTo: document.body,
    })

    await wrapper.vm.$nextTick()

    // 確認ボタンをクリック
    const confirmButton = document.querySelector('[data-testid="confirm-button"]') as HTMLElement
    confirmButton?.click()
    await wrapper.vm.$nextTick()

    // イベントが発火されることを確認
    expect(wrapper.emitted('confirm')).toBeTruthy()
    expect(wrapper.emitted('close')).toBeTruthy()
    
    wrapper.unmount()
  })
})
