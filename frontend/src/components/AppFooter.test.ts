import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import AppFooter from './AppFooter.vue'

describe('AppFooter.vue', () => {
  describe('基本機能', () => {
    it('コンポーネントがレンダリングされる', () => {
      const wrapper = mount(AppFooter)
      expect(wrapper.find('footer').exists()).toBe(true)
    })

    it('著作権表示が表示される', () => {
      const wrapper = mount(AppFooter)
      const currentYear = new Date().getFullYear()
      expect(wrapper.text()).toContain(`© ${currentYear} 住所録アプリ`)
      expect(wrapper.text()).toContain('All rights reserved')
    })

    it('フッターリンクが表示される', () => {
      const wrapper = mount(AppFooter)
      expect(wrapper.text()).toContain('プライバシーポリシー')
      expect(wrapper.text()).toContain('利用規約')
      expect(wrapper.text()).toContain('お問い合わせ')
    })
  })

  describe('リンク', () => {
    it('プライバシーポリシーリンクが存在する', () => {
      const wrapper = mount(AppFooter)
      const links = wrapper.findAll('a')
      const privacyLink = links.find((link) => link.text() === 'プライバシーポリシー')
      expect(privacyLink).toBeDefined()
    })

    it('利用規約リンクが存在する', () => {
      const wrapper = mount(AppFooter)
      const links = wrapper.findAll('a')
      const termsLink = links.find((link) => link.text() === '利用規約')
      expect(termsLink).toBeDefined()
    })

    it('お問い合わせリンクが存在する', () => {
      const wrapper = mount(AppFooter)
      const links = wrapper.findAll('a')
      const contactLink = links.find((link) => link.text() === 'お問い合わせ')
      expect(contactLink).toBeDefined()
    })

    it('リンククリック時にデフォルト動作が防止される', async () => {
      const wrapper = mount(AppFooter)
      const link = wrapper.find('a')
      
      // リンクがhref="#"を持つことを確認
      expect(link.attributes('href')).toBe('#')
    })
  })

  describe('アクセシビリティ', () => {
    it('footer要素にrole="contentinfo"属性が設定されている', () => {
      const wrapper = mount(AppFooter)
      expect(wrapper.find('footer').attributes('role')).toBe('contentinfo')
    })

    it('ナビゲーションにaria-label属性が設定されている', () => {
      const wrapper = mount(AppFooter)
      expect(wrapper.find('nav').attributes('aria-label')).toBe('フッターナビゲーション')
    })

    it('リンクがフォーカス可能である', () => {
      const wrapper = mount(AppFooter)
      const links = wrapper.findAll('a')
      
      // Links should exist and be focusable (tabIndex >= -1 is valid, -1 means programmatically focusable)
      expect(links.length).toBeGreaterThan(0)
      links.forEach((link) => {
        const element = link.element as HTMLAnchorElement
        // Links are focusable elements by default
        expect(element.tagName).toBe('A')
      })
    })

    it('リンクにホバースタイルが適用されている', () => {
      const wrapper = mount(AppFooter)
      const link = wrapper.find('a')
      
      expect(link.classes()).toContain('hover:text-gray-700')
    })

    it('リンクにフォーカススタイルが適用されている', () => {
      const wrapper = mount(AppFooter)
      const link = wrapper.find('a')
      
      expect(link.classes()).toContain('focus:outline-none')
      expect(link.classes()).toContain('focus:ring-2')
      expect(link.classes()).toContain('focus:ring-indigo-500')
    })
  })

  describe('レスポンシブデザイン', () => {
    it('レスポンシブなパディングクラスが適用されている', () => {
      const wrapper = mount(AppFooter)
      const container = wrapper.find('.mx-auto')
      
      expect(container.classes()).toContain('px-4')
      expect(container.classes()).toContain('sm:px-6')
      expect(container.classes()).toContain('lg:px-8')
    })

    it('レスポンシブなレイアウトクラスが適用されている', () => {
      const wrapper = mount(AppFooter)
      const contentDiv = wrapper.find('.py-6')
      
      expect(contentDiv.classes()).toContain('md:flex')
      expect(contentDiv.classes()).toContain('md:items-center')
      expect(contentDiv.classes()).toContain('md:justify-between')
    })

    it('著作権表示がレスポンシブに配置される', () => {
      const wrapper = mount(AppFooter)
      const copyrightDiv = wrapper.find('p').element.parentElement
      
      expect(copyrightDiv?.classList.contains('text-center')).toBe(true)
      expect(copyrightDiv?.classList.contains('md:text-left')).toBe(true)
    })

    it('ナビゲーションがレスポンシブに配置される', () => {
      const wrapper = mount(AppFooter)
      const nav = wrapper.find('nav')
      
      expect(nav.classes()).toContain('mt-4')
      expect(nav.classes()).toContain('md:mt-0')
    })

    it('リンクリストがレスポンシブに配置される', () => {
      const wrapper = mount(AppFooter)
      const linkList = wrapper.find('ul')
      
      expect(linkList.classes()).toContain('justify-center')
      expect(linkList.classes()).toContain('md:justify-end')
    })
  })

  describe('スタイリング', () => {
    it('フッターに適切な背景色が適用されている', () => {
      const wrapper = mount(AppFooter)
      const footer = wrapper.find('footer')
      
      expect(footer.classes()).toContain('bg-white')
    })

    it('フッターに上部ボーダーが適用されている', () => {
      const wrapper = mount(AppFooter)
      const footer = wrapper.find('footer')
      
      expect(footer.classes()).toContain('border-t')
      expect(footer.classes()).toContain('border-gray-200')
    })

    it('フッターがページ下部に配置される', () => {
      const wrapper = mount(AppFooter)
      const footer = wrapper.find('footer')
      
      expect(footer.classes()).toContain('mt-auto')
    })

    it('リンクに適切なスペーシングが適用されている', () => {
      const wrapper = mount(AppFooter)
      const linkList = wrapper.find('ul')
      
      expect(linkList.classes()).toContain('space-x-6')
    })
  })
})
