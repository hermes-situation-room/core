<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { services } from '../services/api';
import type { PostBo } from '../types/post';
import type { UserProfileBo } from '../types/user.ts';
import { useAuthStore } from '../stores/auth-store';
import { useNotification } from '../composables/use-notification.ts';
import { useContextMenu } from '../composables/use-context-menu';
import type { CommentBo, CreateCommentDto, UpdateCommentDto } from '../types/comment';
import ProfileIconDisplay from './profile-icon-display.vue';

const route = useRoute();
const router = useRouter();
const authStore = useAuthStore();
const notification = useNotification();
const { 
    contextMenuItemId: contextMenuPostId,
    contextMenuPosition,
    showContextMenu,
    showMobileMenu, 
    handleRightClick,
    toggleMobileMenu, 
    closeAllMenus 
} = useContextMenu();

const post = ref<PostBo | null>(null);
const loading = ref(false);
const loadingComments = ref(false);
const creatingChat = ref(false);
const creatorDisplayName = ref<string>('');
const creatorProfile = ref<UserProfileBo | null>(null);
const commentContent = ref()
const comments = ref<CommentBo[] | null>(null);
const editingComments = ref(false)
const editCommentContent = ref()

const currentUserUid = computed(() => authStore.userId.value || '');

const canSendMessage = computed(() => {
    return post.value && currentUserUid.value && post.value.creatorUid !== currentUserUid.value;
});

const isPostOwner = computed(() => {
    return post.value && currentUserUid.value && post.value.creatorUid === currentUserUid.value;
});

const privacyText = computed(() => {
    if (post.value?.privacyLevel === 2) return 'This post is visible for journalists'
    if (post.value?.privacyLevel === 1) return 'This post is visible for logged in users'
    else return 'This post is visible for everyone'
});

const editPost = () => {
    closeAllMenus();
    if (post.value) {
        router.push(`/post/${post.value.uid}/edit`);
    }
};

const deletePost = async () => {
    closeAllMenus();
    
    if (!post.value) {
        return;
    }
    
    if (!confirm('Are you sure you want to delete this post?')) {
        return;
    }
    
    try {
        const result = await services.posts.deletePost(post.value.uid);
        if (result.isSuccess) {
            notification.deleted('Post deleted successfully');
            router.push('/');
        } else {
            notification.error(result.responseMessage || 'Failed to delete post');
        }
    } catch (error) {
        notification.error('Error deleting post');
    }
};

const loadPost = async () => {
    const postId = route.params.id as string;
    if (!postId) {
        notification.error('Post ID not found');
        return;
    }

    loading.value = true;
    try {
        const result = await services.posts.getPostById(postId);
        if (result.isSuccess && result.data) {
            post.value = result.data;
            
            if (post.value.creatorUid) {
                const profileResult = await services.users.getUserProfile(post.value.creatorUid, currentUserUid.value || post.value.creatorUid);
                if (profileResult.isSuccess && profileResult.data) {
                    creatorProfile.value = profileResult.data;
                    const displayNameResult = await services.users.getDisplayName(profileResult.data.uid);
                    if (displayNameResult.isSuccess && displayNameResult.data) {
                        creatorDisplayName.value = displayNameResult.data;
                    }
                }
            }
        } else {
            notification.error(result.responseMessage || 'Failed to load post');
            if (result.responseCode == 403)
                router.push('/')
        }
    } catch (err) {
        notification.error('Error loading post');
    } finally {
        loading.value = false;
    }
};

const loadComments = async () => {
    const postId = route.params.id as string;
    if (!postId) {
        notification.warning('Post ID not found');
        return;
    }

    loadingComments.value = true;
    try {
        const result = await services.comments.getCommentsByPost(postId);
        if (result.isSuccess && result.data) {

            for (const comment of result.data) {

                const displayName = await services.users.getDisplayName(comment.creatorUid);
                if (displayName.isSuccess && displayName.data) {
                    comment.displayName = displayName.data
                }
                comment.inEdit = false
            }
            comments.value = result.data;
        } else {
            notification.error(result.responseMessage || 'Failed to load comments');
        }
    } catch (err) {
        notification.error('Error loading comments');
    } finally {
        loadingComments.value = false;
    }
};

