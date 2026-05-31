# 住所録WEBアプリケーション

個人や組織の連絡先情報を効率的に管理するためのモダンなWEBベースシステムです。

## 🌟 主要機能

- ✅ **ユーザー認証**: JWT + ASP.NET Core Identity
- ✅ **連絡先管理**: CRUD操作（作成、読み取り、更新、削除）
- ✅ **データ永続化**: PostgreSQL 18
- ✅ **レスポンシブデザイン**: デスクトップ・モバイル対応
- ✅ **セキュリティ**: HTTPS、パスワードハッシュ化（bcrypt）、監査ログ
- ✅ **プロパティベーステスト**: FsCheckによる正確性検証

## 🛠️ 技術スタック

### フロントエンド
- **Vue.js 3** - プログレッシブJavaScriptフレームワーク
- **TypeScript** - 型安全性
- **Tailwind CSS 4.x** - ユーティリティファーストCSSフレームワーク
- **Pinia** - 状態管理
- **Vue Router** - ルーティング
- **Axios** - HTTPクライアント
- **Vite** - 高速ビルドツール
- **Vitest** - ユニットテストフレームワーク

### バックエンド
- **ASP.NET Core 10.0** - Webフレームワーク
- **C#** - プログラミング言語
- **Entity Framework Core 10.0** - ORM
- **PostgreSQL 18** - リレーショナルデータベース
- **JWT** - 認証トークン
- **FluentValidation** - バリデーション
- **AutoMapper** - オブジェクトマッピング
- **Serilog** - 構造化ログ
- **FsCheck** - プロパティベーステスト
- **xUnit** - ユニットテストフレームワーク

### インフラストラクチャ
- **Docker** - コンテナ化
- **Docker Compose** - マルチコンテナ管理

## 📋 前提条件

以下のソフトウェアがインストールされている必要があります：

- **Docker Desktop** (最新版)
- **.NET SDK 10.0** (プレビュー版)
- **Node.js 20.x** 以上
- **npm** または **pnpm**

## 🚀 クイックスタート

### 1. リポジトリのクローン

```bash
git clone https://github.com/kawashima-kenichi/kiro_addressbook.git
cd kiro_addressbook
```

### 2. 環境変数の設定

```bash
# フロントエンド
cp frontend/.env.example frontend/.env

# バックエンド（必要に応じて）
cp .env.example .env
```

### 3. データベースの起動

```bash
docker compose up -d db
```

### 4. バックエンドの起動

```bash
cd backend/src/AddressBook.API
dotnet restore
dotnet run
```

バックエンドは `http://localhost:5147` で起動します。

### 5. フロントエンドの起動

別のターミナルで：

```bash
cd frontend
npm install
npm run dev
```

フロントエンドは `http://localhost:5173` で起動します。

### 6. アプリケーションへのアクセス

ブラウザで `http://localhost:5173` を開きます。

## 📖 使い方

### ユーザー登録

1. トップページの「登録」ボタンをクリック
2. メールアドレスとパスワードを入力
   - パスワード要件: 8文字以上、大文字、小文字、数字、特殊文字を含む
3. 「登録」ボタンをクリック

### ログイン

1. トップページの「ログイン」ボタンをクリック
2. 登録したメールアドレスとパスワードを入力
3. 「ログイン」ボタンをクリック

### 連絡先の管理

#### 連絡先の追加
1. ログイン後、「連絡先を追加」ボタンをクリック
2. 名前（必須）、住所、電話番号を入力
3. 「保存」ボタンをクリック

#### 連絡先の表示
- 連絡先一覧ページで全ての連絡先を確認
- 名前のアルファベット順（大文字小文字を区別しない）で表示
- 50件を超える場合はページネーション表示

#### 連絡先の編集
1. 連絡先カードの「編集」ボタンをクリック
2. 情報を変更
3. 「保存」ボタンをクリック

#### 連絡先の削除
1. 連絡先カードの「削除」ボタンをクリック
2. 確認ダイアログで「確認」をクリック

## 🧪 テストの実行

### バックエンドテスト

```bash
cd backend
dotnet test
```

#### プロパティベーステスト

```bash
cd backend
dotnet test --filter "Category=Property"
```

#### 統合テスト

```bash
cd backend
dotnet test --filter "Category=Integration"
```

### フロントエンドテスト

```bash
cd frontend
npm run test
```

## 🏗️ プロジェクト構造

