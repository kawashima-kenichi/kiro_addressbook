# フロントエンド・バックエンド統合チェックリスト

## タスク 12.1: フロントエンドとバックエンドの統合

このチェックリストは、タスク12.1の完了状況を確認するためのものです。

### ✅ 完了項目

#### 1. API ベースURL設定と環境変数

- [x] `.env` ファイルに `VITE_API_BASE_URL` を設定
- [x] `.env.example` ファイルを更新し、すべての環境変数を文書化
- [x] 環境変数から API タイムアウトとデバッグモードを設定可能に
- [x] `api.ts` で環境変数を読み込み、Axios インスタンスに適用

**ファイル**:
- `/frontend/.env`
- `/frontend/.env.example`
- `/frontend/src/services/api.ts`

#### 2. Axios インターセプターでエラーハンドリングを統合

- [x] リクエストインターセプターでJWTトークンを自動付与
- [x] リクエストインターセプターでデバッグログを出力
- [x] レスポンスインターセプターで包括的なエラーハンドリングを実装
  - [x] 401 Unauthorized: 認証状態をクリアしてログインページにリダイレクト
  - [x] 403 Forbidden: アクセス権限エラーをログ出力
  - [x] 404 Not Found: リソース未検出エラーをログ出力
  - [x] 422 Unprocessable Entity: バリデーションエラーを処理
  - [x] 429 Too Many Requests: レート制限エラーを処理
  - [x] 500+ Server Error: サーバーエラーをログ出力
  - [x] Network Error: ユーザーフレンドリーなメッセージを返却
- [x] エラーレスポンスの型定義を追加 (`ApiErrorResponse`)
- [x] エラーオブジェクトを整形して返却

**ファイル**:
- `/frontend/src/services/api.ts`

#### 3. CORS設定の最終調整

- [x] バックエンドの `appsettings.json` でCORS設定を確認
- [x] バックエンドの `appsettings.Development.json` でCORS設定を確認
- [x] `Program.cs` でCORSミドルウェアの順序を確認
  - [x] `UseHttpsRedirection()` の後
  - [x] `UseRateLimiter()` の前
  - [x] `UseAuthentication()` の前
- [x] 許可されるオリジンを確認:
  - `http://localhost:5173` (Vite dev server)
  - `http://localhost:3000` (代替ポート)

**ファイル**:
- `/backend/src/AddressBook.API/appsettings.json`
- `/backend/src/AddressBook.API/appsettings.Development.json`
- `/backend/src/AddressBook.API/Program.cs`

#### 4. 統合ドキュメント作成

- [x] 統合ガイドを作成 (`FRONTEND_BACKEND_INTEGRATION.md`)
  - [x] 環境変数設定
  - [x] CORS設定
  - [x] Axios インターセプター
  - [x] APIエンドポイント一覧
  - [x] 起動手順
  - [x] 統合テスト手順
  - [x] トラブルシューティング
  - [x] 本番環境への展開
  - [x] パフォーマンス最適化
  - [x] 監視とログ
- [x] 統合テストスクリプトを作成 (`test-api-integration.ts`)

**ファイル**:
- `/FRONTEND_BACKEND_INTEGRATION.md`
- `/frontend/test-api-integration.ts`
- `/frontend/INTEGRATION_CHECKLIST.md` (このファイル)

### ⏳ 検証待ち項目

以下の項目は、データベースとバックエンドが正常に起動している状態で検証する必要があります。

#### 5. 全APIエンドポイントとフロントエンドの接続を確認

**前提条件**:
1. Docker Desktop が起動していること
2. PostgreSQL コンテナが起動していること: `docker-compose up -d db`
3. バックエンドAPIが起動していること: `dotnet run` または `docker-compose up -d api`

**検証手順**:

```bash
# 1. データベースを起動
cd /path/to/project
docker-compose up -d db

# 2. データベースが起動するまで待機（約5-10秒）
docker-compose logs -f db
# "database system is ready to accept connections" が表示されたらCtrl+Cで終了

# 3. バックエンドを起動
cd backend/src/AddressBook.API
dotnet run

# 別のターミナルで以下を実行

# 4. 統合テストを実行
cd frontend
npx tsx test-api-integration.ts
```

**期待される結果**:
```
🎉 すべてのテストが成功しました！
合計: 9件 | 成功: 9件 | 失敗: 0件
```

**検証項目**:
- [ ] CORS設定が正しく動作している
- [ ] POST /api/auth/register が動作している
- [ ] POST /api/auth/login が動作している
- [ ] GET /api/contacts が動作している
- [ ] POST /api/contacts が動作している
- [ ] GET /api/contacts/{id} が動作している
- [ ] PUT /api/contacts/{id} が動作している
- [ ] DELETE /api/contacts/{id} が動作している
- [ ] POST /api/auth/logout が動作している

#### 6. フロントエンドからの手動テスト

**検証手順**:

```bash
# 1. フロントエンドを起動
cd frontend
npm run dev

# 2. ブラウザで http://localhost:5173 にアクセス
```

**検証項目**:
- [ ] ユーザー登録ページが表示される
- [ ] ユーザー登録が成功する
- [ ] ログインページが表示される
- [ ] ログインが成功する
- [ ] 連絡先一覧ページが表示される
- [ ] 連絡先を作成できる
- [ ] 連絡先を編集できる
- [ ] 連絡先を削除できる
- [ ] ログアウトできる
- [ ] エラーメッセージが適切に表示される
- [ ] ネットワークエラー時に適切なメッセージが表示される

### 📝 注意事項

1. **データベース接続エラー**: 現在、PostgreSQLコンテナが起動していないため、バックエンドAPIが正常に動作しません。Docker Desktopを起動し、`docker-compose up -d db` を実行してください。

2. **CORS エラー**: バックエンドが起動していない状態でフロントエンドからAPIにアクセスすると、CORSエラーが発生します。これは正常な動作です。

3. **環境変数**: `.env` ファイルを変更した場合は、フロントエンドを再起動してください（`npm run dev` を再実行）。

4. **デバッグモード**: `VITE_DEBUG_MODE=true` を設定すると、ブラウザコンソールにAPIリクエスト/レスポンスのログが出力されます。本番環境では `false` に設定してください。

### 🎯 次のステップ

1. Docker Desktop を起動
2. PostgreSQL コンテナを起動: `docker-compose up -d db`
3. バックエンドAPIを起動: `cd backend/src/AddressBook.API && dotnet run`
4. 統合テストを実行: `cd frontend && npx tsx test-api-integration.ts`
5. すべてのテストが成功したら、手動テストを実行
6. すべての検証項目にチェックを入れる
7. タスク12.1を完了としてマーク

### 📚 参考資料

- [フロントエンド・バックエンド統合ガイド](/FRONTEND_BACKEND_INTEGRATION.md)
- [要件定義書](/.kiro/specs/address-book-webapp/requirements.md)
- [設計書](/.kiro/specs/address-book-webapp/design.md)
- [タスク一覧](/.kiro/specs/address-book-webapp/tasks.md)