const goBack = () => {
    router.back();
};

const sendDirectMessage = async () => {
    if (!post.value || !currentUserUid.value) {
        return;
    }

    if (post.value.creatorUid === currentUserUid.value) {
        notification.warning('You cannot send a message to yourself');
        return;
    }

    creatingChat.value = true;

    try {
        const chatResult = await services.chats.getOrCreateChatByUserPair(
            currentUserUid.value,
            post.value.creatorUid
        );

        if (chatResult.isSuccess && chatResult.data) {
            router.push(`/chat/${chatResult.data.uid}`);
        } else {
            notification.error(chatResult.responseMessage || 'Failed to open chat');
        }
    } catch (err) {
        notification.error('An error occurred while opening the chat');
    } finally {
        creatingChat.value = false;
    }
};

const postComment = async () => {
    try {
        if (!authStore.userId.value) {
            notification.warning('You must be logged in to create a comment');
            return;
        }

        const commentData: CreateCommentDto = {
            postUid: route.params.id as string,
            content: commentContent.value,
            creatorUid: authStore.userId.value
        }

        await services.comments.createComment(commentData)
        commentContent.value = "";

        await loadComments()
    } catch (err) {
        notification.error(err instanceof Error ? err.message : 'An error occurred');
    }
}

const editCommentToggle = (comment:CommentBo) => {
    if (comment.inEdit) {
        editCommentContent.value = '';
        comment.inEdit = false;
        editingComments.value = !comments.value?.some(c => c.inEdit);
    } else {
        editCommentContent.value = comment.content;
        comment.inEdit = true;
        editingComments.value = true;
    }
}

const updateComment = async (comment:CommentBo) => {
    try {
        if (comment.creatorUid !== currentUserUid.value) {
            notification.warning('You cannot edit someone else\'s comment');
            return;
        }

        if (!editCommentContent.value.trim()) {
            notification.warning('Comment content cannot be empty');
            return;
        }

        const commentData: UpdateCommentDto = {
            content: editCommentContent.value.trim()
        }

        const result = await services.comments.updateComment(comment.uid, commentData);
        if (result.isSuccess) {
            notification.updated("Comment updated successfully!")
            await loadComments()
            comment.inEdit = false;
            editingComments.value = !comments.value?.some(c => c.inEdit);
        } else {
            notification.error(result.responseMessage || 'Failed to update comment');
        }
    } catch (err) {
        notification.error('An error occurred while updating the comment');
    }
}

const deleteComment = async (comment:CommentBo) => {
    if (!confirm('Are you sure you want to delete this comment?')) {
        return;
    }

    if (comment.creatorUid !== currentUserUid.value) {
        notification.error('You cannot delete someone else\'s comment');
        return;
    }

    try {
        await services.comments.deleteComment(comment.uid);
        notification.deleted("Comment deleted successfully!")
    } catch (err) {
        notification.error('An error occurred while deleting the comment');
    } finally {
        await loadComments();
    }
    
}

