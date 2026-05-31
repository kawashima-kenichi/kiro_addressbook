import { describe, it, expect, beforeEach, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { createRouter, createMemoryHistory } from 'vue-router'
import type { Router } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

// Create a test router with the same configuration as the main router
function createTestRouter(): Router {
  const routes = [
    {
      path: '/login',
      name: 'login',
      component: { template: '<div>Login</div>' },
      meta: { requiresAuth: false, publicOnly: true },
    },
    {
      path: '/register',
      name: 'register',
      component: { template: '<div>Register</div>' },
      meta: { requiresAuth: false, publicOnly: true },
    },
    {
      path: '/',
      name: 'home',
      component: { template: '<div>Home</div>' },
      meta: { requiresAuth: true },
    },
  ]

  const router = createRouter({
    history: createMemoryHistory(),
    routes,
  })

  // Add the same navigation guard as the main router
  router.beforeEach((to, from, next) => {
    const authStore = useAuthStore()

    const requiresAuth = to.matched.some((record) => record.meta.requiresAuth)
    const publicOnly = to.matched.some((record) => record.meta.publicOnly)

    const isAuthenticated = authStore.isAuthenticated

    if (requiresAuth && !isAuthenticated) {
      console.log('未認証ユーザーがアクセスしようとしました。ログインページにリダイレクトします。')
      next({
        name: 'login',
        query: { redirect: to.fullPath },
      })
    } else if (publicOnly && isAuthenticated) {
      console.log('認証済みユーザーが公開ページにアクセスしようとしました。ホームにリダイレクトします。')
      next({ name: 'home' })
    } else {
      next()
    }
  })

  return router
}

describe('Router Navigation Guards', () => {
  let router: Router

  beforeEach(() => {
    setActivePinia(createPinia())
    router = createTestRouter()
  })

  describe('認証必須ルート', () => {
    it('未認証ユーザーはログインページにリダイレクトされる', async () => {
      const authStore = useAuthStore()
      
      // Mock unauthenticated state
      authStore.token = null
      authStore.user = null
      authStore.expiresAt = null

      await router.push('/')

      // Should redirect to login with redirect query
      expect(router.currentRoute.value.name).toBe('login')
      expect(router.currentRoute.value.query.redirect).toBe('/')
    })

    it('認証済みユーザーは認証必須ルートにアクセスできる', async () => {
      const authStore = useAuthStore()
      
      // Mock authenticated state
      authStore.token = 'valid-token'
      authStore.user = { id: '1', email: 'test@example.com' }
      authStore.expiresAt = new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString()

      await router.push('/')

      // Should access home page
      expect(router.currentRoute.value.name).toBe('home')
    })
  })

  describe('公開専用ルート', () => {
    it('認証済みユーザーはログインページからホームにリダイレクトされる', async () => {
      const authStore = useAuthStore()
      
      // Mock authenticated state
      authStore.token = 'valid-token'
      authStore.user = { id: '1', email: 'test@example.com' }
      authStore.expiresAt = new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString()

      await router.push('/login')

      // Should redirect to home
      expect(router.currentRoute.value.name).toBe('home')
    })

    it('認証済みユーザーは登録ページからホームにリダイレクトされる', async () => {
      const authStore = useAuthStore()
      
      // Mock authenticated state
      authStore.token = 'valid-token'
      authStore.user = { id: '1', email: 'test@example.com' }
      authStore.expiresAt = new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString()

      await router.push('/register')

      // Should redirect to home
      expect(router.currentRoute.value.name).toBe('home')
    })

    it('未認証ユーザーは公開ルートにアクセスできる', async () => {
      const authStore = useAuthStore()
      
      // Mock unauthenticated state
      authStore.token = null
      authStore.user = null
      authStore.expiresAt = null

      await router.push('/login')

      // Should access login page
      expect(router.currentRoute.value.name).toBe('login')
    })
  })

  describe('リダイレクトクエリパラメータ', () => {
    it('未認証ユーザーが保護されたルートにアクセスすると、リダイレクト先が保存される', async () => {
      const authStore = useAuthStore()
      
      // Mock unauthenticated state
      authStore.token = null
      authStore.user = null
      authStore.expiresAt = null

      await router.push('/')

      // Should redirect to login with redirect query
      expect(router.currentRoute.value.name).toBe('login')
      expect(router.currentRoute.value.query.redirect).toBe('/')
    })
  })
})
