<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  modelValue: string
  label?: string
  type?: 'text' | 'email' | 'password' | 'tel' | 'number'
  placeholder?: string
  error?: string
  disabled?: boolean
  required?: boolean
  maxlength?: number
  autocomplete?: string
  id?: string
}

const props = withDefaults(defineProps<Props>(), {
  type: 'text',
  disabled: false,
  required: false,
})

const emit = defineEmits<{
  'update:modelValue': [value: string]
  blur: [event: FocusEvent]
  focus: [event: FocusEvent]
}>()

const inputId = computed(() => props.id || `input-${Math.random().toString(36).substr(2, 9)}`)

const handleInput = (event: Event) => {
  const target = event.target as HTMLInputElement
  emit('update:modelValue', target.value)
}

const handleBlur = (event: FocusEvent) => {
  emit('blur', event)
}

const handleFocus = (event: FocusEvent) => {
  emit('focus', event)
}

const inputClasses = computed(() => [
  'block w-full rounded-md border-0 px-3 py-2',
  'text-gray-900 shadow-sm ring-1 ring-inset',
  'placeholder:text-gray-400',
  'focus:ring-2 focus:ring-inset',
  'sm:text-sm sm:leading-6',
  'transition-colors duration-200',
  'disabled:cursor-not-allowed disabled:bg-gray-50 disabled:text-gray-500',
  props.error
    ? 'ring-red-300 focus:ring-red-500'
    : 'ring-gray-300 focus:ring-indigo-600',
].join(' '))
</script>

<template>
  <div class="w-full">
    <!-- Label -->
    <label
      v-if="label"
      :for="inputId"
      class="block text-sm font-medium leading-6 text-gray-900 mb-1"
    >
      {{ label }}
      <span v-if="required" class="text-red-500" aria-label="必須">*</span>
    </label>

    <!-- Input field -->
    <div class="relative">
      <input
        :id="inputId"
        :type="type"
        :value="modelValue"
        :placeholder="placeholder"
        :disabled="disabled"
        :required="required"
        :maxlength="maxlength"
        :autocomplete="autocomplete"
        :class="inputClasses"
        :aria-invalid="!!error"
        :aria-describedby="error ? `${inputId}-error` : undefined"
        @input="handleInput"
        @blur="handleBlur"
        @focus="handleFocus"
      />

      <!-- Error icon -->
      <div
        v-if="error"
        class="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-3"
      >
        <svg
          class="h-5 w-5 text-red-500"
          viewBox="0 0 20 20"
          fill="currentColor"
          aria-hidden="true"
        >
          <path
            fill-rule="evenodd"
            d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-8-5a.75.75 0 01.75.75v4.5a.75.75 0 01-1.5 0v-4.5A.75.75 0 0110 5zm0 10a1 1 0 100-2 1 1 0 000 2z"
            clip-rule="evenodd"
          />
        </svg>
      </div>
    </div>

    <!-- Error message -->
    <p
      v-if="error"
      :id="`${inputId}-error`"
      class="mt-1 text-sm text-red-600"
      role="alert"
    >
      {{ error }}
    </p>

    <!-- Character count (if maxlength is set) -->
    <p
      v-if="maxlength && !error"
      class="mt-1 text-xs text-gray-500 text-right"
      aria-live="polite"
    >
      {{ modelValue.length }} / {{ maxlength }}
    </p>
  </div>
</template>
