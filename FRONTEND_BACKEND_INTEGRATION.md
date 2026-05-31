# フロントエンドとバックエンドの統合ガイド

## 概要

このドキュメントは、Vue.js フロントエンドと ASP.NET Core バックエンドの統合に関する設定と確認手順を説明します。

## 1. 環境変数設定

### フロントエンド (.env)

```bash
# API Base URL
VITE_API_BASE_URL=http://localhost:5000

# API Timeout (ミリ秒)
VITE_API_TIMEOUT=10000

# Enable Debug Mode
VITE_DEBUG_MODE=true
```

### バックエンド (appsettings.Development.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=address_book_dev;Username=postgres;Password=postgres"
  },
  "JwtSettings": {
    "Issuer": "AddressBook.API",
    "Audience": "AddressBook.Client",
    "Key": "DevEnvironmentSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "ExpirationHours": 24
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:5173", "http://localhost:3000"]
  }
}
```

## 2. CORS設定

### バックエンド (Program.cs)

CORS設定は既に実装済みです：

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        policy.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .SetIsOriginAllowedToAllowWildcardSubdomains();
    });
});

// ミドルウェアパイプラインで使用
app.UseCors("AllowFrontend");
```

### 許可されるオリジン

- 開発環境: `http://localhost:5173` (Vite dev server)
- 開発環境: `http://localhost:3000` (代替ポート)
- 本番環境: `appsettings.json` で設定

## 3. Axios インターセプター

### リクエストインターセプター

- JWTトークンを自動的にAuthorizationヘッダーに追加
- デバッグモードでリクエスト情報をログ出力

```typescript
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
  }
)
```

### レスポンスインターセプター

エラーハンドリングを統合：

- **401 Unauthorized**: トークン期限切れ → 認証状態をクリアしてログインページにリダイレクト
- **403 Forbidden**: アクセス権限なし → エラーログ出力
- **404 Not Found**: リソースが見つからない → エラーログ出力
- **422 Unprocessable Entity**: バリデーションエラー → エラー詳細を返却
- **429 Too Many Requests**: レート制限超過 → 警告ログ出力
- **500+ Server Error**: サーバーエラー → エラーログ出力
- **Network Error**: サーバーに到達できない → ユーザーフレンドリーなメッセージを返却

```typescript
apiClient.interceptors.response.use(
  (response) => {
    if (DEBUG_MODE) {
      console.log('[API Response]', response.status, response.config.url)
    }
    return response
  },
  (error: AxiosError<ApiErrorResponse>) => {
    // エラーハンドリングロジック
    // ...
  }
)
```

## 4. APIエンドポイント一覧

### 認証エンドポイント

| メソッド | エンドポイント | 説明 | 認証 |
|---------|--------------|------|------|
| POST | `/api/auth/register` | ユーザー登録 | 不要 |
| POST | `/api/auth/login` | ログイン | 不要 |
| POST | `/api/auth/logout` | ログアウト | 必要 |

### 連絡先エンドポイント

| メソッド | エンドポイント | 説明 | 認証 |
|---------|--------------|------|------|
| GET | `/api/contacts` | 連絡先一覧取得（ページネーション付き） | 必要 |
| GET | `/api/contacts/{id}` | 連絡先詳細取得 | 必要 |
| POST | `/api/contacts` | 連絡先作成 | 必要 |
| PUT | `/api/contacts/{id}` | 連絡先更新 | 必要 |
| DELETE | `/api/contacts/{id}` | 連絡先削除 | 必要 |

## 5. 起動手順

### 前提条件

- Docker Desktop がインストールされ、起動していること
- Node.js 18+ がインストールされていること
- .NET 10.0 SDK がインストールされていること

### 1. データベースの起動

```bash
# プロジェクトルートディレクトリで実行
docker-compose up -d db
```

データベースが起動するまで待機（約5-10秒）：

```bash
docker-compose logs -f db
# "database system is ready to accept connections" が表示されたらOK
```

### 2. バックエンドAPIの起動

```bash
cd backend/src/AddressBook.API
dotnet run
```

または、Docker Composeで起動：

```bash
# プロジェクトルートディレクトリで実行
docker-compose up -d api
```

バックエンドが起動したら、以下のURLで確認：
- API: http://localhost:5000
- OpenAPI (Swagger): http://localhost:5000/openapi/v1.json

### 3. フロントエンドの起動

```bash
cd frontend
npm install  # 初回のみ
npm run dev
```

フロントエンドが起動したら、以下のURLでアクセス：
- フロントエンド: http://localhost:5173

## 6. 統合テスト

### 手動テスト

1. ブラウザで http://localhost:5173 にアクセス
2. ユーザー登録ページで新規ユーザーを作成
3. ログインページでログイン
4. 連絡先一覧ページで連絡先を作成・編集・削除

### 自動テスト

