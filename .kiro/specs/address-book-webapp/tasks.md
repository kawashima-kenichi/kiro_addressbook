# Implementation Plan: 住所録WEBアプリケーション

## Overview

Vue.js 3 + TypeScript（フロントエンド）、ASP.NET Core 10.0 + C#（バックエンド）、PostgreSQL 18（データベース）を使用した住所録WEBアプリケーションの実装計画です。Docker Composeによる開発環境構築から始め、バックエンドAPI、フロントエンドUI、テストの順に段階的に実装します。

## Tasks

- [x] 1. プロジェクト構造とインフラストラクチャのセットアップ
  - [x] 1.1 バックエンドプロジェクトの初期化
    - ASP.NET Core 10.0 Web APIプロジェクトを作成
    - NuGetパッケージを追加: Entity Framework Core, FluentValidation, AutoMapper, Serilog, ASP.NET Core Identity, JWT Bearer Authentication, FsCheck
    - ソリューション構造を作成（API, Domain, Infrastructure, Application層）
    - `appsettings.json` と `appsettings.Development.json` を設定
    - _Requirements: 1.1, 6.1, 9.2_

  - [x] 1.2 フロントエンドプロジェクトの初期化
    - Vite + Vue.js 3 + TypeScriptプロジェクトを作成
    - npm パッケージを追加: Vue Router, Pinia, Axios, Tailwind CSS 4.x
    - ESLint, Prettier, Vitest, Vue Test Utils を設定
    - プロジェクトディレクトリ構造を作成（components, views, stores, services, types）
    - _Requirements: 7.1, 7.2_

  - [x] 1.3 Docker Compose開発環境の構築
    - `docker-compose.yml` を作成（PostgreSQL 18, バックエンドAPI, フロントエンド）
    - `docker-compose.test.yml` をテスト用に作成
    - Dockerfile（バックエンド）と Dockerfile（フロントエンド）を作成
    - `.env.example` ファイルを作成
    - _Requirements: 6.1, 6.3_

- [x] 2. データモデルとデータベース層の実装
  - [x] 2.1 Entity Framework Core のエンティティとDbContextを作成
    - `User`, `Contact`, `AuditLog` エンティティクラスを作成
    - `ApplicationDbContext` を作成し、エンティティ設定を定義
    - インデックス、ユニーク制約、リレーションシップを設定
    - 初期マイグレーションを作成
    - _Requirements: 6.1, 6.2, 9.4_

  - [x] 2.2 リポジトリパターンの実装
    - `IContactRepository`, `IUserRepository`, `IAuditLogRepository` インターフェースを定義
    - 各リポジトリの実装クラスを作成
    - ページネーション、ソート、フィルタリングのサポートを実装
    - _Requirements: 3.1, 3.3, 6.1_

  - [x]* 2.3 データモデルのプロパティテスト
    - **Property 7: データ永続化の保証**
    - **Validates: Requirements 6.1**

- [x] 3. 認証・認可システムの実装
  - [x] 3.1 ASP.NET Core Identity とJWT認証の設定
    - ASP.NET Core Identity をカスタムUserクラスで設定
    - JWT トークン生成・検証サービスを実装
    - 認証ミドルウェアを設定
    - bcrypt（12ラウンド以上）によるパスワードハッシュを設定
    - _Requirements: 1.1, 1.4, 9.3, 9.7_

  - [x] 3.2 認証コントローラーの実装
    - `POST /api/auth/login` エンドポイントを実装
    - `POST /api/auth/register` エンドポイントを実装
    - `POST /api/auth/logout` エンドポイントを実装
    - アカウントロック機能（5回失敗で30分ロック）を実装
    - セッション有効期限（24時間）を実装
    - _Requirements: 1.1, 1.2, 1.3, 1.5, 1.6, 10.1, 10.3, 10.4_

  - [x] 3.3 ユーザー登録バリデーションの実装
    - `RegisterRequestValidator` を FluentValidation で作成
    - メールアドレス形式検証を実装
    - パスワード強度検証（8文字以上、大文字、小文字、数字、特殊文字）を実装
    - 重複メールアドレスチェックを実装
    - _Requirements: 10.1, 10.2, 10.3, 10.6, 10.7_

  - [x]* 3.4 認可アクセス制御のプロパティテスト
    - **Property 8: 認可とアクセス制御**
    - **Validates: Requirements 9.1**

- [x] 4. Checkpoint - 認証基盤の確認
  - Ensure all tests pass, ask the user if questions arise.

