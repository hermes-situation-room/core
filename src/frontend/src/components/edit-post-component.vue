<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { services } from '../services/api';
import type { PostBo, UpdatePostDto } from '../types/post';
import { useAuthStore } from '../stores/auth-store';
import { useNotification } from '../composables/use-notification.ts';

const route = useRoute();
const router = useRouter();
const authStore = useAuthStore();
const notification = useNotification();

const postType = computed(() => {
    return authStore.userType.value;
});

const formData = ref<{
    title: string;
    description: string;
    content: string;
}>({
    title: '',
    description: '',
    content: ''
});

const originalPost = ref<PostBo | null>(null);
const availableTags = ref<string[]>([]);
const selectedTags = ref<string[]>([]);
const availablePostPrivacyLevels = ref<string[]>([]);
const selectedPrivacyLevelName = ref<string>('');
const selectedPrivacyLevelIndex = ref<number>(0);
const selectedPrivacyLevel = ref<number>();
const loading = ref(false);
const loadingTags = ref(false);
const loadingPost = ref(false);
const loadingPrivacyLevels = ref(false);
const error = ref('');
const showPrivacyDropdown = ref(false);

const loadPrivacyLevels = async () => {
    loadingPrivacyLevels.value = true;
    try {
        const result = await services.posts.getPostPrivacyLevels();
        if (result.isSuccess && result.data) {
            availablePostPrivacyLevels.value = result.data;
            selectedPrivacyLevelName.value = availablePostPrivacyLevels.value[Number(selectedPrivacyLevel.value)] || 'EVERYONE';
        }
    } catch (err) {
        error.value = 'Error loading privacy levels';
    } finally {
        loadingPrivacyLevels.value = false;
    }
};

watch(selectedPrivacyLevelName, (newVal) => {
    selectedPrivacyLevelName.value = newVal;
    selectedPrivacyLevelIndex.value = availablePostPrivacyLevels.value.findIndex(a => a === newVal);
});

const formatName = (name: string) => {
  return name
    .toLowerCase()
    .replace(/_/g, ' ')
    .replace(/\b\w/g, char => char.toUpperCase());
};

const loadPost = async () => {
    const postId = route.params.id as string;
    if (!postId) {
        notification.error('Post ID not found');
        return;
    }

    loadingPost.value = true;
    try {
        const result = await services.posts.getPostById(postId);
        if (result.isSuccess && result.data) {
            originalPost.value = result.data;
            
            if (originalPost.value.creatorUid !== authStore.userId.value) {
                notification.error('You are not authorized to edit this post');
                setTimeout(() => {
                    router.push('/');
                }, 2000);
                return;
            }

            formData.value = {
                title: result.data.title,
                description: result.data.description,
                content: result.data.content
            };
            selectedTags.value = [...result.data.tags];

            console.log(originalPost)
            console.log(originalPost.value?.privacyLevel)
            selectedPrivacyLevel.value = Number(originalPost.value?.privacyLevel);
            loadPrivacyLevels();
        } else {
            notification.error(result.responseMessage || 'Failed to load post');
        }
    } catch (err) {
        notification.error('Error loading post');
    } finally {
        loadingPost.value = false;
    }
};

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

const goBack = () => {
    router.back();
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

    if (!originalPost.value) {
        notification.error('Post data not loaded');
        return;
    }

    loading.value = true;

    try {
        const updateData: UpdatePostDto = {
            uid: originalPost.value.uid,
            timestamp: originalPost.value.timestamp,
            title: formData.value.title,
            description: formData.value.description,
            content: formData.value.content,
            creatorUid: originalPost.value.creatorUid,
            tags: selectedTags.value.map(tag => tag.toUpperCase()),
            privacyLevel: selectedPrivacyLevelIndex.value
        };

        const result = await services.posts.updatePost(originalPost.value.uid, updateData);

        if (result.isSuccess) {
            notification.updated('Post updated successfully!');
            router.replace(`/post/${originalPost.value.uid}`);
        } else {
            notification.error(result.responseMessage || 'Failed to update post');
        }
    } catch (err) {
        notification.error(err instanceof Error ? err.message : 'An error occurred');
    } finally {
        loading.value = false;
    }
};

const togglePrivacyDropdown = () => {
    if (!loadingPrivacyLevels.value) {
        showPrivacyDropdown.value = !showPrivacyDropdown.value;
    }
};

const selectPrivacyLevel = (level: string) => {
    selectedPrivacyLevelName.value = level;
    showPrivacyDropdown.value = false;
};

onMounted(() => {
    loadPost();
    loadTags();
});
</script>

<template>
    <div class="container py-4">
        <div class="row justify-content-center">
            <div class="col-12 col-lg-10 col-xl-8">
                <button @click="goBack" class="btn btn-outline-secondary mb-4" :disabled="loading">
                    <i class="fas fa-arrow-left me-2"></i>
                    Cancel
                </button>

                <div v-if="loadingPost" class="d-flex justify-content-center align-items-center py-5">
                    <div class="text-center">
                        <div class="spinner-border text-primary mb-3" role="status" style="width: 3rem; height: 3rem;">
                            <span class="visually-hidden">Loading...</span>
                        </div>
                        <div class="text-muted">Loading post...</div>
                    </div>
                </div>

                <div v-else-if="originalPost" class="card">
                    <div class="card-header bg-dark text-white">
                        <h1 class="h3 mb-0">Edit {{ postType === 'journalist' ? 'Journalist' : 'Activist' }} Post</h1>
                    </div>

                    <div class="card-body">
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
                                <label class="form-label">Post Privacy Level</label>
                                <div class="privacy-dropdown dropdown">
                                    <button
                                        class="btn btn-outline-secondary dropdown-toggle"
                                        type="button"
                                        @click="togglePrivacyDropdown"
                                        :disabled="loadingPrivacyLevels"
                                    >
                                    <span v-if="loadingPrivacyLevels">
                                        <span class="spinner-border spinner-border-sm me-2" role="status"></span>Loading...
                                    </span>
                                    <span v-else>{{ formatName(selectedPrivacyLevelName) }}</span>
                                    </button>
                                    <ul
                                        :class="['dropdown-menu', { 'show': showPrivacyDropdown }]"
                                    >
                                    <li v-for="level in availablePostPrivacyLevels" :key="level">
                                        <a
                                            class="dropdown-item"
                                            href="#"
                                            @click.prevent="selectPrivacyLevel(level)"
                                            :class="{ 'active': selectedPrivacyLevelName === level }"
                                        >
                                            {{ formatName(level) }}
                                        </a>
                                    </li>
                                    </ul>
                                </div>
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

                    <div class="card-footer bg-light">
                        <div class="d-flex justify-content-end gap-2">
                            <button
                                type="button"
                                class="btn btn-secondary"
                                @click="goBack"
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
                                {{ loading ? 'Updating...' : 'Update Post' }}
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