統合テストスクリプトを実行：

```bash
cd frontend
npx tsx test-api-integration.ts
```

期待される結果：

```
🚀 API統合テスト開始...

================================================================================
📊 テスト結果
================================================================================
✅ CORS設定
   CORS設定確認: Origin=http://localhost:5173, Methods=GET,POST,PUT,DELETE
✅ POST /api/auth/register
   登録成功: 201
✅ POST /api/auth/login
   ログイン成功、トークン取得
✅ GET /api/contacts
   連絡先一覧取得成功: 0件
✅ POST /api/contacts
   連絡先作成成功: ID xxx
✅ GET /api/contacts/{id}
   連絡先詳細取得成功
✅ PUT /api/contacts/{id}
   連絡先更新成功
✅ DELETE /api/contacts/{id}
   連絡先削除成功
✅ POST /api/auth/logout
   ログアウト成功

================================================================================
合計: 9件 | 成功: 9件 | 失敗: 0件
================================================================================

🎉 すべてのテストが成功しました！
```

## 7. トラブルシューティング

### データベース接続エラー

**症状**: バックエンドが起動時に "An error occurred using the connection to database" エラーを表示

**解決方法**:
1. PostgreSQLコンテナが起動しているか確認: `docker ps | grep postgres`
2. 起動していない場合: `docker-compose up -d db`
3. 接続文字列が正しいか確認: `appsettings.Development.json`

### CORS エラー

**症状**: ブラウザコンソールに "Access to XMLHttpRequest at 'http://localhost:5000/api/...' from origin 'http://localhost:5173' has been blocked by CORS policy"

**解決方法**:
1. バックエンドの `appsettings.Development.json` で `Cors:AllowedOrigins` にフロントエンドのURLが含まれているか確認
2. バックエンドを再起動
3. ブラウザのキャッシュをクリア

### 401 Unauthorized エラー

**症状**: APIリクエストが401エラーを返す

**解決方法**:
1. ログインしているか確認
2. トークンが有効期限内か確認（24時間）
3. LocalStorageに `auth_token` が保存されているか確認
4. 再ログインを試す

### ネットワークエラー

**症状**: "サーバーに接続できません。ネットワーク接続を確認してください。"

**解決方法**:
1. バックエンドAPIが起動しているか確認: `curl http://localhost:5000/api/contacts`
2. `.env` ファイルの `VITE_API_BASE_URL` が正しいか確認
3. ファイアウォールやセキュリティソフトがポート5000をブロックしていないか確認

## 8. 本番環境への展開

### 環境変数の設定

**フロントエンド**:
```bash
VITE_API_BASE_URL=https://api.yourdomain.com
VITE_API_TIMEOUT=10000
VITE_DEBUG_MODE=false
```

**バックエンド**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=prod-db-host;Database=address_book;Username=prod_user;Password=secure_password"
  },
  "JwtSettings": {
    "Key": "ProductionSecureKeyThatIsAtLeast32CharactersLong!"
  },
  "Cors": {
    "AllowedOrigins": ["https://yourdomain.com"]
  }
}
```

### HTTPS設定

本番環境では必ずHTTPSを使用してください：

1. SSL証明書を取得（Let's Encrypt推奨）
2. Nginxまたはリバースプロキシを設定
3. HTTPからHTTPSへのリダイレクトを設定
4. HSTS（HTTP Strict Transport Security）を有効化

### セキュリティチェックリスト

- [ ] JWT秘密鍵を本番用の強力なものに変更
- [ ] データベースパスワードを強力なものに変更
- [ ] CORS設定を本番ドメインのみに制限
- [ ] HTTPS通信を強制
- [ ] レート制限を適切に設定
- [ ] 監査ログを有効化
- [ ] エラーログを監視システムに統合
- [ ] バックアップを自動化

## 9. パフォーマンス最適化

### フロントエンド

- [ ] 本番ビルドを使用: `npm run build`
- [ ] Gzip圧縮を有効化
- [ ] 静的アセットをCDNで配信
- [ ] コード分割とLazy Loadingを実装
- [ ] 画像を最適化

### バックエンド

- [ ] データベースインデックスを最適化
- [ ] クエリパフォーマンスを監視
- [ ] レスポンスキャッシュを実装
- [ ] 接続プーリングを設定
- [ ] 非同期処理を活用

## 10. 監視とログ

### ログ出力先

- **バックエンド**: `backend/src/AddressBook.API/logs/log-YYYYMMDD.txt`
- **フロントエンド**: ブラウザコンソール（開発環境）

### 監視項目

- APIレスポンス時間
- エラー率
- データベース接続数
- メモリ使用量
- CPU使用率
- ディスク使用量

## まとめ

このガイドに従って、フロントエンドとバックエンドの統合を完了してください。問題が発生した場合は、トラブルシューティングセクションを参照してください。
