<script setup lang="ts">
import {computed, onMounted, ref, watch} from 'vue';
import {services} from '../services/api';
import type {CreatePostDto} from '../types/post';
import { useAuthStore } from '../stores/auth-store';
import { useNotification } from '../composables/use-notification.ts';

const authStore = useAuthStore();
const notification = useNotification();

interface Props {
    show: boolean;
}

const props = defineProps<Props>();

const postType = computed(() => {
    return authStore.userType.value;
});

const emit = defineEmits<{
    (e: 'close'): void;
    (e: 'postCreated'): void;
}>();

const formData = ref<{
    title: string;
    description: string;
    content: string;
}>({
    title: '',
    description: '',
    content: ''
});

const availableTags = ref<string[]>([]);
const selectedTags = ref<string[]>([]);
const loading = ref(false);
const loadingTags = ref(false);

const loadTags = async () => {
    loadingTags.value = true;
    try {
        const result = await services.tags.getAllTags();
        if (result.isSuccess && result.data) {
            availableTags.value = result.data;
        } else {
            notification.error(result.responseMessage || 'Failed to load tags');
        }
    } catch (err) {
        notification.error('Error loading tags');
    } finally {
        loadingTags.value = false;
    }
};

const toggleTag = (tag: string) => {
    const index = selectedTags.value.indexOf(tag);
    if (index > -1) {
        selectedTags.value.splice(index, 1);
    } else {
        selectedTags.value.push(tag);
    }
};

const isTagSelected = (tag: string) => {
    return selectedTags.value.includes(tag);
};

watch(() => props.show, (newVal) => {
    if (!newVal) {
        formData.value = {
            title: '',
            description: '',
            content: ''
        };
        selectedTags.value = [];
    }
});

const handleClose = () => {
    emit('close');
};

const handleSubmit = async () => {
    if (!formData.value.title.trim()) {
        notification.warning('Title is required');
        return;
    }
    if (!formData.value.description.trim()) {
        notification.warning('Description is required');
        return;
    }
    if (!formData.value.content.trim()) {
        notification.warning('Content is required');
        return;
    }

    loading.value = true;

    try {
        if (!authStore.userId.value) {
            notification.error('You must be logged in to create a post');
            loading.value = false;
            return;
        }

        const postData: CreatePostDto = {
            title: formData.value.title,
            description: formData.value.description,
            content: formData.value.content,
            creatorUid: authStore.userId.value,
            tags: selectedTags.value.map(tag => tag.toUpperCase())
        };

        const result = await services.posts.createPost(postData);

        if (result.isSuccess) {
            notification.created('Post created successfully!');
            emit('postCreated');
            emit('close');
        } else {
            notification.error(result.responseMessage || 'Failed to create post');
        }
    } catch (err) {
        notification.error(err instanceof Error ? err.message : 'An error occurred');
    } finally {
        loading.value = false;
    }
};

onMounted(() => {
    loadTags();
});
</script>

<template>
    <div
        v-if="show"
        class="modal fade show d-block"
        tabindex="-1"
        style="background-color: rgba(0, 0, 0, 0.5);"
        @click.self="handleClose"
    >
        <div class="modal-dialog modal-lg modal-dialog-centered modal-dialog-scrollable modal-fullscreen-sm-down">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Create New {{ postType === 'journalist' ? 'Journalist' : 'Activist' }}
                        Post</h5>
                    <button
                        type="button"
                        class="btn-close"
                        @click="handleClose"
                        :disabled="loading"
                    ></button>
                </div>
                <div class="modal-body">
                    <form @submit.prevent="handleSubmit">
                        <div class="mb-3">
                            <label for="postTitle" class="form-label">Title <span class="text-danger">*</span></label>
                            <input
                                type="text"
                                class="form-control"
                                id="postTitle"
                                v-model="formData.title"
                                placeholder="Enter post title"
                                :disabled="loading"
                                required
                            />
                        </div>

                        <div class="mb-3">
                            <label for="postDescription" class="form-label">Description <span
                                class="text-danger">*</span></label>
                            <textarea
                                class="form-control"
                                id="postDescription"
                                v-model="formData.description"
                                placeholder="Brief description of the post"
                                rows="2"
                                :disabled="loading"
                                required
                            ></textarea>
                        </div>

                        <div class="mb-3">
                            <label for="postContent" class="form-label">Content <span
                                class="text-danger">*</span></label>
                            <textarea
                                class="form-control"
                                id="postContent"
                                v-model="formData.content"
                                placeholder="Write your post content here..."
                                rows="8"
                                :disabled="loading"
                                required
                            ></textarea>
                        </div>

                        <div class="mb-3">
                            <label class="form-label">Tags</label>
                            <div v-if="loadingTags" class="text-muted">
                                <span class="spinner-border spinner-border-sm me-2" role="status"></span>
                                Loading tags...
                            </div>
                            <div v-else class="d-flex flex-wrap gap-2">
                                <span
                                    v-for="tag in availableTags"
                                    :key="tag"
                                    @click="toggleTag(tag)"
                                    :class="[
                                        'badge',
                                        'py-2',
                                        'px-3',
                                        isTagSelected(tag) ? 'bg-primary' : 'bg-secondary',
                                        loading ? '' : 'cursor-pointer'
                                    ]"
                                    :style="{ cursor: loading ? 'not-allowed' : 'pointer' }"
                                >
                                    <i v-if="isTagSelected(tag)" class="fas fa-check me-1"></i>
                                    {{ tag }}
                                </span>
                            </div>
                            <div class="form-text mt-2">
                                Click on tags to select/deselect them
                                <span v-if="selectedTags.length > 0" class="text-primary fw-bold">
                                    ({{ selectedTags.length }} selected)
                                </span>
                            </div>
                        </div>
                    </form>
                </div>
                <div class="modal-footer">
                    <button
                        type="button"
                        class="btn btn-secondary"
                        @click="handleClose"
                        :disabled="loading"
                    >
                        Cancel
                    </button>
                    <button
                        type="button"
                        class="btn btn-primary"
                        @click="handleSubmit"
                        :disabled="loading"
                    >
                        <span v-if="loading" class="spinner-border spinner-border-sm me-2" role="status"
                              aria-hidden="true"></span>
                        {{ loading ? 'Creating...' : 'Create Post' }}
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>