const formatDate = (dateString: string) => {
    const date = new Date(dateString + 'Z') || new Date().toISOString();
    return date.toLocaleDateString(navigator.language || 'en-US', { 
        day: 'numeric', 
        month: 'long', 
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
};

onMounted(() => {
    loadPost();
    loadComments()
});
</script>

<template>
    <div class="container py-4">
        <div class="row justify-content-center">
            <div class="col-12 col-lg-10 col-xl-8">
                <button @click="goBack" class="btn btn-outline-secondary mb-4">
                    <i class="fas fa-arrow-left me-2"></i>
                    Back to Posts
                </button>

                <div v-if="loading" class="d-flex justify-content-center align-items-center py-5">
                    <div class="text-center">
                        <div class="spinner-border text-primary mb-3" role="status" style="width: 3rem; height: 3rem;">
                            <span class="visually-hidden">Loading...</span>
                        </div>
                        <div class="text-muted">Loading post...</div>
                    </div>
                </div>

                <div v-else-if="post" class="card">
                    <div class="card-header bg-dark text-white">
                        <div class="d-flex justify-content-between align-items-start">
                            <div 
                                class="flex-grow-1"
                                v-if="isPostOwner"
                                @contextmenu="handleRightClick($event, post.uid)"
                            >
                                <h1 class="h3 mb-2">{{ post.title }}</h1>
                                <small class="text-light">{{ formatDate(post.timestamp) }}</small>
                            </div>
                            <div 
                                v-else
                                class="flex-grow-1"
                            >
                                <h1 class="h3 mb-2">{{ post.title }}</h1>
                                <small class="text-light">{{ formatDate(post.timestamp) }}</small>
                            </div>
                            <div v-if="isPostOwner" class="position-relative">
                                <button 
                                    class="btn btn-link text-white p-0 ms-2" 
                                    @click="toggleMobileMenu($event, post.uid)"
                                    style="line-height: 1;"
                                >
                                    <i class="fas fa-ellipsis-v"></i>
                                </button>
                                <div 
                                    v-if="showMobileMenu === post.uid"
                                    class="dropdown-menu show position-absolute"
                                    style="right: 0; top: 100%;"
                                >
                                    <button 
                                        class="dropdown-item"
                                        @click="editPost()"
                                    >
                                        <i class="fas fa-edit me-2"></i>Edit
                                    </button>
                                    <div class="dropdown-divider"></div>
                                    <button 
                                        class="dropdown-item text-danger"
                                        @click="deletePost()"
                                    >
                                        <i class="fas fa-trash me-2"></i>Delete
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="card-body">
                        <div v-if="post.description" class="mb-4">
                            <h6 class="text-muted text-uppercase mb-3">Description</h6>
                            <p class="lead">{{ post.description }}</p>
                        </div>

                      
                        <div>
                            <h6 class="text-muted text-uppercase mb-3">Content</h6>
                            <div class="text-dark">
                                <p v-for="paragraph in post.content.split('\n')" :key="paragraph" class="mb-3">
                                    {{ paragraph }}
                                </p>
                            </div>
                        </div>

                        <div class="mb-4">
                            <h6 class="text-muted text-uppercase mb-3">Tags</h6>
                            <div class="d-flex flex-wrap gap-1">
                                <span v-for="tag in post.tags" :key="tag" class="badge bg-info text-white">
                                    {{ tag }}
                                </span>
                            </div>
                        </div>
                    </div>

                    <div class="card-footer bg-light">
                        <div class="d-flex justify-content-between align-items-center">
                            <div class="text-muted small d-flex align-items-center gap-2">
                            <span v-if="currentUserUid && post.creatorUid === currentUserUid">Created by: You</span>
                            <span v-else>
                                Created by: 
                                <a 
                                    href="#" 
                                    class="text-primary text-decoration-none"
                                    @click.prevent="router.push({ path: '/profile', query: { id: post.creatorUid } })"
                                >
                                    <ProfileIconDisplay
                                        :icon="creatorProfile?.profileIcon"
                                        :color="creatorProfile?.profileIconColor"
                                        size="sm"
                                    />
                                    {{ creatorDisplayName || post.creatorUid }}
                                </a>
                            </span>
                            </div>
                            <button 
                                v-if="canSendMessage"
                                class="btn btn-primary btn-sm"
                                :disabled="creatingChat"
                                @click="sendDirectMessage"
                            >
                                <span v-if="creatingChat" class="spinner-border spinner-border-sm me-1"></span>
                                <i v-else class="fas fa-comment me-1"></i>
                                Direct Message
                            </button>
                            <RouterLink 
                                v-if="!currentUserUid"
                                to="/login"
                                class="btn btn-outline-primary btn-sm"
                            >
                                <i class="fas fa-sign-in-alt me-1"></i>
                                Login to Message
                            </RouterLink>
                            
                            <span class="text-muted small d-flex align-items-center" v-if="currentUserUid && post.creatorUid === currentUserUid">
                                {{ privacyText }}
                            </span>
                        </div>
                    </div>

                </div>
                
                <div 
                    v-if="showContextMenu && contextMenuPostId && isPostOwner"
                    class="dropdown-menu show position-fixed"
                    :style="{ 
                        left: contextMenuPosition.x + 'px', 
                        top: contextMenuPosition.y + 'px' 
                    }"
                >
                    <button 
                        class="dropdown-item"
                        @click="editPost()"
                    >
                        <i class="fas fa-edit me-2"></i>Edit Post
                    </button>
                    <div class="dropdown-divider"></div>
                    <button 
                        class="dropdown-item text-danger"
                        @click="deletePost()"
                    >
                        <i class="fas fa-trash me-2"></i>Delete Post
                    </button>
                </div>

                <div class="comment-input mt-3 mb-2 d-flex gap-2">
                    <input v-model="commentContent" maxlength="255" minlength="1" type="text" class="rounded w-100 form-control" placeholder="Comment...">
                    <button v-if="currentUserUid" class="w-25 rounded btn btn-primary" @click="postComment">Comment</button>
                    <RouterLink 
                        v-else
                        to="/login"
                        class="btn btn-outline-primary btn-sm"
                    >
                        <i class="fas fa-sign-in-alt me-1"></i>
                        Login to Comment
                    </RouterLink>
                </div>
                <div class="comment-list d-flex flex-column">
                    <div v-for="comment in comments" :key="comment.uid" class="comment border mb-2 p-3 rounded" style="min-height: 50px;">
                        <div v-if="!comment.inEdit" class="d-flex align-items-start gap-2 h-100">
                            <div class="d-flex align-items-center" style="min-height: 40px;">
                                <a
                                    href="#"
                                    class="text-decoration-none"
                                    @click.prevent="router.push({ path: '/profile', query: { id: comment.creatorUid } })"
                                    title="View profile"
                                >
                                    <ProfileIconDisplay
                                        :icon="comment?.profileIcon"
                                        :color="comment?.profileIconColor"
                                        size="lg"
                                        class="bg-white"
                                    />
                                </a>
                            </div>
                            
                            <div class="flex-grow-1 d-flex flex-column justify-content-between h-100">
                                <div class="d-flex justify-content-between align-items-center mb-1">
                                    <a
                                        href="#"
                                        class="text-primary text-decoration-none fw-bold"
                                        @click.prevent="router.push({ path: '/profile', query: { id: comment.creatorUid } })"
                                    >
                                        {{ comment.displayName }}
                                    </a>
                                    <div class="d-flex align-items-center gap-2">
                                        <small class="text-muted">{{ formatDate(comment.timestamp) }}</small>
                                        <div v-if="comment.creatorUid == currentUserUid && !comment.inEdit" class="d-flex gap-2">
                                            <i class="fas fa-edit text-muted" style="cursor: pointer;" @click="editCommentToggle(comment)" title="Edit comment"></i>
                                            <i class="fas fa-trash text-muted" style="cursor: pointer;" @click="deleteComment(comment)" title="Delete comment"></i>
                                        </div>
                                    </div>
                                </div>
                                
                                <div class="text-break text-dark flex-grow-1">
                                    {{ comment.content }}
                                </div>
                            </div>
                        </div>
                        
                        <div v-else class="d-flex justify-content-between align-items-center gap-2">
                            <input v-model="editCommentContent" type="text" maxlength="255" class="rounded w-100 form-control">
                            
                            <button class="btn btn-primary btn-sm d-flex align-items-center" @click="updateComment(comment)">
                                <i class="fas fa-check me-1"></i>
                                Confirm
                            </button>
                            <button class="btn btn-secondary btn-sm d-flex align-items-center" @click="editCommentToggle(comment)">
                                <i class="fas fa-times me-1"></i>
                                Cancel
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
