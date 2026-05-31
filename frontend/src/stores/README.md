# Stores

このディレクトリには、Piniaストアが含まれています。

## 認証ストア (auth.ts)

### 概要

`useAuthStore`は、ユーザー認証状態を管理するPiniaストアです。以下の機能を提供します：

- ユーザーログイン
- ユーザー登録
- ログアウト
- JWTトークンの管理
- トークン有効期限の自動チェック
- 期限切れ時の自動ログアウト
- ローカルストレージへの永続化

### 使用方法

#### 基本的な使用例

```typescript
import { useAuthStore } from '@/stores/auth';

// コンポーネント内で使用
const authStore = useAuthStore();

// ログイン
await authStore.login({
  email: 'user@example.com',
  password: 'password123'
});

// 認証状態の確認
if (authStore.isAuthenticated) {
  console.log('ログイン済み:', authStore.user);
}

// ログアウト
await authStore.logout();
```

#### エラーハンドリング

```typescript
try {
  await authStore.login({
    email: 'user@example.com',
    password: 'wrong-password'
  });
} catch (error) {
  // エラーメッセージはストアのerrorプロパティに保存される
  console.error('ログインエラー:', authStore.error);
}

// エラーをクリア
authStore.clearError();
```

#### ユーザー登録

```typescript
try {
  await authStore.register({
    email: 'newuser@example.com',
    password: 'Password123!'
  });
  
  // 登録成功後、ログインページにリダイレクト
  router.push('/login');
} catch (error) {
  console.error('登録エラー:', authStore.error);
}
```

### State

- `token`: JWTトークン（string | null）
- `user`: ユーザー情報（UserDto | null）
- `expiresAt`: トークン有効期限（string | null）
- `loading`: ローディング状態（boolean）
- `error`: エラーメッセージ（string | null）

### Getters

- `isAuthenticated`: 認証済みかどうか（boolean）

### Actions

- `login(credentials)`: ログイン
- `register(userData)`: ユーザー登録
- `logout()`: ログアウト
- `clearError()`: エラーメッセージをクリア
- `isTokenExpired()`: トークンが期限切れかどうかをチェック

### トークン有効期限チェック

認証ストアは、1分ごとにトークンの有効期限をチェックします。トークンが期限切れの場合、自動的にログアウトします。

### ローカルストレージ

認証情報は以下のキーでローカルストレージに保存されます：

- `auth_token`: JWTトークン
- `auth_user`: ユーザー情報（JSON）
- `auth_expires_at`: トークン有効期限

ページをリロードしても、ローカルストレージから認証情報が復元されます。
