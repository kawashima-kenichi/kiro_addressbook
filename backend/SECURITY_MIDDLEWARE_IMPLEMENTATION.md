# セキュリティミドルウェア実装ドキュメント

## 概要

タスク 6.2「CORSとセキュリティミドルウェアの設定」の実装完了。

## 実装内容

### 1. CORSポリシーの設定（要件 9.2）

**場所**: `Program.cs`

**実装内容**:
- 許可されたオリジンからのリクエストのみを受け入れる
- すべてのHTTPメソッドとヘッダーを許可
- 認証情報（クッキー）の送信を許可
- ワイルドカードサブドメインのサポート

**設定**:
```json
"Cors": {
  "AllowedOrigins": ["http://localhost:5173", "http://localhost:3000"]
}
```

### 2. レート制限ミドルウェアの設定（要件 9.6）

**場所**: `Program.cs`

**実装内容**:

#### グローバルレート制限
- **制限**: 1分あたり100リクエスト
- **パーティション**: ユーザーID（認証済み）またはIPアドレス（未認証）
- **アルゴリズム**: Fixed Window

#### 認証エンドポイント用レート制限（要件 1.3）
- **制限**: 15分あたり5リクエスト
- **パーティション**: IPアドレス
- **適用対象**: `/api/auth/*` エンドポイント
- **目的**: ブルートフォース攻撃の防止

**レート制限超過時のレスポンス**:
```json
{
  "message": "リクエストが多すぎます。しばらくしてから再試行してください。",
  "retryAfter": 60
}
```

**HTTPステータスコード**: 429 Too Many Requests

### 3. HTTPSリダイレクトの設定（要件 9.2）

**場所**: `Program.cs`

**実装内容**:
- すべてのHTTPリクエストをHTTPSにリダイレクト
- `app.UseHttpsRedirection()` ミドルウェアを使用

### 4. グローバル例外ハンドラーの実装（要件 6.5, 9.6）

**場所**: `Middleware/GlobalExceptionHandlerMiddleware.cs`

**実装内容**:
- すべての未処理例外をキャッチ
- ユニークなエラーIDを生成（形式: `err_YYYYMMDD_<32文字のGUID>`）
- 構造化ログでエラーを記録（タイムスタンプ、ユーザーID、アクション種別、IPアドレス）
- ユーザーフレンドリーなエラーメッセージを返す

**エラーレスポンス形式**:
```json
{
  "message": "システムエラーが発生しました。もう一度お試しください。",
  "errorId": "err_20260530_abc123...",
  "timestamp": "2026-05-30T12:34:56.789Z"
}
```

**ログ記録内容**:
- ErrorId: エラー追跡用のユニークID
- Path: リクエストパス
- Method: HTTPメソッド
- UserId: ユーザーID（認証済みの場合）
- IpAddress: クライアントIPアドレス
- Exception: 例外の詳細

## ミドルウェアの実行順序

```
1. GlobalExceptionHandler（例外キャッチ）
2. SerilogRequestLogging（リクエストログ）
3. HttpsRedirection（HTTPS強制）
4. Cors（CORS検証）
5. RateLimiter（レート制限）
6. Authentication（認証）
7. Authorization（認可）
8. AuditLogging（監査ログ）
9. Controllers（エンドポイント処理）
```

## テスト

### 実装したテスト

**場所**: `tests/AddressBook.Tests/Middleware/GlobalExceptionHandlerMiddlewareTests.cs`

**テストケース**:
1. `InvokeAsync_WhenExceptionThrown_ReturnsInternalServerError` - 例外発生時に500エラーを返す
2. `InvokeAsync_WhenExceptionThrown_ReturnsErrorResponseWithErrorId` - エラーIDを含むレスポンスを返す
3. `InvokeAsync_WhenExceptionThrown_LogsErrorWithDetails` - エラーの詳細をログに記録する
4. `InvokeAsync_WhenNoException_CallsNextMiddleware` - 例外がない場合は次のミドルウェアを呼び出す
5. `InvokeAsync_WhenExceptionThrown_ErrorIdFollowsCorrectFormat` - エラーIDが正しい形式に従う

### テスト結果

```
テスト概要: 合計: 126, 失敗数: 0, 成功数: 126, スキップ済み数: 0
```

すべてのテストが成功しました。

## 設定ファイル

### appsettings.json

```json
{
  "Cors": {
    "AllowedOrigins": ["http://localhost:5173", "http://localhost:3000"]
  },
  "RateLimiting": {
    "GlobalLimit": 100,
    "GlobalWindowMinutes": 1,
    "AuthLimit": 5,
    "AuthWindowMinutes": 15
  }
}
```

## セキュリティ考慮事項

1. **CORS**: 本番環境では、`AllowedOrigins` を実際のフロントエンドドメインに制限する
2. **レート制限**: 本番環境では、より厳格な制限を設定することを推奨
3. **エラーメッセージ**: 技術的な詳細を隠蔽し、ユーザーフレンドリーなメッセージのみを表示
4. **ログ記録**: エラーIDを使用して、ユーザーサポート時にログを追跡可能

## 要件との対応

- ✅ **要件 6.5**: データベースエラー時のエラーログ記録（エラーID付き）
- ✅ **要件 9.2**: HTTPS通信（HTTPSリダイレクト、CORS設定）
- ✅ **要件 9.6**: 不正アクセス試行の防止（レート制限、セキュリティ違反のログ記録）
- ✅ **要件 1.3**: ログイン試行制限（認証エンドポイントの厳格なレート制限）

## 今後の改善案

1. **分散レート制限**: Redis等を使用した分散環境でのレート制限
2. **動的CORS設定**: データベースから動的にCORS設定を読み込む
3. **エラー通知**: 重大なエラー発生時の管理者への通知機能
4. **メトリクス収集**: レート制限の統計情報を収集・可視化
