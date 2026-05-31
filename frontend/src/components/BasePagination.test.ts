import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import BasePagination from './BasePagination.vue'

describe('BasePagination.vue', () => {
  it('前へボタンと次へボタンを表示する', () => {
    const wrapper = mount(BasePagination, {
      props: {
        currentPage: 2,
        totalPages: 5,
        hasNextPage: true,
        hasPreviousPage: true,
      },
    })

    expect(wrapper.find('[data-testid="previous-button"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="next-button"]').exists()).toBe(true)
  })

  it('前のページがない場合、前へボタンを無効化する', () => {
    const wrapper = mount(BasePagination, {
      props: {
        currentPage: 1,
        totalPages: 5,
        hasNextPage: true,
        hasPreviousPage: false,
      },
    })

    const previousButton = wrapper.find('[data-testid="previous-button"]')
    expect(previousButton.attributes('disabled')).toBeDefined()
  })

  it('次のページがない場合、次へボタンを無効化する', () => {
    const wrapper = mount(BasePagination, {
      props: {
        currentPage: 5,
        totalPages: 5,
        hasNextPage: false,
        hasPreviousPage: true,
      },
    })

    const nextButton = wrapper.find('[data-testid="next-button"]')
    expect(nextButton.attributes('disabled')).toBeDefined()
  })

  it('ページ番号を正しく表示する（7ページ以下）', () => {
    const wrapper = mount(BasePagination, {
      props: {
        currentPage: 3,
        totalPages: 5,
        hasNextPage: true,
        hasPreviousPage: true,
      },
    })

    expect(wrapper.find('[data-testid="page-1"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="page-2"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="page-3"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="page-4"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="page-5"]').exists()).toBe(true)
  })

  it('ページ番号を省略表示する（7ページより多い場合）', () => {
    const wrapper = mount(BasePagination, {
      props: {
        currentPage: 5,
        totalPages: 10,
        hasNextPage: true,
        hasPreviousPage: true,
      },
    })

    expect(wrapper.find('[data-testid="page-1"]').exists()).toBe(true)
    expect(wrapper.findAll('[data-testid="page-ellipsis"]').length).toBeGreaterThan(0)
    expect(wrapper.find('[data-testid="page-10"]').exists()).toBe(true)
  })

  it('現在のページをハイライト表示する', () => {
    const wrapper = mount(BasePagination, {
      props: {
        currentPage: 3,
        totalPages: 5,
        hasNextPage: true,
        hasPreviousPage: true,
      },
    })

    const currentPageButton = wrapper.find('[data-testid="page-3"]')
    expect(currentPageButton.classes()).toContain('border-indigo-500')
    expect(currentPageButton.classes()).toContain('text-indigo-600')
  })

  it('前へボタンクリック時にprevious-pageイベントを発火する', async () => {
    const wrapper = mount(BasePagination, {
      props: {
        currentPage: 2,
        totalPages: 5,
        hasNextPage: true,
        hasPreviousPage: true,
      },
    })

    await wrapper.find('[data-testid="previous-button"]').trigger('click')

    expect(wrapper.emitted('previous-page')).toBeTruthy()
  })

  it('次へボタンクリック時にnext-pageイベントを発火する', async () => {
    const wrapper = mount(BasePagination, {
      props: {
        currentPage: 2,
        totalPages: 5,
        hasNextPage: true,
        hasPreviousPage: true,
      },
    })

    await wrapper.find('[data-testid="next-button"]').trigger('click')

    expect(wrapper.emitted('next-page')).toBeTruthy()
  })

  it('ページ番号クリック時にpage-changeイベントを発火する', async () => {
    const wrapper = mount(BasePagination, {
      props: {
        currentPage: 2,
        totalPages: 5,
        hasNextPage: true,
        hasPreviousPage: true,
      },
    })

    await wrapper.find('[data-testid="page-4"]').trigger('click')

    expect(wrapper.emitted('page-change')).toBeTruthy()
    expect(wrapper.emitted('page-change')?.[0]).toEqual([4])
  })

  it('省略記号（...）クリック時はイベントを発火しない', async () => {
    const wrapper = mount(BasePagination, {
      props: {
        currentPage: 5,
        totalPages: 10,
        hasNextPage: true,
        hasPreviousPage: true,
      },
    })

    const ellipsis = wrapper.find('[data-testid="page-ellipsis"]')
    await ellipsis.trigger('click')

    expect(wrapper.emitted('page-change')).toBeFalsy()
  })

  it('現在のページをクリックしてもイベントを発火しない', async () => {
    const wrapper = mount(BasePagination, {
      props: {
        currentPage: 3,
        totalPages: 5,
        hasNextPage: true,
        hasPreviousPage: true,
      },
    })

    await wrapper.find('[data-testid="page-3"]').trigger('click')

    expect(wrapper.emitted('page-change')).toBeFalsy()
  })
})
