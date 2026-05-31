import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import BaseButton from './BaseButton.vue'

describe('BaseButton.vue', () => {
  describe('基本機能', () => {
    it('デフォルトのpropsでレンダリングされる', () => {
      const wrapper = mount(BaseButton, {
        slots: {
          default: 'クリック',
        },
      })

      expect(wrapper.text()).toBe('クリック')
      expect(wrapper.attributes('type')).toBe('button')
      expect(wrapper.classes()).toContain('bg-indigo-600')
    })

    it('スロットコンテンツが正しく表示される', () => {
      const wrapper = mount(BaseButton, {
        slots: {
          default: 'テストボタン',
        },
      })

      expect(wrapper.text()).toBe('テストボタン')
    })

    it('クリックイベントが発火される', async () => {
      const wrapper = mount(BaseButton, {
        slots: {
          default: 'クリック',
        },
      })

      await wrapper.trigger('click')
      expect(wrapper.emitted('click')).toBeTruthy()
      expect(wrapper.emitted('click')?.[0]).toBeDefined()
    })
  })

  describe('バリアント', () => {
    it('primaryバリアントが適用される', () => {
      const wrapper = mount(BaseButton, {
        props: { variant: 'primary' },
        slots: { default: 'Primary' },
      })

      expect(wrapper.classes()).toContain('bg-indigo-600')
      expect(wrapper.classes()).toContain('text-white')
    })

    it('secondaryバリアントが適用される', () => {
      const wrapper = mount(BaseButton, {
        props: { variant: 'secondary' },
        slots: { default: 'Secondary' },
      })

      expect(wrapper.classes()).toContain('bg-gray-200')
      expect(wrapper.classes()).toContain('text-gray-900')
    })

    it('dangerバリアントが適用される', () => {
      const wrapper = mount(BaseButton, {
        props: { variant: 'danger' },
        slots: { default: 'Danger' },
      })

      expect(wrapper.classes()).toContain('bg-red-600')
      expect(wrapper.classes()).toContain('text-white')
    })

    it('ghostバリアントが適用される', () => {
      const wrapper = mount(BaseButton, {
        props: { variant: 'ghost' },
        slots: { default: 'Ghost' },
      })

      expect(wrapper.classes()).toContain('bg-transparent')
      expect(wrapper.classes()).toContain('text-gray-700')
    })
  })

  describe('サイズ', () => {
    it('smサイズが適用される', () => {
      const wrapper = mount(BaseButton, {
        props: { size: 'sm' },
        slots: { default: 'Small' },
      })

      expect(wrapper.classes()).toContain('px-3')
      expect(wrapper.classes()).toContain('py-1.5')
      expect(wrapper.classes()).toContain('text-sm')
    })

    it('mdサイズが適用される', () => {
      const wrapper = mount(BaseButton, {
        props: { size: 'md' },
        slots: { default: 'Medium' },
      })

      expect(wrapper.classes()).toContain('px-4')
      expect(wrapper.classes()).toContain('py-2')
      expect(wrapper.classes()).toContain('text-base')
    })

    it('lgサイズが適用される', () => {
      const wrapper = mount(BaseButton, {
        props: { size: 'lg' },
        slots: { default: 'Large' },
      })

      expect(wrapper.classes()).toContain('px-6')
      expect(wrapper.classes()).toContain('py-3')
      expect(wrapper.classes()).toContain('text-lg')
    })
  })

  describe('disabled状態', () => {
    it('disabledプロパティが適用される', () => {
      const wrapper = mount(BaseButton, {
        props: { disabled: true },
        slots: { default: 'Disabled' },
      })

      expect(wrapper.attributes('disabled')).toBeDefined()
      expect(wrapper.attributes('aria-disabled')).toBe('true')
    })

    it('disabled状態でクリックイベントが発火されない', async () => {
      const wrapper = mount(BaseButton, {
        props: { disabled: true },
        slots: { default: 'Disabled' },
      })

      await wrapper.trigger('click')
      expect(wrapper.emitted('click')).toBeFalsy()
    })
  })

  describe('loading状態', () => {
    it('loadingプロパティが適用される', () => {
      const wrapper = mount(BaseButton, {
        props: { loading: true },
        slots: { default: 'Loading' },
      })

      expect(wrapper.attributes('aria-busy')).toBe('true')
      expect(wrapper.find('svg.animate-spin').exists()).toBe(true)
    })

    it('loading状態でクリックイベントが発火されない', async () => {
      const wrapper = mount(BaseButton, {
        props: { loading: true },
        slots: { default: 'Loading' },
      })

      await wrapper.trigger('click')
      expect(wrapper.emitted('click')).toBeFalsy()
    })
  })

  describe('fullWidth', () => {
    it('fullWidthプロパティが適用される', () => {
      const wrapper = mount(BaseButton, {
        props: { fullWidth: true },
        slots: { default: 'Full Width' },
      })

      expect(wrapper.classes()).toContain('w-full')
    })
  })

  describe('type属性', () => {
    it('type="submit"が適用される', () => {
      const wrapper = mount(BaseButton, {
        props: { type: 'submit' },
        slots: { default: 'Submit' },
      })

      expect(wrapper.attributes('type')).toBe('submit')
    })

    it('type="reset"が適用される', () => {
      const wrapper = mount(BaseButton, {
        props: { type: 'reset' },
        slots: { default: 'Reset' },
      })

      expect(wrapper.attributes('type')).toBe('reset')
    })
  })

  describe('アクセシビリティ', () => {
    it('フォーカス可能である', () => {
      const wrapper = mount(BaseButton, {
        slots: { default: 'Focus' },
      })

      const button = wrapper.element as HTMLButtonElement
      // Buttons are focusable by default
      expect(button.tagName).toBe('BUTTON')
      expect(button.disabled).toBe(false)
    })

    it('disabled状態でaria-disabled属性が設定される', () => {
      const wrapper = mount(BaseButton, {
        props: { disabled: true },
        slots: { default: 'Disabled' },
      })

      expect(wrapper.attributes('aria-disabled')).toBe('true')
    })

    it('loading状態でaria-busy属性が設定される', () => {
      const wrapper = mount(BaseButton, {
        props: { loading: true },
        slots: { default: 'Loading' },
      })

      expect(wrapper.attributes('aria-busy')).toBe('true')
    })
  })
})