- [x] 5. 連絡先CRUDバックエンドの実装
  - [x] 5.1 連絡先サービス層の実装
    - `IContactService` インターフェースを定義
    - `ContactService` を実装（作成、読み取り、更新、削除）
    - AutoMapper プロファイルを作成（Entity ↔ DTO変換）
    - 所有権検証ロジックを実装
    - _Requirements: 2.1, 3.1, 4.2, 5.2, 9.1_

  - [x] 5.2 連絡先バリデーションの実装
    - `CreateContactRequestValidator` を FluentValidation で作成
    - `UpdateContactRequestValidator` を FluentValidation で作成
    - 名前（1-100文字）、住所（最大500文字）、電話番号（最大20文字、形式検証）のバリデーション
    - 重複名チェック（大文字小文字区別なし）を実装
    - _Requirements: 2.2, 2.3, 2.4, 2.5, 2.8, 4.3, 8.1, 8.2, 8.3_

  - [x] 5.3 連絡先コントローラーの実装
    - `GET /api/contacts` （ページネーション、ソート付き）を実装
    - `GET /api/contacts/{id}` を実装
    - `POST /api/contacts` を実装
    - `PUT /api/contacts/{id}` を実装
    - `DELETE /api/contacts/{id}` を実装
    - エラーレスポンス（バリデーションエラー、データベースエラー）を統一フォーマットで返却
    - _Requirements: 2.1, 2.6, 2.7, 3.1, 3.2, 3.3, 3.4, 3.7, 3.8, 4.1, 4.2, 4.4, 4.6, 5.1, 5.2, 5.4, 5.5_

  - [x]* 5.4 連絡先作成のプロパティテスト
    - **Property 1: 連絡先作成の基本機能**
    - **Validates: Requirements 2.1**

  - [x]* 5.5 連絡先名重複防止のプロパティテスト
    - **Property 2: 連絡先名の重複防止**
    - **Validates: Requirements 2.4, 8.2**

  - [x]* 5.6 入力検証エラーハンドリングのプロパティテスト
    - **Property 3: 入力検証エラーハンドリング**
    - **Validates: Requirements 2.5, 2.8, 8.1, 8.3**

  - [x]* 5.7 連絡先リストソートのプロパティテスト
    - **Property 4: 連絡先リストのソート機能**
    - **Validates: Requirements 3.1**

  - [x]* 5.8 連絡先更新のプロパティテスト
    - **Property 5: 連絡先更新の基本機能**
    - **Validates: Requirements 4.2**

  - [x]* 5.9 更新時入力検証のプロパティテスト
    - **Property 6: 更新時の入力検証**
    - **Validates: Requirements 4.3**

- [x] 6. 監査ログとセキュリティミドルウェアの実装
  - [x] 6.1 監査ログサービスの実装
    - `IAuditLogService` インターフェースを定義
    - `AuditLogService` を実装（アクション記録、IPアドレス、UserAgent取得）
    - 監査ログミドルウェアを作成し、データアクセス・変更操作を自動記録
    - _Requirements: 9.4, 9.6_

  - [x] 6.2 CORSとセキュリティミドルウェアの設定
    - CORS ポリシーを設定
    - レート制限ミドルウェアを設定
    - HTTPS リダイレクトを設定
    - グローバル例外ハンドラーを実装（エラーID付きログ記録）
    - _Requirements: 6.5, 9.2, 9.6_

- [x] 7. Checkpoint - バックエンドAPIの確認
  - Ensure all tests pass, ask the user if questions arise.

- [x] 8. フロントエンド認証機能の実装
  - [x] 8.1 認証ストアとサービスの実装
    - Pinia認証ストア（`useAuthStore`）を作成
    - Axiosインスタンスを作成（JWTトークン自動付与、インターセプター設定）
    - 認証APIサービス（login, register, logout）を実装
    - トークン有効期限チェックと自動ログアウトを実装
    - _Requirements: 1.1, 1.4, 1.5, 1.6_

  - [x] 8.2 ログインページの実装
    - `LoginForm.vue` コンポーネントを作成
    - メールアドレス・パスワード入力フィールドとバリデーションを実装
    - ログインエラーメッセージ表示（汎用メッセージ、アカウントロック通知）を実装
    - ログイン成功時のリダイレクトを実装
    - _Requirements: 1.1, 1.2, 1.3_

  - [x] 8.3 ユーザー登録ページの実装
    - `RegisterForm.vue` コンポーネントを作成
    - メールアドレス、パスワード入力フィールドとリアルタイムバリデーションを実装
    - パスワード強度インジケーターを実装
    - 登録成功・エラーメッセージ表示を実装
    - _Requirements: 10.1, 10.2, 10.3, 10.4, 10.5, 10.6, 10.7_

  - [x] 8.4 認証ガードとルーティングの実装
    - Vue Router を設定（認証必須ルート、公開ルート）
    - `AuthGuard.vue` コンポーネントを作成
    - 未認証時のログインページリダイレクトを実装
    - セッション期限切れ時の自動リダイレクトを実装
    - _Requirements: 1.5, 9.7_

