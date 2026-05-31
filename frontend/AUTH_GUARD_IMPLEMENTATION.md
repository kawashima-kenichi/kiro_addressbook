# 認証ガードとルーティングの実装

## 概要

タスク 8.4「認証ガードとルーティングの実装」を完了しました。このタスクでは、Vue Router を使用した認証ガード、未認証時のリダイレクト、セッション期限切れ時の自動リダイレクトを実装しました。

## 実装内容

### 1. Vue Router の設定（`src/router/index.ts`）

#### 認証必須ルートと公開ルートの定義

```typescript
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
    component: () => import('@/views/HomeView.vue'),
    meta: { requiresAuth: true },
  },
]
```

#### グローバルナビゲーションガード

- **未認証ユーザーの保護**: 認証が必要なルートにアクセスしようとすると、ログインページにリダイレクト
- **リダイレクト先の保存**: `redirect` クエリパラメータに元のURLを保存し、ログイン後に元のページに戻れるようにする
- **認証済みユーザーの公開ページアクセス制限**: ログイン済みユーザーがログインページや登録ページにアクセスしようとすると、ホームページにリダイレクト

```typescript
router.beforeEach((to, _from, next) => {
  const authStore = useAuthStore()
  const requiresAuth = to.matched.some((record) => record.meta.requiresAuth)
  const publicOnly = to.matched.some((record) => record.meta.publicOnly)
  const isAuthenticated = authStore.isAuthenticated

  if (requiresAuth && !isAuthenticated) {
    // 未認証ユーザーをログインページにリダイレクト
    next({
      name: 'login',
      query: { redirect: to.fullPath },
    })
  } else if (publicOnly && isAuthenticated) {
    // 認証済みユーザーをホームにリダイレクト
    next({ name: 'home' })
  } else {
    next()
  }
})
```

### 2. AuthGuard コンポーネント（`src/components/AuthGuard.vue`）

認証が必要なコンテンツをラップし、セッション期限切れ時の自動リダイレクトを処理するコンポーネントです。

#### 主な機能

1. **認証状態の監視**: `useAuthStore` の `isAuthenticated` を監視
2. **セッション期限切れチェック**: 30秒ごとにトークンの有効期限をチェック
3. **自動ログアウトとリダイレクト**: セッションが期限切れになった場合、自動的にログアウトしてログインページにリダイレクト
4. **スロットベースのコンテンツ表示**: 認証済みの場合のみ、スロットのコンテンツを表示

```vue
<template>
  <div v-if="isAuthenticated">
    <!-- 認証済みの場合、スロットのコンテンツを表示 -->
    <slot />
  </div>
  <div v-else class="flex min-h-screen items-center justify-center">
    <!-- 未認証の場合、ローディング表示 -->
    <div class="text-center">
      <p class="text-gray-600">認証を確認しています...</p>
    </div>
  </div>
</template>
```

### 3. LoginForm の更新（`src/components/LoginForm.vue`）

#### リダイレクト処理の実装

- ログイン成功後、`redirect` クエリパラメータに保存されたURLにリダイレクト
- `redirect` パラメータがない場合は、ホームページにリダイレクト

```typescript
const handleSubmit = async () => {
  try {
    await authStore.login({
      email: email.value,
      password: password.value,
    })
    
    // Redirect to the original destination or home page
    const redirectPath = (route.query.redirect as string) || '/'
    router.push(redirectPath)
  } catch (error) {
    console.error('Login failed:', error)
  }
}
```

#### セッション期限切れメッセージの表示

- `expired=true` クエリパラメータがある場合、セッション期限切れメッセージを表示

```typescript
onMounted(() => {
  if (route.query.expired === 'true') {
    sessionExpiredMessage.value = 'セッションが期限切れになりました。再度ログインしてください。'
  }
})
```

### 4. HomeView の更新（`src/views/HomeView.vue`）

- ヘッダーにユーザー情報とログアウトボタンを追加
- ログアウト機能を実装

```vue
<template>
  <div class="min-h-screen bg-gray-50">
    <header class="bg-white shadow">
      <div class="mx-auto max-w-7xl px-4 py-6 sm:px-6 lg:px-8">
        <div class="flex items-center justify-between">
          <h1 class="text-3xl font-bold text-gray-900">住所録アプリケーション</h1>
          <div class="flex items-center space-x-4">
            <span class="text-sm text-gray-600">
              {{ authStore.user?.email }}
            </span>
            <button @click="handleLogout">
              ログアウト
            </button>
          </div>
        </div>
      </div>
    </header>
  </div>
</template>
```

