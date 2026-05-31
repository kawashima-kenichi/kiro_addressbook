#!/bin/bash

# フロントエンド・バックエンド統合検証スクリプト
# このスクリプトは、統合が正しく設定されているかを確認します

set -e

echo "========================================="
echo "フロントエンド・バックエンド統合検証"
echo "========================================="
echo ""

# 色の定義
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# 1. Docker Desktop の確認
echo "1. Docker Desktop の確認..."
if docker info > /dev/null 2>&1; then
    echo -e "${GREEN}✓${NC} Docker Desktop が起動しています"
else
    echo -e "${RED}✗${NC} Docker Desktop が起動していません"
    echo "   Docker Desktop を起動してから再実行してください"
    exit 1
fi
echo ""

# 2. PostgreSQL コンテナの確認
echo "2. PostgreSQL コンテナの確認..."
if docker ps | grep -q "addressbook-db"; then
    echo -e "${GREEN}✓${NC} PostgreSQL コンテナが起動しています"
else
    echo -e "${YELLOW}!${NC} PostgreSQL コンテナが起動していません"
    echo "   起動しています..."
    docker-compose up -d db
    echo "   データベースの初期化を待機中（10秒）..."
    sleep 10
    echo -e "${GREEN}✓${NC} PostgreSQL コンテナを起動しました"
fi
echo ""

# 3. バックエンドAPIの確認
echo "3. バックエンドAPIの確認..."
if curl -s -o /dev/null -w "%{http_code}" http://localhost:5147/api/contacts | grep -q "403\|401"; then
    echo -e "${GREEN}✓${NC} バックエンドAPIが起動しています"
else
    echo -e "${RED}✗${NC} バックエンドAPIが起動していません"
    echo "   以下のコマンドでバックエンドを起動してください:"
    echo "   cd backend/src/AddressBook.API && dotnet run"
    exit 1
fi
echo ""

# 4. フロントエンド環境変数の確認
echo "4. フロントエンド環境変数の確認..."
if [ -f "frontend/.env" ]; then
    echo -e "${GREEN}✓${NC} .env ファイルが存在します"
    
    if grep -q "VITE_API_BASE_URL" frontend/.env; then
        API_URL=$(grep "VITE_API_BASE_URL" frontend/.env | cut -d '=' -f2)
        echo "   API Base URL: $API_URL"
    else
        echo -e "${RED}✗${NC} VITE_API_BASE_URL が設定されていません"
        exit 1
    fi
else
    echo -e "${RED}✗${NC} .env ファイルが存在しません"
    echo "   .env.example をコピーして .env を作成してください"
    exit 1
fi
echo ""

# 5. 統合テストの実行
echo "5. 統合テストの実行..."
echo "   テストを実行中..."
cd frontend
if npx tsx test-api-integration.ts > /tmp/integration-test.log 2>&1; then
    echo -e "${GREEN}✓${NC} すべての統合テストが成功しました"
    cat /tmp/integration-test.log | tail -20
else
    echo -e "${RED}✗${NC} 統合テストが失敗しました"
    echo "   詳細:"
    cat /tmp/integration-test.log | tail -30
    exit 1
fi
cd ..
echo ""

# 6. CORS設定の確認
echo "6. CORS設定の確認..."
CORS_RESPONSE=$(curl -s -I -X OPTIONS http://localhost:5147/api/contacts \
    -H "Origin: http://localhost:5173" \
    -H "Access-Control-Request-Method: GET" | grep -i "access-control")

if echo "$CORS_RESPONSE" | grep -q "access-control-allow-origin"; then
    echo -e "${GREEN}✓${NC} CORS設定が正しく動作しています"
    echo "$CORS_RESPONSE" | sed 's/^/   /'
else
    echo -e "${RED}✗${NC} CORS設定に問題があります"
    exit 1
fi
echo ""

# 7. 最終確認
echo "========================================="
echo -e "${GREEN}✓ すべての検証が完了しました！${NC}"
echo "========================================="
echo ""
echo "次のステップ:"
echo "1. フロントエンドを起動: cd frontend && npm run dev"
echo "2. ブラウザで http://localhost:5173 にアクセス"
echo "3. ユーザー登録とログインをテスト"
echo "4. 連絡先のCRUD操作をテスト"
echo ""
