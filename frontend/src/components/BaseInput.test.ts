import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import BaseInput from './BaseInput.vue'

describe('BaseInput.vue', () => {
  describe('基本機能', () => {
    it('デフォルトのpropsでレンダリングされる', () => {
      const wrapper = mount(BaseInput, {
        props: {
          modelValue: '',
        },
      })

      expect(wrapper.find('input').exists()).toBe(true)
      expect(wrapper.find('input').attributes('type')).toBe('text')
    })

    it('modelValueが正しく表示される', () => {
      const wrapper = mount(BaseInput, {
        props: {
          modelValue: 'テスト値',
        },
      })

      expect((wrapper.find('input').element as HTMLInputElement).value).toBe('テスト値')
    })

    it('入力時にupdate:modelValueイベントが発火される', async () => {
      const wrapper = mount(BaseInput, {
        props: {
          modelValue: '',
        },
      })

      const input = wrapper.find('input')
      await input.setValue('新しい値')

      expect(wrapper.emitted('update:modelValue')).toBeTruthy()
      expect(wrapper.emitted('update:modelValue')?.[0]).toEqual(['新しい値'])
    })
  })

  describe('ラベル', () => {
    it('labelプロパティが表示される', () => {
      const wrapper = mount(BaseInput, {
        props: {
          modelValue: '',
          label: 'テストラベル',
        },
      })

      expect(wrapper.find('label').text()).toContain('テストラベル')
    })

    it('requiredプロパティで必須マークが表示される', () => {
      const wrapper = mount(BaseInput, {
        props: {
          modelValue: '',
          label: 'テストラベル',
          required: true,
        },
      })

      expect(wrapper.find('label').html()).toContain('*')
      expect(wrapper.find('label').html()).toContain('aria-label="必須"')
    })
  })

  describe('入力タイプ', () => {
    it('type="email"が適用される', () => {
      const wrapper = mount(BaseInput, {
        props: {
          modelValue: '',
          type: 'email',
        },
      })

      expect(wrapper.find('input').attributes('type')).toBe('email')
    })

    it('type="password"が適用される', () => {
      const wrapper = mount(BaseInput, {
        props: {
          modelValue: '',
          type: 'password',
        },
      })

      expect(wrapper.find('input').attributes('type')).toBe('password')
    })

    it('type="tel"が適用される', () => {
      const wrapper = mount(BaseInput, {
        props: {
          modelValue: '',
          type: 'tel',
        },
      })

      expect(wrapper.find('input').attributes('type')).toBe('tel')
    })
  })

  describe('プレースホルダー', () => {
    it('placeholderプロパティが適用される', () => {
      const wrapper = mount(BaseInput, {
        props: {
          modelValue: '',
          placeholder: 'テストプレースホルダー',
        },
      })

      expect(wrapper.find('input').attributes('placeholder')).toBe('テストプレースホルダー')
    })
  })

  describe('エラー表示', () => {
    it('errorプロパティでエラーメッセージが表示される', () => {
      const wrapper = mount(BaseInput, {
        props: {
          modelValue: '',
          error: 'エラーメッセージ',
        },
      })

      expect(wrapper.find('p[role="alert"]').text()).toBe('エラーメッセージ')
    })

    it('エラー時にエラーアイコンが表示される', () => {
      const wrapper = mount(BaseInput, {
        props: {
          modelValue: '',
          error: 'エラーメッセージ',
        },
      })

      expect(wrapper.find('svg.text-red-500').exists()).toBe(true)
    })

    it('エラー時にaria-invalid属性が設定される', () => {
      const wrapper = mount(BaseInput, {
        props: {
          modelValue: '',
          error: 'エラーメッセージ',
        },
      })

      expect(wrapper.find('input').attributes('aria-invalid')).toBe('true')
    })

    it('エラー時にaria-describedby属性が設定される', () => {
      const wrapper = mount(BaseInput, {
        props: {
          modelValue: '',
          error: 'エラーメッセージ',
        },
      })

      const input = wrapper.find('input')
      const errorId = input.attributes('aria-describedby')
      expect(errorId).toBeDefined()
      expect(wrapper.find(`#${errorId}`).exists()).toBe(true)
    })

    it('エラー時に入力フィールドのスタイルが変わる', () => {
      const wrapper = mount(BaseInput, {
        props: {
          modelValue: '',
          error: 'エラーメッセージ',
        },
      })

      expect(wrapper.find('input').classes()).toContain('ring-red-300')
    })
  })

  describe('disabled状態', () => {
    it('disabledプロパティが適用される', () => {
      const wrapper = mount(BaseInput, {
        props: {
          modelValue: '',
          disabled: true,
        },
      })

      expect(wrapper.find('input').attributes('disabled')).toBeDefined()
    })

    it('disabled状態でスタイルが変わる', () => {
      const wrapper = mount(BaseInput, {
        props: {
          modelValue: '',
          disabled: true,
        },
      })

      expect(wrapper.find('input').classes()).toContain('disabled:cursor-not-allowed')
      expect(wrapper.find('input').classes()).toContain('disabled:bg-gray-50')
    })
  })

  describe('文字数制限', () => {
    it('maxlengthプロパティが適用される', () => {
      const wrapper = mount(BaseInput, {
        props: {
          modelValue: '',
          maxlength: 100,
        },
      })

      expect(wrapper.find('input').attributes('maxlength')).toBe('100')
    })

    it('maxlength設定時に文字数カウンターが表示される', () => {
      const wrapper = mount(BaseInput, {
        props: {
          modelValue: 'テスト',
          maxlength: 100,
        },
      })

      expect(wrapper.text()).toContain('3 / 100')
    })

    it('エラー時は文字数カウンターが表示されない', () => {
      const wrapper = mount(BaseInput, {
        props: {
          modelValue: 'テスト',
          maxlength: 100,
          error: 'エラー',
        },
      })

      expect(wrapper.text()).not.toContain('3 / 100')
    })
  })

  describe('イベント', () => {
    it('blurイベントが発火される', async () => {
      const wrapper = mount(BaseInput, {
        props: {
          modelValue: '',
        },
      })

      await wrapper.find('input').trigger('blur')
      expect(wrapper.emitted('blur')).toBeTruthy()
    })

    it('focusイベントが発火される', async () => {
      const wrapper = mount(BaseInput, {
        props: {
          modelValue: '',
        },
      })

      await wrapper.find('input').trigger('focus')
      expect(wrapper.emitted('focus')).toBeTruthy()
    })
  })

  describe('アクセシビリティ', () => {
    it('ラベルとinputがfor/id属性で関連付けられる', () => {
      const wrapper = mount(BaseInput, {
        props: {
          modelValue: '',
          label: 'テストラベル',
        },
      })

      const input = wrapper.find('input')
      const label = wrapper.find('label')
      const inputId = input.attributes('id')

      expect(inputId).toBeDefined()
      expect(label.attributes('for')).toBe(inputId)
    })

    it('カスタムIDが指定できる', () => {
      const wrapper = mount(BaseInput, {
        props: {
          modelValue: '',
          id: 'custom-id',
        },
      })

      expect(wrapper.find('input').attributes('id')).toBe('custom-id')
    })

    it('autocomplete属性が設定できる', () => {
      const wrapper = mount(BaseInput, {
        props: {
          modelValue: '',
          autocomplete: 'email',
        },
      })

      expect(wrapper.find('input').attributes('autocomplete')).toBe('email')
    })
  })
})
