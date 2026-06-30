<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue';
import { services } from '../services/api';
import type { CreatePostDto } from '../types/post';
import { useAuthStore } from '../stores/auth-store';
import { useNotification } from '../composables/use-notification.ts';
import { useTextFormatting } from '../composables/use-text-formatting.ts';
import FormattingToolbar from './formatting-toolbar.vue';
import AttachmentPicker from './attachments/attachment-picker.vue';
import type { AttachmentDto } from '../services/api/attachments-api';

const authStore = useAuthStore();
const notification = useNotification();
const { getFormattingHelp } = useTextFormatting();

interface Props {
    show: boolean;
}

const props = defineProps<Props>();

const postType = computed(() => {
    return authStore.userType.value;
});

const isJournalist = computed(() => {
    return authStore.isJournalist.value;
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
const attachments = ref<AttachmentDto[]>([]);
const availablePostPrivacyLevels = ref<string[]>([]);
const selectedPrivacyLevelName = ref<string>('');
const selectedPrivacyLevelIndex = ref<number>(0);
const loading = ref(false);
const loadingTags = ref(false);
const loadingPrivacyLevels = ref(false);
const error = ref('');
const showPrivacyDropdown = ref(false);
const showFormattingHelp = ref(false);

const loadPostPrivacyLevels = async () => {
    loadingPrivacyLevels.value = true;
    try {
        const result = await services.posts.getPostPrivacyLevels();
        if (result.isSuccess && result.data) {
            availablePostPrivacyLevels.value = result.data;
            selectedPrivacyLevelName.value = availablePostPrivacyLevels.value[0] || 'EVERYONE';
            selectedPrivacyLevelIndex.value = 0;
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

const newTagName = ref('');
const requestingTag = ref(false);

const requestNewTag = async () => {
    const name = newTagName.value.trim();
    if (!name) {
        notification.warning('Enter a tag name to request');
        return;
    }

    requestingTag.value = true;
    try {
        const result = await services.tagRequests.createTagRequest({ name });
        if (result.isSuccess) {
            notification.created('Tag request submitted for admin approval');
            newTagName.value = '';
        } else {
            notification.error(result.responseMessage || 'Failed to request tag');
        }
    } catch (err) {
        notification.error(err instanceof Error ? err.message : 'Error requesting tag');
    } finally {
        requestingTag.value = false;
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
        attachments.value = [];
        error.value = '';
        selectedPrivacyLevelName.value = availablePostPrivacyLevels.value[0] || 'EVERYONE';
        selectedPrivacyLevelIndex.value = 0;
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
            tags: selectedTags.value.map(tag => tag.toUpperCase()),
            privacyLevel: selectedPrivacyLevelIndex.value,
            attachmentIds: attachments.value.map(attachment => attachment.uid),
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
    loadTags();
    
    if (!isJournalist.value) {
        loadPostPrivacyLevels();
    }

    document.addEventListener('click', (e) => {
        const target = e.target as HTMLElement;
        if (!target.closest('.privacy-dropdown')) {
        showPrivacyDropdown.value = false;
        }
    });
});
</script>

<template>
    <div
        v-if="show"
        class="modal fade show d-block"
        tabindex="-1"
        style="background-color: var(--modal-backdrop);"
        @click.self="handleClose"
    >
        <div class="modal-dialog modal-lg modal-dialog-centered modal-dialog-scrollable modal-fullscreen-sm-down">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Create New {{ postType === 'journalist' ? 'Journalist' : 'Activist' }} Post</h5>
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
                                spellcheck="true"
                                lang="en"
                                :disabled="loading"
                                required
                            />
                        </div>

                        <div class="mb-3">
                            <label for="postDescription" class="form-label">Description <span class="text-danger">*</span></label>
                            <textarea
                                class="form-control"
                                id="postDescription"
                                v-model="formData.description"
                                placeholder="Brief description of the post"
                                rows="2"
                                spellcheck="true"
                                lang="en"
                                :disabled="loading"
                                required
                            ></textarea>
                        </div>

                        <div class="mb-3">
                            <label for="postContent" class="form-label">Content <span class="text-danger">*</span></label>
                            <FormattingToolbar
                                target-id="postContent"
                                v-model="formData.content"
                                :disabled="loading"
                            />
                            <textarea
                                class="form-control"
                                id="postContent"
                                v-model="formData.content"
                                placeholder="Write your post content here..."
                                rows="8"
                                spellcheck="true"
                                lang="en"
                                :disabled="loading"
                                required
                            ></textarea>
                            <div class="form-text d-flex justify-content-between align-items-center">
                                <div>
                                    <i class="fas fa-info-circle me-1"></i>
                                    <strong>Formatting:</strong>
                                    <span v-for="(help, index) in getFormattingHelp()" :key="index" class="ms-2">
                                        <code class="text-muted">{{ help }}</code>
                                    </span>
                                </div>
                                <button
                                    type="button"
                                    class="btn btn-sm btn-outline-secondary"
                                    @click="showFormattingHelp = true"
                                >
                                    <i class="fas fa-question-circle me-1"></i>
                                    Help
                                </button>
                            </div>
                        </div>

                        <div class="mb-3">
                            <label class="form-label">Attachments</label>
                            <AttachmentPicker v-model="attachments" :disabled="loading" />
                            <div class="form-text">
                                <i class="fas fa-user-secret me-1"></i>
                                Images can be redacted (faces/text blurred or blacked out) before they leave your device.
                            </div>
                        </div>

                        <div v-if="!isJournalist" class="mb-3">
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

                            <div class="mt-3">
                                <label for="requestTagInput" class="form-label small text-muted">
                                    Can't find a tag? Request a new one
                                </label>
                                <div class="input-group input-group-sm">
                                    <input
                                        type="text"
                                        class="form-control"
                                        id="requestTagInput"
                                        v-model="newTagName"
                                        placeholder="e.g. Climate Crisis"
                                        :disabled="requestingTag"
                                        @keyup.enter.prevent="requestNewTag"
                                    />
                                    <button
                                        type="button"
                                        class="btn btn-outline-primary"
                                        @click="requestNewTag"
                                        :disabled="requestingTag || !newTagName.trim()"
                                    >
                                        <span v-if="requestingTag" class="spinner-border spinner-border-sm me-1" role="status"></span>
                                        Request tag
                                    </button>
                                </div>
                                <div class="form-text">
                                    New tags require admin approval before they can be used.
                                </div>
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
                    <span v-if="loading" class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                        {{ loading ? 'Creating...' : 'Create Post' }}
                    </button>
                </div>
            </div>
        </div>
    </div>
    
    <!-- Formatting Help Modal -->
    <div 
        v-if="showFormattingHelp"
        class="modal fade show d-block" 
        tabindex="-1" 
        style="background-color: var(--modal-backdrop); z-index: 1060;"
        @click.self="showFormattingHelp = false"
    >
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">
                        <i class="fas fa-question-circle me-2"></i>
                        Text Formatting Guide
                    </h5>
                    <button 
                        type="button" 
                        class="btn-close" 
                        @click="showFormattingHelp = false"
                    ></button>
                </div>
                <div class="modal-body">
                    <div class="mb-3">
                        <h6 class="fw-bold">Bold Text</h6>
                        <p class="mb-2">
                            <code>**your text**</code> 
                            <i class="fas fa-arrow-right mx-2"></i>
                            <strong>your text</strong>
                        </p>
                    </div>
                    
                    <div class="mb-3">
                        <h6 class="fw-bold">Italic Text</h6>
                        <p class="mb-2">
                            <code>*your text*</code>
                            <i class="fas fa-arrow-right mx-2"></i>
                            <em>your text</em>
                        </p>
                    </div>
                    
                    <div class="mb-3">
                        <h6 class="fw-bold">Bullet Lists</h6>
                        <p class="mb-1">Start lines with <code>-</code> or <code>*</code>:</p>
                        <div class="row">
                            <div class="col-6">
                                <small class="text-muted">Input:</small>
                                <pre class="bg-light p-2 rounded"><code>- First item
- Second item
- Third item</code></pre>
                            </div>
                            <div class="col-6">
                                <small class="text-muted">Output:</small>
                                <ul>
                                    <li>First item</li>
                                    <li>Second item</li>
                                    <li>Third item</li>
                                </ul>
                            </div>
                        </div>
                    </div>
                    
                    <div class="mb-3">
                        <h6 class="fw-bold">Colored Text</h6>
                        <p class="mb-1">Wrap text with <code>[color=NAME]</code> and <code>[/color]</code>:</p>
                        <p class="mb-2">
                            <code>[color=red]your text[/color]</code>
                            <i class="fas fa-arrow-right mx-2"></i>
                            <span style="color: red">your text</span>
                        </p>
                        <p class="mb-0 small text-muted">
                            Allowed colors: red, orange, yellow, green, teal, blue, purple, pink, gray,
                            black, white, or a hex value like <code>[color=#247BA0]text[/color]</code>.
                        </p>
                    </div>

                    <div class="alert alert-info mb-0">
                        <i class="fas fa-lightbulb me-2"></i>
                        <small>You can combine formatting: <code>**bold and *italic***</code></small>
                    </div>
                </div>
                <div class="modal-footer">
                    <button 
                        type="button" 
                        class="btn btn-primary" 
                        @click="showFormattingHelp = false"
                    >
                        Got it!
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>
