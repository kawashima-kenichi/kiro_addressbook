export interface LoginRequest {
  email: string
  password: string
}

export interface LoginResponse {
  token: string
  user: UserDto
  expiresAt: string
}

export interface RegisterRequest {
  email: string
  password: string
}

export interface RegisterResponse {
  success: boolean
  message: string
}

export interface LogoutResponse {
  success: boolean
  message: string
}

export interface AuthErrorResponse {
  message: string
  retryAfterSeconds?: number
}

export interface UserDto {
  id: string
  email: string
}

export interface ValidationError {
  field: string
  message: string
}

export interface ApiError {
  message: string
  errors?: ValidationError[]
}
