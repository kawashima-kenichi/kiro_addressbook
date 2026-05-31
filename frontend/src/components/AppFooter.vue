<script setup lang="ts">
const currentYear = new Date().getFullYear()
</script>

<template>
  <footer class="bg-white border-t border-gray-200 mt-auto" role="contentinfo">
    <div class="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
      <div class="py-6 md:flex md:items-center md:justify-between">
        <!-- Copyright -->
        <div class="text-center md:text-left">
          <p class="text-sm text-gray-500">
            &copy; {{ currentYear }} 住所録アプリ. All rights reserved.
          </p>
        </div>

        <!-- Links -->
        <nav class="mt-4 md:mt-0" aria-label="フッターナビゲーション">
          <ul class="flex justify-center space-x-6 md:justify-end">
            <li>
              <a
                href="#"
                class="text-sm text-gray-500 hover:text-gray-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 rounded-md px-2 py-1 transition-colors"
                @click.prevent
              >
                プライバシーポリシー
              </a>
            </li>
            <li>
              <a
                href="#"
                class="text-sm text-gray-500 hover:text-gray-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 rounded-md px-2 py-1 transition-colors"
                @click.prevent
              >
                利用規約
              </a>
            </li>
            <li>
              <a
                href="#"
                class="text-sm text-gray-500 hover:text-gray-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 rounded-md px-2 py-1 transition-colors"
                @click.prevent
              >
                お問い合わせ
              </a>
            </li>
          </ul>
        </nav>
      </div>
    </div>
  </footer>
</template>
