<script setup lang="ts">
import type { ContactDto } from '@/types/contact'

interface Props {
  contact: ContactDto
}

defineProps<Props>()

const emit = defineEmits<{
  click: [contact: ContactDto]
}>()

const handleClick = (contact: ContactDto) => {
  emit('click', contact)
}
</script>

<template>
  <div
    @click="handleClick(contact)"
    class="cursor-pointer rounded-lg border border-gray-200 bg-white p-4 shadow-sm transition-all hover:border-indigo-500 hover:shadow-md"
    data-testid="contact-card"
  >
    <div class="flex items-start justify-between">
      <div class="flex-1">
        <h3 class="text-lg font-semibold text-gray-900" data-testid="contact-name">
          {{ contact.name }}
        </h3>
        <div class="mt-2 space-y-1">
          <p class="text-sm text-gray-600" data-testid="contact-phone">
            <span class="font-medium">電話番号:</span>
            {{ contact.phoneNumber || '電話番号なし' }}
          </p>
          <p
            v-if="contact.address"
            class="text-sm text-gray-600"
            data-testid="contact-address"
          >
            <span class="font-medium">住所:</span>
            {{ contact.address }}
          </p>
        </div>
      </div>
      <div class="ml-4">
        <svg
          class="h-5 w-5 text-gray-400"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M9 5l7 7-7 7"
          />
        </svg>
      </div>
    </div>
  </div>
</template>
