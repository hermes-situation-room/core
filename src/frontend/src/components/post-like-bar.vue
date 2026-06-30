<script setup lang="ts">
interface Props {
    likeCount: number;
    userHasLiked: boolean;
    likingPost: boolean;
}

defineProps<Props>();

const emit = defineEmits<{
    (event: 'toggle'): void;
}>();
</script>

<template>
    <div class="d-flex align-items-center gap-3 pt-3 border-top">
        <button
            @click="emit('toggle')"
            :disabled="likingPost"
            class="btn btn-sm d-flex align-items-center gap-2"
            :class="userHasLiked ? 'btn-primary' : 'btn-outline-primary'"
        >
            <i :class="userHasLiked ? 'fas fa-heart' : 'far fa-heart'"></i>
            <span>{{ userHasLiked ? 'Liked' : 'Like' }}</span>
            <span v-if="likeCount > 0" class="badge bg-light text-dark">{{ likeCount }}</span>
        </button>
        <span class="text-muted small">
            {{ likeCount }} {{ likeCount === 1 ? 'person likes' : 'people like' }} this post
        </span>
    </div>
</template>
