import axios, { type AxiosInstance, type InternalAxiosRequestConfig, type AxiosError } from 'axios'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000'
const API_TIMEOUT = Number(import.meta.env.VITE_API_TIMEOUT) || 10000
const DEBUG_MODE = import.meta.env.VITE_DEBUG_MODE === 'true'

// エラーレスポンスの型定義
export interface ApiErrorResponse {
  message?: string
  errors?: Record<string, string[]>
  errorId?: string
  timestamp?: string
}

// Create axios instance
const apiClient: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: API_TIMEOUT,
  withCredentials: false, // CORS設定に応じて調整
})

// Request interceptor to add JWT token
apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = localStorage.getItem('auth_token')
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`
    }
    
    if (DEBUG_MODE) {
      console.log('[API Request]', config.method?.toUpperCase(), config.url)
    }
    
    return config
  },
  (error) => {
    console.error('Request interceptor error:', error)
    return Promise.reject(error)
  }
)

// Response interceptor for error handling
apiClient.interceptors.response.use(
  (response) => {
    if (DEBUG_MODE) {
      console.log('[API Response]', response.status, response.config.url)
    }
    return response
  },
  (error: AxiosError<ApiErrorResponse>) => {
    // ネットワークエラー（サーバーに到達できない）
    if (!error.response) {
      console.error('Network error:', error.message)
      return Promise.reject({
        message: 'サーバーに接続できません。ネットワーク接続を確認してください。',
        originalError: error,
      })
    }

    const { status, data } = error.response

    if (DEBUG_MODE) {
      console.error('[API Error]', status, error.config?.url, data)
    }

    // 401 Unauthorized - トークン期限切れまたは無効
    if (status === 401) {
      console.warn('Authentication error: Token expired or invalid')
      
      // 認証状態をクリア
      localStorage.removeItem('auth_token')
      localStorage.removeItem('auth_user')
      localStorage.removeItem('auth_expires_at')
      
      // ログインページにリダイレクト（ログインページ以外の場合）
      if (window.location.pathname !== '/login' && window.location.pathname !== '/register') {
        window.location.href = '/login'
      }
    }

    // 403 Forbidden - アクセス権限なし
    if (status === 403) {
      console.error('Authorization error: Access forbidden')
    }

    // 404 Not Found
    if (status === 404) {
      console.error('Resource not found:', error.config?.url)
    }

    // 422 Unprocessable Entity - バリデーションエラー
    if (status === 422 && data?.errors) {
      console.warn('Validation errors:', data.errors)
    }

    // 429 Too Many Requests - レート制限
    if (status === 429) {
      console.warn('Rate limit exceeded')
    }

    // 500 Internal Server Error
    if (status >= 500) {
      console.error('Server error:', status, data)
    }

    // エラーオブジェクトを整形して返す
    return Promise.reject({
      status,
      message: data?.message || 'エラーが発生しました',
      errors: data?.errors,
      errorId: data?.errorId,
      originalError: error,
    })
  }
)

export default apiClient
