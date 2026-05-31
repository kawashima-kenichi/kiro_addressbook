import { createRouter, createWebHistory } from 'vue-router'
import type { RouteRecordRaw } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const routes: RouteRecordRaw[] = [
  // 公開ルート（認証不要）
  {
    path: '/login',
    name: 'login',
    component: () => import('@/views/LoginView.vue'),
    meta: { requiresAuth: false, publicOnly: true },
  },
  {
    path: '/register',
    name: 'register',
    component: () => import('@/views/RegisterView.vue'),
    meta: { requiresAuth: false, publicOnly: true },
  },
  // 認証必須ルート
  {
    path: '/',
    name: 'home',
    component: () => import('@/views/ContactListView.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/contacts',
    name: 'contacts',
    component: () => import('@/views/ContactListView.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/contacts/new',
    name: 'contact-new',
    component: () => import('@/views/ContactCreateView.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/contacts/:id',
    name: 'contact-detail',
    component: () => import('@/views/ContactDetailView.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/contacts/:id/edit',
    name: 'contact-edit',
    component: () => import('@/views/ContactEditView.vue'),
    meta: { requiresAuth: true },
  },
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
})

/**
 * グローバルナビゲーションガード
 * 認証が必要なルートへのアクセスを制御
 */
router.beforeEach((to, _from, next) => {
  const authStore = useAuthStore()

  // ルートが認証を必要とするか確認
  const requiresAuth = to.matched.some((record) => record.meta.requiresAuth)
  // ルートが公開専用（認証済みユーザーはアクセス不可）か確認
  const publicOnly = to.matched.some((record) => record.meta.publicOnly)

  // 認証状態をチェック
  const isAuthenticated = authStore.isAuthenticated

  if (requiresAuth && !isAuthenticated) {
    // 認証が必要だが未認証の場合、ログインページにリダイレクト
    console.log('未認証ユーザーがアクセスしようとしました。ログインページにリダイレクトします。')
    next({
      name: 'login',
      query: { redirect: to.fullPath }, // リダイレクト先を保存
    })
  } else if (publicOnly && isAuthenticated) {
    // 公開専用ページに認証済みユーザーがアクセスした場合、ホームにリダイレクト
    console.log('認証済みユーザーが公開ページにアクセスしようとしました。ホームにリダイレクトします。')
    next({ name: 'home' })
  } else {
    // それ以外の場合は通常通り進む
    next()
  }
})

export default router
