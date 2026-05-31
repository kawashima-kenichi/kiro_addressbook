/**
 * API統合テストスクリプト
 * フロントエンドとバックエンドの接続を確認
 */

import axios from 'axios'

const API_BASE_URL = 'http://localhost:5147'

interface TestResult {
  name: string
  status: 'PASS' | 'FAIL'
  message: string
}

const results: TestResult[] = []

async function testRegisterEndpoint() {
  try {
    const response = await axios.post(`${API_BASE_URL}/api/auth/register`, {
      email: `test${Date.now()}@example.com`,
      password: 'Test1234!',
    })
    
    if (response.status === 200 || response.status === 201) {
      results.push({
        name: 'POST /api/auth/register',
        status: 'PASS',
        message: `登録成功: ${response.status}`,
      })
      return response.data
    }
  } catch (error: any) {
    results.push({
      name: 'POST /api/auth/register',
      status: 'FAIL',
      message: `エラー: ${error.response?.status} - ${error.response?.data?.message || error.message}`,
    })
  }
  return null
}

async function testLoginEndpoint(email: string, password: string) {
  try {
    const response = await axios.post(`${API_BASE_URL}/api/auth/login`, {
      email,
      password,
    })
    
    if (response.status === 200 && response.data.token) {
      results.push({
        name: 'POST /api/auth/login',
        status: 'PASS',
        message: `ログイン成功、トークン取得`,
      })
      return response.data.token
    }
  } catch (error: any) {
    results.push({
      name: 'POST /api/auth/login',
      status: 'FAIL',
      message: `エラー: ${error.response?.status} - ${error.response?.data?.message || error.message}`,
    })
  }
  return null
}

async function testGetContacts(token: string) {
  try {
    const response = await axios.get(`${API_BASE_URL}/api/contacts`, {
      headers: { Authorization: `Bearer ${token}` },
    })
    
    if (response.status === 200) {
      results.push({
        name: 'GET /api/contacts',
        status: 'PASS',
        message: `連絡先一覧取得成功: ${response.data.contacts?.length || 0}件`,
      })
      return response.data
    }
  } catch (error: any) {
    results.push({
      name: 'GET /api/contacts',
      status: 'FAIL',
      message: `エラー: ${error.response?.status} - ${error.response?.data?.message || error.message}`,
    })
  }
  return null
}

async function testCreateContact(token: string) {
  try {
    const response = await axios.post(
      `${API_BASE_URL}/api/contacts`,
      {
        name: `テスト連絡先 ${Date.now()}`,
        address: '東京都渋谷区',
        phoneNumber: '03-1234-5678',
      },
      {
        headers: { Authorization: `Bearer ${token}` },
      }
    )
    
    if (response.status === 200 || response.status === 201) {
      results.push({
        name: 'POST /api/contacts',
        status: 'PASS',
        message: `連絡先作成成功: ID ${response.data.id}`,
      })
      return response.data
    }
  } catch (error: any) {
    results.push({
      name: 'POST /api/contacts',
      status: 'FAIL',
      message: `エラー: ${error.response?.status} - ${error.response?.data?.message || error.message}`,
    })
  }
  return null
}

async function testGetContactById(token: string, contactId: string) {
  try {
    const response = await axios.get(`${API_BASE_URL}/api/contacts/${contactId}`, {
      headers: { Authorization: `Bearer ${token}` },
    })
    
    if (response.status === 200) {
      results.push({
        name: 'GET /api/contacts/{id}',
        status: 'PASS',
        message: `連絡先詳細取得成功`,
      })
      return response.data
    }
  } catch (error: any) {
    results.push({
      name: 'GET /api/contacts/{id}',
      status: 'FAIL',
      message: `エラー: ${error.response?.status} - ${error.response?.data?.message || error.message}`,
    })
  }
  return null
}

async function testUpdateContact(token: string, contactId: string) {
  try {
    const response = await axios.put(
      `${API_BASE_URL}/api/contacts/${contactId}`,
      {
        name: `更新されたテスト連絡先 ${Date.now()}`,
        address: '東京都新宿区',
        phoneNumber: '03-9876-5432',
      },
      {
        headers: { Authorization: `Bearer ${token}` },
      }
    )
    
    if (response.status === 200) {
      results.push({
        name: 'PUT /api/contacts/{id}',
        status: 'PASS',
        message: `連絡先更新成功`,
      })
      return response.data
    }
  } catch (error: any) {
    results.push({
      name: 'PUT /api/contacts/{id}',
      status: 'FAIL',
      message: `エラー: ${error.response?.status} - ${error.response?.data?.message || error.message}`,
    })
  }
  return null
}