- [x] 9. フロントエンド連絡先管理機能の実装
  - [x] 9.1 連絡先ストアとサービスの実装
    - Pinia連絡先ストア（`useContactStore`）を作成
    - 連絡先APIサービス（CRUD操作、ページネーション）を実装
    - エラーハンドリングとローディング状態管理を実装
    - _Requirements: 2.1, 3.1, 4.2, 5.2_

  - [x] 9.2 連絡先一覧ページの実装
    - `ContactList.vue` コンポーネントを作成
    - `ContactCard.vue` コンポーネントを作成
    - ページネーションコンポーネント（`BasePagination.vue`）を作成
    - 空状態メッセージ表示を実装
    - 連絡先数インジケーター（「Y件中X件を表示」）を実装
    - _Requirements: 3.1, 3.2, 3.3, 3.5, 3.6, 3.8_

  - [x] 9.3 連絡先作成フォームの実装
    - `ContactForm.vue` コンポーネントを作成（作成・編集兼用）
    - フロントエンドバリデーション（名前必須、文字数制限、電話番号形式）を実装
    - フィールド固有のエラーメッセージ表示を実装
    - 成功・エラーメッセージ表示とリダイレクトを実装
    - _Requirements: 2.1, 2.2, 2.3, 2.5, 2.6, 2.7, 2.8, 8.1, 8.3, 8.4_

  - [x] 9.4 連絡先詳細・編集ページの実装
    - `ContactDetail.vue` コンポーネントを作成
    - 編集モードへの切り替えを実装
    - 更新成功・エラーメッセージ表示を実装
    - キャンセル機能（変更破棄）を実装
    - _Requirements: 3.4, 4.1, 4.2, 4.3, 4.4, 4.5, 4.6_

  - [x] 9.5 連絡先削除機能の実装
    - 削除確認モーダル（`BaseModal.vue`）を作成
    - 確認メッセージ「[連絡先名]を削除してもよろしいですか？この操作は元に戻せません。」を表示
    - 30秒タイムアウトによる自動クローズを実装
    - 削除成功・エラーメッセージ表示を実装
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5, 5.6, 5.7_

- [x] 10. レスポンシブデザインとUIコンポーネントの実装
  - [x] 10.1 共通UIコンポーネントの実装
    - `BaseButton.vue`, `BaseInput.vue`, `BaseModal.vue` を作成
    - `AppHeader.vue`, `AppNavigation.vue`, `AppFooter.vue` を作成
    - Tailwind CSS 4.x でレスポンシブスタイルを適用
    - アクセシビリティ対応（ARIA属性、キーボードナビゲーション）を実装
    - _Requirements: 7.1, 7.2_

  - [x]* 10.2 フロントエンドコンポーネントのユニットテスト
    - ContactForm, ContactList, LoginForm のユニットテストを作成（Vitest + Vue Test Utils）
    - バリデーションロジックのテストを作成
    - ストアのテストを作成
    - _Requirements: 2.5, 8.4, 10.2_

- [x] 11. Checkpoint - フロントエンド機能の確認
  - Ensure all tests pass, ask the user if questions arise.

- [x] 12. 統合とエンドツーエンド接続
  - [x] 12.1 フロントエンドとバックエンドの統合
    - API ベースURL設定と環境変数を設定
    - Axios インターセプターでエラーハンドリングを統合
    - CORS設定の最終調整
    - 全APIエンドポイントとフロントエンドの接続を確認
    - _Requirements: 2.1, 3.1, 4.2, 5.2, 9.2_

  - [x]* 12.2 バックエンド統合テストの作成
    - ASP.NET Core Test Host + Testcontainers で統合テスト環境を構築
    - 認証フロー（登録、ログイン、ログアウト、セッション期限切れ）の統合テストを作成
    - 連絡先CRUD操作の統合テストを作成
    - アクセス制御（他ユーザーの連絡先へのアクセス拒否）の統合テストを作成
    - _Requirements: 1.1, 1.3, 2.1, 3.1, 4.2, 5.2, 9.1_

- [x] 13. Final Checkpoint - 全体統合の確認
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation
- Property tests validate universal correctness properties defined in the design document
- Unit tests validate specific examples and edge cases
- フロントエンドは TypeScript + Vue.js 3、バックエンドは C# + ASP.NET Core 10.0 で実装
- Docker Compose で開発環境を統一し、PostgreSQL 18 をデータベースとして使用
- プロパティベーステストは FsCheck (.NET) を使用

## Task Dependency Graph

```json
{
  "waves": [
    { "id": 0, "tasks": ["1.1", "1.2", "1.3"] },
    { "id": 1, "tasks": ["2.1"] },
    { "id": 2, "tasks": ["2.2", "3.1"] },
    { "id": 3, "tasks": ["2.3", "3.2", "3.3"] },
    { "id": 4, "tasks": ["3.4", "5.1", "5.2"] },
    { "id": 5, "tasks": ["5.3", "6.1", "6.2"] },
    { "id": 6, "tasks": ["5.4", "5.5", "5.6", "5.7", "5.8", "5.9"] },
    { "id": 7, "tasks": ["8.1", "8.2", "8.3"] },
    { "id": 8, "tasks": ["8.4", "9.1"] },
    { "id": 9, "tasks": ["9.2", "9.3", "9.4", "9.5"] },
    { "id": 10, "tasks": ["10.1"] },
    { "id": 11, "tasks": ["10.2", "12.1"] },
    { "id": 12, "tasks": ["12.2"] }
  ]
}
```
