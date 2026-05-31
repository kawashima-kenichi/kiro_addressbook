<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const route = useRoute()
const authStore = useAuthStore()

const isAuthenticated = computed(() => authStore.isAuthenticated)

interface NavItem {
  name: string
  path: string
  icon: string
  ariaLabel: string
}

const navItems: NavItem[] = [
  {
    name: '連絡先一覧',
    path: '/contacts',
    icon: 'M15 19.128a9.38 9.38 0 002.625.372 9.337 9.337 0 004.121-.952 4.125 4.125 0 00-7.533-2.493M15 19.128v-.003c0-1.113-.285-2.16-.786-3.07M15 19.128v.106A12.318 12.318 0 018.624 21c-2.331 0-4.512-.645-6.374-1.766l-.001-.109a6.375 6.375 0 0111.964-3.07M12 6.375a3.375 3.375 0 11-6.75 0 3.375 3.375 0 016.75 0zm8.25 2.25a2.625 2.625 0 11-5.25 0 2.625 2.625 0 015.25 0z',
    ariaLabel: '連絡先一覧ページへ移動',
  },
]

const isActive = (path: string) => {
  return route.path === path || route.path.startsWith(path + '/')
}
</script>

<template>
  <nav
    v-if="isAuthenticated"
    class="bg-white border-b border-gray-200"
    aria-label="メインナビゲーション"
  >
    <div class="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
      <div class="flex h-12 items-center space-x-8">
        <router-link
          v-for="item in navItems"
          :key="item.path"
          :to="item.path"
          :class="[
            'inline-flex items-center px-3 py-2 text-sm font-medium rounded-md transition-colors',
            'focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2',
            isActive(item.path)
              ? 'text-indigo-600 bg-indigo-50'
              : 'text-gray-700 hover:text-indigo-600 hover:bg-gray-50',
          ]"
          :aria-label="item.ariaLabel"
          :aria-current="isActive(item.path) ? 'page' : undefined"
        >
          <svg
            class="h-5 w-5 mr-2"
            fill="none"
            viewBox="0 0 24 24"
            stroke-width="1.5"
            stroke="currentColor"
            aria-hidden="true"
          >
            <path stroke-linecap="round" stroke-linejoin="round" :d="item.icon" />
          </svg>
          {{ item.name }}
        </router-link>
      </div>
    </div>
  </nav>
</template>