async function testDeleteContact(token: string, contactId: string) {
  try {
    const response = await axios.delete(`${API_BASE_URL}/api/contacts/${contactId}`, {
      headers: { Authorization: `Bearer ${token}` },
    })
    
    if (response.status === 200 || response.status === 204) {
      results.push({
        name: 'DELETE /api/contacts/{id}',
        status: 'PASS',
        message: `連絡先削除成功`,
      })
      return true
    }
  } catch (error: any) {
    results.push({
      name: 'DELETE /api/contacts/{id}',
      status: 'FAIL',
      message: `エラー: ${error.response?.status} - ${error.response?.data?.message || error.message}`,
    })
  }
  return false
}

async function testLogout(token: string) {
  try {
    const response = await axios.post(
      `${API_BASE_URL}/api/auth/logout`,
      {},
      {
        headers: { Authorization: `Bearer ${token}` },
      }
    )
    
    if (response.status === 200) {
      results.push({
        name: 'POST /api/auth/logout',
        status: 'PASS',
        message: `ログアウト成功`,
      })
      return true
    }
  } catch (error: any) {
    results.push({
      name: 'POST /api/auth/logout',
      status: 'FAIL',
      message: `エラー: ${error.response?.status} - ${error.response?.data?.message || error.message}`,
    })
  }
  return false
}

async function testCORS() {
  try {
    const response = await axios.options(`${API_BASE_URL}/api/contacts`, {
      headers: {
        'Origin': 'http://localhost:5173',
        'Access-Control-Request-Method': 'GET',
      },
    })
    
    const allowOrigin = response.headers['access-control-allow-origin']
    const allowMethods = response.headers['access-control-allow-methods']
    
    if (allowOrigin && allowMethods) {
      results.push({
        name: 'CORS設定',
        status: 'PASS',
        message: `CORS設定確認: Origin=${allowOrigin}, Methods=${allowMethods}`,
      })
      return true
    }
  } catch (error: any) {
    results.push({
      name: 'CORS設定',
      status: 'FAIL',
      message: `エラー: ${error.message}`,
    })
  }
  return false
}

async function runTests() {
  console.log('🚀 API統合テスト開始...\n')
  
  // 1. CORS設定テスト
  await testCORS()
  
  // 2. ユーザー登録テスト
  const email = `test${Date.now()}@example.com`
  const password = 'Test1234!'
  const registerData = await testRegisterEndpoint()
  
  if (!registerData) {
    console.log('❌ 登録に失敗したため、以降のテストをスキップします')
    printResults()
    return
  }
  
  // 3. ログインテスト
  const token = await testLoginEndpoint(email, password)
  
  if (!token) {
    console.log('❌ ログインに失敗したため、以降のテストをスキップします')
    printResults()
    return
  }
  
  // 4. 連絡先一覧取得テスト
  await testGetContacts(token)
  
  // 5. 連絡先作成テスト
  const contact = await testCreateContact(token)
  
  if (!contact) {
    console.log('❌ 連絡先作成に失敗したため、以降のテストをスキップします')
    printResults()
    return
  }
  
  // 6. 連絡先詳細取得テスト
  await testGetContactById(token, contact.id)
  
  // 7. 連絡先更新テスト
  await testUpdateContact(token, contact.id)
  
  // 8. 連絡先削除テスト
  await testDeleteContact(token, contact.id)
  
  // 9. ログアウトテスト
  await testLogout(token)
  
  printResults()
}

function printResults() {
  console.log('\n' + '='.repeat(80))
  console.log('📊 テスト結果')
  console.log('='.repeat(80))
  
  results.forEach((result) => {
    const icon = result.status === 'PASS' ? '✅' : '❌'
    console.log(`${icon} ${result.name}`)
    console.log(`   ${result.message}`)
  })
  
  const passCount = results.filter((r) => r.status === 'PASS').length
  const failCount = results.filter((r) => r.status === 'FAIL').length
  
  console.log('\n' + '='.repeat(80))
  console.log(`合計: ${results.length}件 | 成功: ${passCount}件 | 失敗: ${failCount}件`)
  console.log('='.repeat(80))
  
  if (failCount === 0) {
    console.log('\n🎉 すべてのテストが成功しました！')
  } else {
    console.log('\n⚠️  一部のテストが失敗しました')
  }
}

// テスト実行
runTests().catch((error) => {
  console.error('❌ テスト実行中にエラーが発生しました:', error)
  process.exit(1)
})
