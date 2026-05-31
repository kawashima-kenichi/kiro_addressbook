<!--
  AuthGuard コンポーネント
  認証が必要なコンテンツをラップし、セッション期限切れ時の自動リダイレクトを処理
-->
<script setup lang="ts">
import { computed, onMounted, onUnmounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const authStore = useAuthStore()

// 認証状態を監視
const isAuthenticated = computed(() => authStore.isAuthenticated)

// セッション期限切れチェックの間隔（ミリ秒）- 30秒ごと
const SESSION_CHECK_INTERVAL = 30 * 1000
let sessionCheckTimer: number | null = null

/**
 * セッション期限切れをチェックし、必要に応じてログインページにリダイレクト
 */
function checkSessionExpiration(): void {
  if (authStore.isTokenExpired() && authStore.token) {
    console.log('セッションが期限切れになりました。ログインページにリダイレクトします。')
    
    // ログアウト処理を実行
    authStore.logout().then(() => {
      // ログインページにリダイレクト
      router.push({
        name: 'login',
        query: { expired: 'true' },
      })
    })
  }
}

/**
 * セッション期限切れチェックを開始
 */
function startSessionCheck(): void {
  // 既存のタイマーをクリア
  if (sessionCheckTimer !== null) {
    clearInterval(sessionCheckTimer)
  }

  // 定期的にセッションの有効期限をチェック
  sessionCheckTimer = window.setInterval(() => {
    checkSessionExpiration()
  }, SESSION_CHECK_INTERVAL)
}

/**
 * セッション期限切れチェックを停止
 */
function stopSessionCheck(): void {
  if (sessionCheckTimer !== null) {
    clearInterval(sessionCheckTimer)
    sessionCheckTimer = null
  }
}

// コンポーネントマウント時にセッションチェックを開始
onMounted(() => {
  // 初回チェック
  checkSessionExpiration()
  
  // 定期チェックを開始
  if (isAuthenticated.value) {
    startSessionCheck()
  }
})

// コンポーネントアンマウント時にタイマーをクリア
onUnmounted(() => {
  stopSessionCheck()
})

// 認証状態の変化を監視
watch(isAuthenticated, (newValue) => {
  if (newValue) {
    // 認証された場合、セッションチェックを開始
    startSessionCheck()
  } else {
    // 認証が解除された場合、セッションチェックを停止
    stopSessionCheck()
    
    // ログインページにリダイレクト（既にログインページにいない場合）
    if (router.currentRoute.value.name !== 'login') {
      router.push({ name: 'login' })
    }
  }
})
</script>

<template>
  <div v-if="isAuthenticated">
    <!-- 認証済みの場合、スロットのコンテンツを表示 -->
    <slot />
  </div>
  <div v-else class="flex min-h-screen items-center justify-center">
    <!-- 未認証の場合、ローディング表示（リダイレクト中） -->
    <div class="text-center">
      <p class="text-gray-600">認証を確認しています...</p>
    </div>
  </div>
</template>
