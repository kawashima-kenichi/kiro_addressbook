# クイックスタートガイド

## 🚀 5分で始める住所録WEBアプリケーション

このガイドに従って、フロントエンドとバックエンドを起動し、統合を確認してください。

## 前提条件

以下がインストールされていることを確認してください：

- ✅ Docker Desktop
- ✅ .NET 10.0 SDK
- ✅ Node.js 18+

## ステップ1: Docker Desktop を起動

1. Docker Desktop アプリケーションを起動
2. Docker が起動するまで待機（通常30秒〜1分）

## ステップ2: データベースを起動

```bash
# プロジェクトルートディレクトリで実行
docker-compose up -d db
```

データベースが起動するまで待機（約5-10秒）：

```bash
# ログを確認
docker-compose logs -f db

# "database system is ready to accept connections" が表示されたら
# Ctrl+C で終了
```

## ステップ3: バックエンドAPIを起動

**ターミナル1**:
```bash
cd backend/src/AddressBook.API
dotnet run
```

以下のメッセージが表示されたら成功：
```
Now listening on: http://localhost:5000
Application started. Press Ctrl+C to shut down.
```

## ステップ4: フロントエンドを起動

**ターミナル2**（新しいターミナルを開く）:
```bash
cd frontend
npm install  # 初回のみ
npm run dev
```

以下のメッセージが表示されたら成功：
```
VITE v5.x.x  ready in xxx ms

➜  Local:   http://localhost:5173/
```

## ステップ5: ブラウザでアクセス

ブラウザで以下のURLにアクセス：

**http://localhost:5173**

## ステップ6: 動作確認

### 1. ユーザー登録

1. 「新規登録」ボタンをクリック
2. メールアドレスとパスワードを入力
   - メールアドレス: `test@example.com`
   - パスワード: `Test1234!` （大文字、小文字、数字、特殊文字を含む8文字以上）
3. 「登録」ボタンをクリック

### 2. ログイン

1. 登録したメールアドレスとパスワードでログイン
2. 連絡先一覧ページにリダイレクトされる

### 3. 連絡先を作成

1. 「新しい連絡先を追加」ボタンをクリック
2. 連絡先情報を入力
   - 名前: `田中太郎` （必須）
   - 住所: `東京都渋谷区` （任意）
   - 電話番号: `03-1234-5678` （任意）
3. 「保存」ボタンをクリック

### 4. 連絡先を編集

1. 連絡先カードの「編集」ボタンをクリック
2. 情報を変更
3. 「保存」ボタンをクリック

### 5. 連絡先を削除

1. 連絡先カードの「削除」ボタンをクリック
2. 確認ダイアログで「はい」をクリック

### 6. ログアウト

1. ヘッダーの「ログアウト」ボタンをクリック
2. ログインページにリダイレクトされる

## 🧪 自動テストを実行

**ターミナル3**（新しいターミナルを開く）:
```bash
cd frontend
npx tsx test-api-integration.ts
```

期待される結果：
```
🎉 すべてのテストが成功しました！
合計: 9件 | 成功: 9件 | 失敗: 0件
```

## 🔍 統合検証スクリプトを実行

すべてを自動でチェック：

```bash
./verify-integration.sh
```

このスクリプトは以下を確認します：
1. Docker Desktop が起動しているか
2. PostgreSQL コンテナが起動しているか
3. バックエンドAPIが起動しているか
4. フロントエンド環境変数が設定されているか
5. 統合テストが成功するか
6. CORS設定が正しいか

## ❌ トラブルシューティング

### Docker Desktop が起動しない

**症状**: `docker: command not found` または `Cannot connect to the Docker daemon`

**解決方法**:
1. Docker Desktop アプリケーションを起動
2. Docker が完全に起動するまで待機（通常30秒〜1分）
3. ターミナルで `docker ps` を実行して確認

### データベース接続エラー

**症状**: バックエンドが "An error occurred using the connection to database" エラーを表示

**解決方法**:
```bash
# PostgreSQLコンテナが起動しているか確認
docker ps | grep postgres

# 起動していない場合
docker-compose up -d db

# ログを確認
docker-compose logs -f db
```

### ポート5000が使用中

**症状**: バックエンドが "Address already in use" エラーを表示

**解決方法**:
```bash
# ポート5000を使用しているプロセスを確認
lsof -i :5000

# プロセスを終了
kill -9 <PID>

# または、別のポートを使用
# appsettings.Development.json で "ASPNETCORE_URLS" を変更
```

### フロントエンドがAPIに接続できない

**症状**: ブラウザコンソールに "Network Error" または "CORS policy" エラー

**解決方法**:
1. バックエンドが起動しているか確認: `curl http://localhost:5000/api/contacts`
2. `.env` ファイルの `VITE_API_BASE_URL` が正しいか確認
3. バックエンドの `appsettings.Development.json` で `Cors:AllowedOrigins` を確認
4. ブラウザのキャッシュをクリア

## 📚 詳細なドキュメント

- [フロントエンド・バックエンド統合ガイド](FRONTEND_BACKEND_INTEGRATION.md)
- [タスク12.1完了サマリー](TASK_12.1_SUMMARY.md)
- [統合チェックリスト](frontend/INTEGRATION_CHECKLIST.md)

## 🎯 次のステップ

統合が正常に動作したら：

1. ✅ タスク12.1を完了としてマーク
2. 📝 次のタスク（12.2 バックエンド統合テストの作成）に進む
3. 🚀 本番環境への展開を準備

## 💡 ヒント

- **デバッグモード**: `.env` で `VITE_DEBUG_MODE=true` を設定すると、ブラウザコンソールにAPIリクエスト/レスポンスのログが表示されます
- **ホットリロード**: フロントエンドのコードを変更すると、自動的にブラウザがリロードされます
- **バックエンドの再起動**: バックエンドのコードを変更した場合は、`Ctrl+C` で停止して `dotnet run` で再起動してください

## 🆘 サポート

問題が解決しない場合は、以下を確認してください：

1. [トラブルシューティングガイド](FRONTEND_BACKEND_INTEGRATION.md#7-トラブルシューティング)
2. バックエンドのログ: `backend/src/AddressBook.API/logs/log-YYYYMMDD.txt`
3. ブラウザのコンソール（F12キーで開く）

---

**Happy Coding! 🎉**
