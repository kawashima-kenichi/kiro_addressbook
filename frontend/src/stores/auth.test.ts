import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useAuthStore } from './auth'
import { authService } from '@/services/authService'
import type { LoginResponse, UserDto } from '@/types/auth'

// Mock authService
vi.mock('@/services/authService', () => ({
  authService: {
    login: vi.fn(),
    register: vi.fn(),
    logout: vi.fn(),
  },
}))

describe('useAuthStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    localStorage.clear()
    vi.useFakeTimers()
  })

  afterEach(() => {
    vi.useRealTimers()
  })

  describe('初期状態', () => {
    it('初期状態が正しく設定される', () => {
      const store = useAuthStore()

      expect(store.token).toBeNull()
      expect(store.user).toBeNull()
      expect(store.expiresAt).toBeNull()
      expect(store.loading).toBe(false)
      expect(store.error).toBeNull()
      expect(store.isAuthenticated).toBe(false)
    })
  })

  describe('login', () => {
    const mockUser: UserDto = {
      id: 'user-1',
      email: 'test@example.com',
    }

    const mockLoginResponse: LoginResponse = {
      token: 'test-token',
      user: mockUser,
      expiresAt: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString(), // 24時間後
    }

    it('正常にログインできる', async () => {
      vi.mocked(authService.login).mockResolvedValue(mockLoginResponse)

      const store = useAuthStore()
      await store.login({
        email: 'test@example.com',
        password: 'password123',
      })

      expect(store.token).toBe('test-token')
      expect(store.user).toEqual(mockUser)
      expect(store.expiresAt).toBe(mockLoginResponse.expiresAt)
      expect(store.loading).toBe(false)
      expect(store.error).toBeNull()
      expect(store.isAuthenticated).toBe(true)
    })

    it('ログイン情報をローカルストレージに保存する', async () => {
      vi.mocked(authService.login).mockResolvedValue(mockLoginResponse)

      const store = useAuthStore()
      await store.login({
        email: 'test@example.com',
        password: 'password123',
      })

      expect(localStorage.getItem('auth_token')).toBe('test-token')
      expect(localStorage.getItem('auth_user')).toBe(JSON.stringify(mockUser))
      expect(localStorage.getItem('auth_expires_at')).toBe(mockLoginResponse.expiresAt)
    })

    it('認証エラー時に適切なエラーメッセージを表示する', async () => {
      const error = {
        response: {
          status: 401,
          data: {
            message: 'ユーザー名またはパスワードが無効です',
          },
        },
      }
      vi.mocked(authService.login).mockRejectedValue(error)

      const store = useAuthStore()

      try {
        await store.login({
          email: 'test@example.com',
          password: 'wrong-password',
        })
      } catch (err) {
        // エラーが投げられることを期待
      }

      expect(store.error).toBe('ユーザー名またはパスワードが無効です')
      expect(store.loading).toBe(false)
      expect(store.isAuthenticated).toBe(false)
    })

    it('アカウントロック時に適切なエラーメッセージを表示する', async () => {
      const error = {
        response: {
          status: 423,
          data: {
            message: 'アカウントがロックされています',
            retryAfterSeconds: 1800, // 30分
          },
        },
      }
      vi.mocked(authService.login).mockRejectedValue(error)

      const store = useAuthStore()

      try {
        await store.login({
          email: 'test@example.com',
          password: 'password123',
        })
      } catch (err) {
        // エラーが投げられることを期待
      }

      expect(store.error).toBe('アカウントがロックされています。30分後に再試行してください。')
      expect(store.loading).toBe(false)
    })

    it('その他のエラー時に汎用エラーメッセージを表示する', async () => {
      vi.mocked(authService.login).mockRejectedValue(new Error('Network error'))

      const store = useAuthStore()

      try {
        await store.login({
          email: 'test@example.com',
          password: 'password123',
        })
      } catch (err) {
        // エラーが投げられることを期待
      }

      expect(store.error).toBe('ログインに失敗しました。もう一度お試しください。')
      expect(store.loading).toBe(false)
    })
  })

  describe('register', () => {
    it('正常にユーザー登録できる', async () => {
      vi.mocked(authService.register).mockResolvedValue()

      const store = useAuthStore()
      await store.register({
        email: 'newuser@example.com',
        password: 'Password123!',
      })

      expect(store.loading).toBe(false)
      expect(store.error).toBeNull()
    })

    it('バリデーションエラー時に適切なエラーメッセージを表示する', async () => {
      const error = {
        response: {
          status: 400,
          data: {
            message: '登録に失敗しました',
          },
        },
      }
      vi.mocked(authService.register).mockRejectedValue(error)

      const store = useAuthStore()

      try {
        await store.register({
          email: 'invalid-email',
          password: 'weak',
        })
      } catch (err) {
        // エラーが投げられることを期待
      }

      expect(store.error).toBe('登録に失敗しました')
      expect(store.loading).toBe(false)
    })

    it('その他のエラー時に汎用エラーメッセージを表示する', async () => {
      vi.mocked(authService.register).mockRejectedValue(new Error('Network error'))

      const store = useAuthStore()

      try {
        await store.register({
          email: 'newuser@example.com',
          password: 'Password123!',
        })
      } catch (err) {
        // エラーが投げられることを期待
      }

      expect(store.error).toBe('アカウントを作成できません。もう一度お試しください。')
      expect(store.loading).toBe(false)
    })
  })

  describe('logout', () => {
    it('正常にログアウトできる', async () => {
      vi.mocked(authService.logout).mockResolvedValue()

      const store = useAuthStore()
      // ログイン状態を設定
      store.token = 'test-token'
      store.user = { id: 'user-1', email: 'test@example.com' }
      store.expiresAt = new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString()
      localStorage.setItem('auth_token', 'test-token')
      localStorage.setItem('auth_user', JSON.stringify(store.user))
      localStorage.setItem('auth_expires_at', store.expiresAt)

      await store.logout()

      expect(store.token).toBeNull()
      expect(store.user).toBeNull()
      expect(store.expiresAt).toBeNull()
      expect(store.error).toBeNull()
      expect(store.isAuthenticated).toBe(false)
      expect(localStorage.getItem('auth_token')).toBeNull()
      expect(localStorage.getItem('auth_user')).toBeNull()
      expect(localStorage.getItem('auth_expires_at')).toBeNull()
    })

    it('ログアウトAPIエラー時もクライアント側のクリーンアップを実行する', async () => {
      vi.mocked(authService.logout).mockRejectedValue(new Error('API Error'))

      const store = useAuthStore()
      // ログイン状態を設定
      store.token = 'test-token'
      store.user = { id: 'user-1', email: 'test@example.com' }
      store.expiresAt = new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString()

      await store.logout()

      // エラーが発生してもクリーンアップは実行される
      expect(store.token).toBeNull()
      expect(store.user).toBeNull()
      expect(store.expiresAt).toBeNull()
      expect(store.isAuthenticated).toBe(false)
    })

    it('トークンがない場合はAPIを呼ばずにクリーンアップのみ実行する', async () => {
      const store = useAuthStore()
      store.token = null

      await store.logout()

      expect(authService.logout).not.toHaveBeenCalled()
      expect(store.isAuthenticated).toBe(false)
    })
  })

  describe('トークン有効期限チェック', () => {
    it('トークンが有効な場合はisAuthenticatedがtrueを返す', () => {
      const store = useAuthStore()
      store.token = 'test-token'
      store.user = { id: 'user-1', email: 'test@example.com' }
      store.expiresAt = new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString() // 24時間後

      expect(store.isAuthenticated).toBe(true)
    })

    it('トークンが期限切れの場合はisAuthenticatedがfalseを返す', () => {
      const store = useAuthStore()
      store.token = 'test-token'
      store.user = { id: 'user-1', email: 'test@example.com' }
      store.expiresAt = new Date(Date.now() - 1000).toISOString() // 1秒前（期限切れ）

      expect(store.isAuthenticated).toBe(false)
      expect(store.isTokenExpired()).toBe(true)
    })

    it('expiresAtがnullの場合はトークンが期限切れと判定される', () => {
      const store = useAuthStore()
      store.token = 'test-token'
      store.user = { id: 'user-1', email: 'test@example.com' }
      store.expiresAt = null

      expect(store.isTokenExpired()).toBe(true)
      expect(store.isAuthenticated).toBe(false)
    })
  })

  describe('ローカルストレージからの読み込み', () => {
    it('有効なトークンをローカルストレージから読み込む', () => {
      const mockUser: UserDto = {
        id: 'user-1',
        email: 'test@example.com',
      }
      const expiresAt = new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString()

      localStorage.setItem('auth_token', 'test-token')
      localStorage.setItem('auth_user', JSON.stringify(mockUser))
      localStorage.setItem('auth_expires_at', expiresAt)

      // 新しいストアインスタンスを作成（初期化時にローカルストレージから読み込む）
      const store = useAuthStore()

      expect(store.token).toBe('test-token')
      expect(store.user).toEqual(mockUser)
      expect(store.expiresAt).toBe(expiresAt)
      expect(store.isAuthenticated).toBe(true)
    })
  })

  describe('clearError', () => {
    it('エラーをクリアできる', () => {
      const store = useAuthStore()
      store.error = 'エラーメッセージ'

      store.clearError()

      expect(store.error).toBeNull()
    })
  })

  describe('トークン有効期限の自動チェック', () => {
    it('ログイン後にトークン有効期限チェックが開始される', async () => {
      const mockLoginResponse: LoginResponse = {
        token: 'test-token',
        user: { id: 'user-1', email: 'test@example.com' },
        expiresAt: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString(),
      }
      vi.mocked(authService.login).mockResolvedValue(mockLoginResponse)

      const store = useAuthStore()
      await store.login({
        email: 'test@example.com',
        password: 'password123',
      })

      expect(store.isAuthenticated).toBe(true)

      // 24時間後にトークンが期限切れになるようシミュレート
      store.expiresAt = new Date(Date.now() - 1000).toISOString()

      // 1分後（トークンチェック間隔）に進める
      vi.advanceTimersByTime(60 * 1000)

      // 期限切れのため自動ログアウトされる
      expect(store.isAuthenticated).toBe(false)
    })

    it('ログアウト時にトークン有効期限チェックが停止される', async () => {
      const mockLoginResponse: LoginResponse = {
        token: 'test-token',
        user: { id: 'user-1', email: 'test@example.com' },
        expiresAt: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString(),
      }
      vi.mocked(authService.login).mockResolvedValue(mockLoginResponse)
      vi.mocked(authService.logout).mockResolvedValue()

      const store = useAuthStore()
      await store.login({
        email: 'test@example.com',
        password: 'password123',
      })

      expect(store.isAuthenticated).toBe(true)

      await store.logout()

      expect(store.isAuthenticated).toBe(false)

      // タイマーが停止されているため、時間を進めても何も起こらない
      vi.advanceTimersByTime(60 * 1000)
    })
  })
})
