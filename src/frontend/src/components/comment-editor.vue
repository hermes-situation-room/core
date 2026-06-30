<script setup lang="ts">
interface Props {
    modelValue: string;
    canComment: boolean;
}

defineProps<Props>();

const emit = defineEmits<{
    (event: 'update:modelValue', value: string): void;
    (event: 'submit'): void;
}>();
</script>

<template>
    <div class="comment-input mt-3 mb-2 d-flex gap-2">
        <input
            :value="modelValue"
            @input="emit('update:modelValue', ($event.target as HTMLInputElement).value)"
            maxlength="255"
            minlength="1"
            type="text"
            spellcheck="true"
            lang="en"
            class="rounded w-100 form-control"
            placeholder="Comment..."
        >
        <button v-if="canComment" class="w-25 rounded btn btn-primary" @click="emit('submit')">Comment</button>
        <RouterLink
            v-else
            to="/login"
            class="btn btn-outline-primary btn-sm"
        >
            <i class="fas fa-sign-in-alt me-1"></i>
            Login to Comment
        </RouterLink>
    </div>
</template>
