/**
 * Pinia認証ストア
 * ユーザー認証状態の管理、トークン有効期限チェック、自動ログアウト
 */

import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authService } from '@/services/authService'
import type {
  LoginRequest,
  RegisterRequest,
  UserDto,
  AuthErrorResponse,
} from '@/types/auth'

// ローカルストレージのキー
const TOKEN_KEY = 'auth_token'
const USER_KEY = 'auth_user'
const EXPIRES_AT_KEY = 'auth_expires_at'

// トークン有効期限チェックの間隔（ミリ秒）- 1分ごと
const TOKEN_CHECK_INTERVAL = 60 * 1000

export const useAuthStore = defineStore('auth', () => {
  // State
  const token = ref<string | null>(null)
  const user = ref<UserDto | null>(null)
  const expiresAt = ref<string | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  // トークン有効期限チェック用のタイマーID
  let tokenCheckTimer: number | null = null

  // Getters
  const isAuthenticated = computed(() => {
    return !!token.value && !!user.value && !isTokenExpired()
  })

  /**
   * トークンが期限切れかどうかをチェック
   */
  function isTokenExpired(): boolean {
    if (!expiresAt.value) {
      return true
    }

    const expirationTime = new Date(expiresAt.value).getTime()
    const currentTime = Date.now()

    return currentTime >= expirationTime
  }

  /**
   * ローカルストレージから認証情報を読み込む
   */
  function loadFromStorage(): void {
    const storedToken = localStorage.getItem(TOKEN_KEY)
    const storedUser = localStorage.getItem(USER_KEY)
    const storedExpiresAt = localStorage.getItem(EXPIRES_AT_KEY)

    if (storedToken && storedUser && storedExpiresAt) {
      token.value = storedToken
      user.value = JSON.parse(storedUser)
      expiresAt.value = storedExpiresAt

      // トークンが期限切れの場合、自動ログアウト
      if (isTokenExpired()) {
        logout()
      } else {
        // トークン有効期限チェックを開始
        startTokenExpirationCheck()
      }
    }
  }

  /**
   * ローカルストレージに認証情報を保存
   */
  function saveToStorage(): void {
    if (token.value && user.value && expiresAt.value) {
      localStorage.setItem(TOKEN_KEY, token.value)
      localStorage.setItem(USER_KEY, JSON.stringify(user.value))
      localStorage.setItem(EXPIRES_AT_KEY, expiresAt.value)
    }
  }

  /**
   * ローカルストレージから認証情報をクリア
   */
  function clearStorage(): void {
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(USER_KEY)
    localStorage.removeItem(EXPIRES_AT_KEY)
  }

  /**
   * トークン有効期限チェックを開始
   */
  function startTokenExpirationCheck(): void {
    // 既存のタイマーをクリア
    if (tokenCheckTimer !== null) {
      clearInterval(tokenCheckTimer)
    }

    // 定期的にトークンの有効期限をチェック
    tokenCheckTimer = window.setInterval(() => {
      if (isTokenExpired()) {
        console.log('トークンが期限切れです。自動ログアウトします。')
        logout()
      }
    }, TOKEN_CHECK_INTERVAL)
  }

  /**
   * トークン有効期限チェックを停止
   */
  function stopTokenExpirationCheck(): void {
    if (tokenCheckTimer !== null) {
      clearInterval(tokenCheckTimer)
      tokenCheckTimer = null
    }
  }

  /**
   * ログイン
   */
  async function login(credentials: LoginRequest): Promise<void> {
    loading.value = true
    error.value = null

    try {
      const response = await authService.login(credentials)

      // 認証情報を保存
      token.value = response.token
      user.value = response.user
      expiresAt.value = response.expiresAt

      // ローカルストレージに保存
      saveToStorage()

      // トークン有効期限チェックを開始
      startTokenExpirationCheck()
    } catch (err: any) {
      // エラーハンドリング
      if (err.response?.status === 423) {
        // アカウントロックエラー
        const errorData = err.response.data as AuthErrorResponse
        if (errorData.retryAfterSeconds) {
          const minutes = Math.ceil(errorData.retryAfterSeconds / 60)
          error.value = `アカウントがロックされています。${minutes}分後に再試行してください。`
        } else {
          error.value = errorData.message || 'アカウントがロックされています。'
        }
      } else if (err.response?.status === 401) {
        // 認証エラー
        const errorData = err.response.data as AuthErrorResponse
        error.value = errorData.message || 'ユーザー名またはパスワードが無効です'
      } else {
        // その他のエラー
        error.value = 'ログインに失敗しました。もう一度お試しください。'
      }
      throw err
    } finally {
      loading.value = false
    }
  }

  /**
   * ユーザー登録
   */
  async function register(userData: RegisterRequest): Promise<void> {
    loading.value = true
    error.value = null

    try {
      await authService.register(userData)
    } catch (err: any) {
      // エラーハンドリング
      if (err.response?.status === 400) {
        const errorData = err.response.data as AuthErrorResponse
        error.value = errorData.message || '登録に失敗しました'
      } else {
        error.value = 'アカウントを作成できません。もう一度お試しください。'
      }
      throw err
    } finally {
      loading.value = false
    }
  }

  /**
   * ログアウト
   */
  async function logout(): Promise<void> {
    try {
      // トークンが存在する場合のみ、サーバーにログアウトリクエストを送信
      if (token.value) {
        await authService.logout()
      }
    } catch (err) {
      // ログアウトAPIのエラーは無視（クライアント側のクリーンアップは実行）
      console.error('ログアウトAPIエラー:', err)
    } finally {
      // 認証情報をクリア
      token.value = null
      user.value = null
      expiresAt.value = null
      error.value = null

      // ローカルストレージをクリア
      clearStorage()

      // トークン有効期限チェックを停止
      stopTokenExpirationCheck()
    }
  }

  /**
   * エラーをクリア
   */
  function clearError(): void {
    error.value = null
  }

  // 初期化時にローカルストレージから認証情報を読み込む
  loadFromStorage()

  return {
    // State
    token,
    user,
    expiresAt,
    loading,
    error,

    // Getters
    isAuthenticated,

    // Actions
    login,
    register,
    logout,
    clearError,
    isTokenExpired,
  }
})
