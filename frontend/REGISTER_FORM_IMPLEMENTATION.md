# RegisterForm Implementation

## Overview
This document describes the implementation of the user registration page (Task 8.3) for the address-book-webapp.

## Files Created

### 1. Types (`src/types/auth.ts`)
- `RegisterRequest`: Interface for registration request data
- `RegisterResponse`: Interface for registration response
- `LoginRequest`, `LoginResponse`: Interfaces for login (for future use)
- `UserDto`: User data transfer object
- `ValidationError`, `ApiError`: Error handling interfaces

### 2. Services (`src/services/auth.ts`)
- `authService.register()`: Calls the backend registration API
- `authService.login()`: Calls the backend login API (for future use)
- `authService.logout()`: Calls the backend logout API (for future use)
- Configured with Axios for HTTP requests

### 3. Store (`src/stores/auth.ts`)
- `useAuthStore`: Pinia store for authentication state management
- Manages token, user data, and session expiration
- Provides `register()`, `login()`, `logout()` methods
- Implements token expiration checking
- Persists authentication state in localStorage

### 4. Component (`src/components/RegisterForm.vue`)
Implements all requirements from the spec:

#### Features:
1. **Email Input Field**
   - Real-time validation
   - Checks for required field
   - Validates email format
   - Checks maximum length (255 characters)
   - Displays field-specific error messages

2. **Password Input Field**
   - Real-time validation
   - Checks for required field
   - Validates password strength requirements:
     - Minimum 8 characters
     - Contains uppercase letter
     - Contains lowercase letter
     - Contains number
     - Contains special character (@$!%*?&)

3. **Password Strength Indicator**
   - Visual progress bar showing strength (weak/medium/strong)
   - Color-coded (red/yellow/green)
   - Checklist showing which requirements are met
   - Real-time updates as user types

4. **Success/Error Messages**
   - Success message: "アカウントが正常に作成されました"
   - Error messages for:
     - Duplicate email: "このメールアドレスは既に使用されています"
     - Database errors: "アカウントを作成できません。もう一度お試しください。"
     - Validation errors from API
   - Auto-redirect to login page after successful registration (2 seconds)

5. **Form Validation**
   - Submit button disabled until all validations pass
   - Trims email before submission
   - Prevents submission during processing (loading state)

6. **User Experience**
   - Responsive design with Tailwind CSS
   - Clear visual feedback for validation states
   - Link to login page for existing users
   - Accessible form with proper labels and ARIA attributes

### 5. View (`src/views/RegisterView.vue`)
- Wrapper view for the RegisterForm component
- Provides centered layout with proper spacing

### 6. Router (`src/router/index.ts`)
- Added `/register` route pointing to RegisterView

### 7. Tests (`src/components/RegisterForm.test.ts`)
Comprehensive unit tests covering:
- Form rendering
- Email validation (empty, invalid format, valid)
- Password validation (all requirements)
- Password strength indicator
- Submit button state (enabled/disabled)
- Form submission with valid data
- Success message display
- Error message display
- API validation errors
- Email trimming

## Requirements Validation

### Requirement 10.1 ✓
"ユーザーが有効なメールアドレスと強力なパスワードを提供した場合、住所録システムは新しいアカウントを作成する"
- Implemented in `handleSubmit()` method

### Requirement 10.2 ✓
"User_Validator SHALL パスワードが8文字以上で、大文字、小文字、数字、特殊文字を含むことを検証する"
- Implemented in `validatePassword()` with computed properties for each requirement

### Requirement 10.3 ✓
"ユーザーが既に存在するメールアドレスで登録を試行した場合、住所録システムは登録を拒否し、「このメールアドレスは既に使用されています」エラーを表示する"
- Implemented in error handling with specific message check

### Requirement 10.4 ✓
"登録が正常に完了した場合、住所録システムは確認メッセージ「アカウントが正常に作成されました」を表示し、ログインページにリダイレクトする"
- Implemented with success message and 2-second delay before redirect

### Requirement 10.5 ✓
"IF 登録中にデータベースエラーが発生した場合、THEN 住所録システムは「アカウントを作成できません。もう一度お試しください。」エラーを表示し、フォームデータを保持する"
- Implemented in error handling, form data is preserved on error

### Requirement 10.6 ✓
"ユーザーが無効なメールアドレス形式を入力した場合、住所録システムは「有効なメールアドレスを入力してください」エラーを表示する"
- Implemented in `validateEmail()` method

### Requirement 10.7 ✓
"IF パスワード要件を満たさない場合、THEN 住所録システムは「パスワードは8文字以上で、大文字、小文字、数字、特殊文字を含む必要があります」エラーを表示する"
- Implemented in `validatePassword()` method

## Testing

All 12 unit tests pass successfully:
```bash
npm test -- RegisterForm.test.ts
```

## Usage

1. Navigate to `/register` route
2. Enter email address
3. Enter password (see real-time strength indicator)
4. Click "登録" button
5. On success: see confirmation message and auto-redirect to login
6. On error: see specific error message and retry

## Integration with Backend

The component expects the backend API to be available at:
- `POST /api/auth/register`

Request format:
```json
{
  "email": "user@example.com",
  "password": "SecurePass1@"
}
```

Response format (success):
```json
{
  "message": "アカウントが正常に作成されました"
}
```

Response format (error):
```json
{
  "message": "このメールアドレスは既に使用されています",
  "errors": [
    {
      "field": "email",
      "message": "有効なメールアドレスを入力してください"
    }
  ]
}
```

## Environment Configuration

Set the API base URL in `.env`:
```
VITE_API_BASE_URL=http://localhost:5000
```

## Next Steps

- Task 8.1: Complete auth store with token management and auto-logout
- Task 8.2: Implement LoginForm component
- Task 8.4: Implement auth guard and routing protection
