# バックエンド統合テスト

## 概要

このディレクトリには、ASP.NET Core Test Host と Testcontainers を使用した包括的な統合テストが含まれています。

## テストスイート

### 1. 認証フロー統合テスト (`AuthenticationFlowIntegrationTests.cs`)

**検証対象要件:** 1.1, 1.3, 2.1, 3.1, 4.2, 5.2

**テストケース:**
- ユーザー登録（有効な認証情報）
- ユーザー登録（重複メールアドレス）
- ユーザー登録（弱いパスワード）
- ユーザー登録（無効なメールアドレス形式）
- ログイン（有効な認証情報）
- ログイン（無効な認証情報）
- ログイン（複数回の失敗によるアカウントロック）
- ログイン（LastLoginタイムスタンプの更新）
- ログアウト（認証済みユーザー）
- ログアウト（未認証ユーザー）
- 保護されたエンドポイントへのアクセス（有効なトークン）
- 保護されたエンドポイントへのアクセス（トークンなし）
- 保護されたエンドポイントへのアクセス（無効なトークン）
- ログアウト後の再認証要求

### 2. 連絡先CRUD操作統合テスト (`ContactCrudIntegrationTests.cs`)

**検証対象要件:** 2.1, 3.1, 4.2, 5.2

**テストケース:**

#### 作成（Create）
- 有効なデータで連絡先を作成
- 最小限のデータで連絡先を作成（名前のみ）
- 重複する名前で連絡先を作成（エラー）
- 空の名前で連絡先を作成（エラー）
- 名前が長すぎる連絡先を作成（エラー）
- 無効な電話番号で連絡先を作成（エラー）
- 有効な電話番号形式で連絡先を作成（複数形式）
- 未認証で連絡先を作成（エラー）

#### 読み取り（Read）
- 空のリストを取得
- 連絡先リストを取得（アルファベット順ソート）
- ページネーション付きで連絡先リストを取得
- IDで連絡先を取得
- 存在しない連絡先を取得（エラー）

#### 更新（Update）
- 有効なデータで連絡先を更新
- 存在しない連絡先を更新（エラー）
- 無効なデータで連絡先を更新（エラー）
- 重複する名前で連絡先を更新（エラー）
- 連絡先更新時のタイムスタンプ更新

#### 削除（Delete）
- 既存の連絡先を削除
- 存在しない連絡先を削除（エラー）
- 未認証で連絡先を削除（エラー）

#### データ永続化
- リクエスト間でのデータ永続化
- 更新時のタイムスタンプ更新

### 3. アクセス制御統合テスト (`AccessControlIntegrationTests.cs`)

**検証対象要件:** 9.1

**テストケース:**
- ユーザーは自分の連絡先のみを取得
- 他のユーザーの連絡先を取得（NotFound）
- 他のユーザーの連絡先を更新（NotFound）
- 他のユーザーの連絡先を削除（NotFound）
- 重複チェックはユーザーごとに実施
- ユーザー切り替え後の正しい連絡先取得
- 複数ユーザー間での連絡先の分離
- 情報漏洩の防止（NotFoundを返す）
- データベースレベルでのアクセス制御
- ページネーション付きアクセス制御

## テストインフラストラクチャ

### IntegrationTestWebAppFactory

ASP.NET Core の `WebApplicationFactory<Program>` を継承したカスタムファクトリ。

**機能:**
- Testcontainers を使用した PostgreSQL 18 コンテナの自動起動
- テスト用データベースの自動作成とマイグレーション
- 開発環境設定（レート制限の緩和）
- テスト終了後の自動クリーンアップ

### IntegrationTestBase

統合テストの基底クラス。共通のヘルパーメソッドを提供。

**ヘルパーメソッド:**
- `RegisterAndLoginUserAsync()`: テストユーザーの登録とログイン
- `SetAuthorizationHeader()`: 認証ヘッダーの設定
- `ClearAuthorizationHeader()`: 認証ヘッダーのクリア
- `GetDbContext()`: データベースコンテキストの取得
- `CreateTestContactAsync()`: テスト用連絡先の作成

## 実行方法

### すべての統合テストを実行

```bash
cd backend
dotnet test tests/AddressBook.Tests/AddressBook.Tests.csproj --filter "FullyQualifiedName~Integration"
```

### 特定のテストクラスを実行

```bash
# 認証フローテスト
dotnet test --filter "FullyQualifiedName~AuthenticationFlowIntegrationTests"

# 連絡先CRUDテスト
dotnet test --filter "FullyQualifiedName~ContactCrudIntegrationTests"

# アクセス制御テスト
dotnet test --filter "FullyQualifiedName~AccessControlIntegrationTests"
```

### 特定のテストメソッドを実行

```bash
dotnet test --filter "FullyQualifiedName~AuthenticationFlowIntegrationTests.Login_ValidCredentials_ReturnsTokenAndUserInfo"
```

## 必要な環境

- .NET 10.0 SDK
- Docker（Testcontainers用）
- PostgreSQL 18イメージ（自動ダウンロード）

## 依存パッケージ

- `Microsoft.AspNetCore.Mvc.Testing` (10.0.1): ASP.NET Core Test Host
- `Testcontainers.PostgreSql` (4.2.0): PostgreSQL コンテナ管理
- `xUnit` (2.9.3): テストフレームワーク

## 注意事項

1. **Docker の起動**: テスト実行前に Docker が起動している必要があります
2. **ポートの競合**: PostgreSQL のデフォルトポート（5432）が使用中の場合、Testcontainers が自動的に別のポートを割り当てます
3. **テスト実行時間**: 初回実行時は PostgreSQL イメージのダウンロードに時間がかかる場合があります
4. **並列実行**: xUnit はデフォルトでテストを並列実行しますが、各テストクラスは独立したデータベースコンテナを使用します

## テスト設計の原則

1. **独立性**: 各テストは他のテストに依存せず、独立して実行可能
2. **再現性**: テストは何度実行しても同じ結果を返す
3. **クリーンアップ**: テスト終了後、データベースコンテナは自動的に削除される
4. **実環境に近い**: Testcontainers により、実際の PostgreSQL を使用してテスト
5. **包括性**: 認証、CRUD、アクセス制御など、主要な機能をカバー

## トラブルシューティング

### Docker 接続エラー

```
Error: Cannot connect to Docker daemon
```

**解決方法**: Docker Desktop を起動してください。

### ポート競合エラー

```
Error: Port 5432 is already in use
```

**解決方法**: Testcontainers が自動的に別のポートを使用するため、通常は問題ありません。それでもエラーが発生する場合は、既存の PostgreSQL サービスを停止してください。

### レート制限エラー

```
Error: 429 Too Many Requests
```

**解決方法**: `IntegrationTestWebAppFactory` が Development 環境を使用していることを確認してください。Program.cs では開発環境でレート制限が緩和されています。

## 今後の拡張

- E2Eテストの追加（Playwright）
- パフォーマンステストの追加（負荷テスト）
- セキュリティテストの追加（OWASP ZAP）
- カバレッジレポートの生成