```
kiro_addressbook/
├── backend/                      # バックエンド（ASP.NET Core）
│   ├── src/
│   │   ├── AddressBook.API/      # APIレイヤー
│   │   ├── AddressBook.Application/  # アプリケーション層
│   │   ├── AddressBook.Domain/   # ドメイン層
│   │   └── AddressBook.Infrastructure/  # インフラストラクチャ層
│   └── tests/
│       └── AddressBook.Tests/    # テスト
├── frontend/                     # フロントエンド（Vue.js）
│   ├── src/
│   │   ├── components/           # Vueコンポーネント
│   │   ├── views/                # ページビュー
│   │   ├── stores/               # Piniaストア
│   │   ├── services/             # APIサービス
│   │   ├── types/                # TypeScript型定義
│   │   └── router/               # Vue Router設定
│   └── tests/                    # テスト
├── .kiro/
│   └── specs/
│       └── address-book-webapp/  # 仕様書
│           ├── requirements.md   # 要件定義書
│           ├── design.md         # 設計書
│           └── tasks.md          # タスクリスト
├── docker-compose.yml            # Docker Compose設定
└── README.md                     # このファイル
```

## 🔒 セキュリティ機能

- **HTTPS**: すべての通信を暗号化（TLS 1.2以上）
- **パスワードハッシュ化**: bcrypt（12ラウンド以上）
- **JWT認証**: セキュアなトークンベース認証
- **セッション管理**: 24時間の有効期限
- **アカウントロック**: 5回失敗で30分間ロック
- **監査ログ**: すべてのデータアクセスと変更を記録
- **アクセス制御**: ユーザーは自分の連絡先のみアクセス可能

## 📊 データベーススキーマ

### Users テーブル
- `id` (UUID, PK)
- `email` (VARCHAR(255), UNIQUE)
- `password_hash` (VARCHAR(255))
- `created_at` (TIMESTAMP)
- `updated_at` (TIMESTAMP)
- `last_login` (TIMESTAMP)
- `failed_login_attempts` (INTEGER)
- `locked_until` (TIMESTAMP)

### Contacts テーブル
- `id` (UUID, PK)
- `user_id` (UUID, FK → Users)
- `name` (VARCHAR(100))
- `address` (VARCHAR(500))
- `phone_number` (VARCHAR(20))
- `created_at` (TIMESTAMP)
- `updated_at` (TIMESTAMP)
- UNIQUE制約: (user_id, name)

### AuditLogs テーブル
- `id` (UUID, PK)
- `user_id` (UUID, FK → Users)
- `action` (VARCHAR(50))
- `resource_type` (VARCHAR(50))
- `resource_id` (UUID)
- `ip_address` (INET)
- `user_agent` (TEXT)
- `created_at` (TIMESTAMP)

## 🧩 APIエンドポイント

### 認証
- `POST /api/auth/login` - ログイン
- `POST /api/auth/register` - ユーザー登録
- `POST /api/auth/logout` - ログアウト

### 連絡先
- `GET /api/contacts` - 連絡先一覧取得（ページネーション付き）
- `GET /api/contacts/{id}` - 連絡先詳細取得
- `POST /api/contacts` - 連絡先作成
- `PUT /api/contacts/{id}` - 連絡先更新
- `DELETE /api/contacts/{id}` - 連絡先削除

## 🐛 トラブルシューティング

### データベース接続エラー

```bash
# Dockerコンテナの状態を確認
docker ps

# データベースコンテナを再起動
docker compose restart db
```

### ポートが既に使用されている

```bash
# 使用中のポートを確認
lsof -i :5147  # バックエンド
lsof -i :5173  # フロントエンド
lsof -i :5432  # PostgreSQL

# プロセスを停止
kill -9 <PID>
```

### マイグレーションエラー

```bash
cd backend/src/AddressBook.API
dotnet ef database drop --force
dotnet ef database update
```

## 📚 ドキュメント

- [要件定義書](.kiro/specs/address-book-webapp/requirements.md)
- [技術設計書](.kiro/specs/address-book-webapp/design.md)
- [タスクリスト](.kiro/specs/address-book-webapp/tasks.md)
- [クイックスタートガイド](QUICK_START.md)
- [統合ガイド](FRONTEND_BACKEND_INTEGRATION.md)

## 🤝 コントリビューション

プルリクエストを歓迎します！大きな変更の場合は、まずissueを開いて変更内容を議論してください。

## 📄 ライセンス

このプロジェクトはMITライセンスの下でライセンスされています。

## 👤 作者

**Kenichi Kawashima**

- GitHub: [@kawashima-kenichi](https://github.com/kawashima-kenichi)

## 🙏 謝辞

このプロジェクトは以下の技術を使用しています：

- [Vue.js](https://vuejs.org/)
- [ASP.NET Core](https://dotnet.microsoft.com/apps/aspnet)
- [PostgreSQL](https://www.postgresql.org/)
- [Tailwind CSS](https://tailwindcss.com/)
- [FsCheck](https://fscheck.github.io/FsCheck/)
- [Docker](https://www.docker.com/)

---

⭐ このプロジェクトが役に立った場合は、スターをつけてください！