## テスト

### AuthGuard コンポーネントのテスト（`src/components/AuthGuard.test.ts`）

1. **認証済みユーザーにはスロットコンテンツを表示する**
2. **未認証ユーザーにはローディングメッセージを表示する**
3. **セッション期限切れ時にログインページにリダイレクトする**
4. **定期的にセッション期限切れをチェックする**
5. **コンポーネントアンマウント時にタイマーをクリアする**

### Router ナビゲーションガードのテスト（`src/router/index.test.ts`）

#### 認証必須ルート
1. **未認証ユーザーはログインページにリダイレクトされる**
2. **認証済みユーザーは認証必須ルートにアクセスできる**

#### 公開専用ルート
3. **認証済みユーザーはログインページからホームにリダイレクトされる**
4. **認証済みユーザーは登録ページからホームにリダイレクトされる**
5. **未認証ユーザーは公開ルートにアクセスできる**

#### リダイレクトクエリパラメータ
6. **未認証ユーザーが保護されたルートにアクセスすると、リダイレクト先が保存される**

### テスト結果

```
✓ src/components/AuthGuard.test.ts (5)
✓ src/router/index.test.ts (6)

Test Files  6 passed (6)
Tests  54 passed (54)
```

すべてのテストが成功しました。

## 要件の検証

### 要件 1.5: セッション期限切れ時の自動ログアウトとリダイレクト

✅ **実装済み**
- `AuthGuard` コンポーネントが30秒ごとにセッションの有効期限をチェック
- セッションが期限切れになった場合、自動的にログアウトしてログインページにリダイレクト
- ログインページに `expired=true` クエリパラメータを付与し、セッション期限切れメッセージを表示

### 要件 9.7: セッションタイムアウトの実装

✅ **実装済み**
- `useAuthStore` が24時間の非アクティブ状態後にセッションタイムアウトを実装
- トークンの有効期限を定期的にチェックし、期限切れの場合は自動ログアウト

## ファイル構成

```
frontend/src/
├── router/
│   ├── index.ts                    # Vue Router 設定（認証ガード付き）
│   └── index.test.ts               # Router ナビゲーションガードのテスト
├── components/
│   ├── AuthGuard.vue               # 認証ガードコンポーネント
│   ├── AuthGuard.test.ts           # AuthGuard のテスト
│   └── LoginForm.vue               # ログインフォーム（リダイレクト処理追加）
├── views/
│   └── HomeView.vue                # ホームビュー（ログアウト機能追加）
└── stores/
    └── auth.ts                     # 認証ストア（既存）
```

## 使用方法

### 認証必須ルートの追加

新しい認証必須ルートを追加する場合は、`meta: { requiresAuth: true }` を設定します。

```typescript
{
  path: '/contacts',
  name: 'contacts',
  component: () => import('@/views/ContactsView.vue'),
  meta: { requiresAuth: true },
}
```

### 公開ルートの追加

新しい公開ルート（認証済みユーザーはアクセス不可）を追加する場合は、`meta: { requiresAuth: false, publicOnly: true }` を設定します。

```typescript
{
  path: '/about',
  name: 'about',
  component: () => import('@/views/AboutView.vue'),
  meta: { requiresAuth: false, publicOnly: true },
}
```

### AuthGuard コンポーネントの使用（オプション）

Vue Router のナビゲーションガードで十分ですが、特定のコンポーネント内でセッション期限切れを監視したい場合は、`AuthGuard` コンポーネントを使用できます。

```vue
<template>
  <AuthGuard>
    <div>
      <!-- 認証が必要なコンテンツ -->
    </div>
  </AuthGuard>
</template>

<script setup lang="ts">
import AuthGuard from '@/components/AuthGuard.vue'
</script>
```

## まとめ

タスク 8.4「認証ガードとルーティングの実装」を完了しました。

- ✅ Vue Router を設定（認証必須ルート、公開ルート）
- ✅ `AuthGuard.vue` コンポーネントを作成
- ✅ 未認証時のログインページリダイレクトを実装
- ✅ セッション期限切れ時の自動リダイレクトを実装
- ✅ 包括的なユニットテストを作成（11テスト）
- ✅ すべてのテストが成功（54/54 passed）

要件 1.5 と 9.7 を満たす実装が完了しました。
