<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { services } from '../services/api';
import type { PostBo } from '../types/post';
import { useAuthStore } from '../stores/auth-store';
import { useNotification } from '../composables/use-notification.ts';
import type { CommentBo, CreateCommentDto, UpdateCommentDto } from '../types/comment';

const route = useRoute();
const router = useRouter();
const authStore = useAuthStore();
const notification = useNotification();

const post = ref<PostBo | null>(null);
const loading = ref(false);
const loadingComments = ref(false);
const creatingChat = ref(false);
const creatorDisplayName = ref<string>('');
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

const editPost = () => {
    if (post.value) {
        router.push(`/post/${post.value.uid}/edit`);
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
            
            if (post.value.creatorUid && post.value.creatorUid !== currentUserUid.value) {
                const displayNameResult = await services.users.getDisplayName(post.value.creatorUid);
                if (displayNameResult.isSuccess && displayNameResult.data) {
                    creatorDisplayName.value = displayNameResult.data;
                }
            }
        } else {
            notification.error(result.responseMessage || 'Failed to load post');
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
    editCommentContent.value = comment.content;
    editingComments.value = !editingComments.value
    comment.inEdit = !comment.inEdit
}

const updateComment = async (comment:CommentBo) => {
    try {
        if (comment.creatorUid !== currentUserUid.value) {
            notification.warning('You cannot edit someone else\'s comment');
            return;
        }

        const commentData: UpdateCommentDto = {
            content: editCommentContent.value
        }

        const result = await services.comments.updateComment(comment.uid, commentData);
        if (result.isSuccess && result.data) {
            notification.updated("Comment updated successfully!")
            await loadComments()
        } else {
            notification.error(result.responseMessage || 'Failed to update comment');
        }
    } catch (err) {
        notification.error('An error occurred while updating the comment');
    } finally {
        editingComments.value = false
        comment.inEdit = false
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
                        <h1 class="h3 mb-2">{{ post.title }}</h1>
                        <small class="text-light">{{ formatDate(post.timestamp) }}</small>
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
                            <div class="text-muted small d-flex align-items-center">
                            <i class="fas fa-user me-2"></i>
                            <span v-if="currentUserUid && post.creatorUid === currentUserUid">Created by: You</span>
                            <span v-else>
                                Created by: 
                                <a 
                                    href="#" 
                                    class="text-primary text-decoration-none"
                                    @click.prevent="router.push({ path: '/profile', query: { id: post.creatorUid } })"
                                >
                                    {{ creatorDisplayName || post.creatorUid }}
                                </a>
                            </span>
                            </div>
                                                        <div class="d-flex gap-2">
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
                                <button
                                    v-if="isPostOwner"
                                    class="btn btn-warning btn-sm"
                                    @click="editPost"
                                >
                                    <i class="fas fa-edit me-1"></i>
                                    Edit Post
                                </button>
                            </div>
                            <RouterLink 
                                v-if="!currentUserUid"
                                to="/login"
                                class="btn btn-outline-primary btn-sm"
                            >
                                <i class="fas fa-sign-in-alt me-1"></i>
                                Login to Message
                            </RouterLink>
                        </div>
                    </div>

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
                    <div v-for="comment in comments" :key="comment.uid" class="comment border mb-1 p-2 pe-3 ps-3 rounded d-flex flex-column flex-wrap">
                        <small class="d-flex justify-content-between">
                            <strong>
                                <a href="#" class="text-primary text-decoration-none" @click.prevent="router.push({ path: '/profile', query: { id: comment.creatorUid } })">
                                    {{ comment.displayName }}
                                </a>
                            </strong>
                            {{ formatDate(comment.timestamp) }}
                        </small>
                        <div>
                            <div v-if="!comment.inEdit" class="d-flex justify-content-between flex-nowrap gap-2">
                                <div class="text-break width-100">
                                    {{ comment.content }}
                                </div>
                                <div class="d-flex gap-3 mt-1">
                                    <i v-if="comment.creatorUid == currentUserUid && !editingComments" class="fas fa-edit" @click="editCommentToggle(comment)"></i>
                                    <i v-if="comment.creatorUid == currentUserUid && !editingComments" class="fas fa-trash" @click="deleteComment(comment)"></i>
                                </div>
                            </div>
                            <div v-else class="d-flex justify-content-between align-items-center gap-2">
                                <input v-model="editCommentContent" type="text" maxlength="255" class="rounded w-100 form-control">
                                
                                <button class="w-20 rounded btn btn-primary d-flex align-items-center" @click="updateComment(comment)">
                                    <i class="fas fa-check fa-lg m-2"></i>
                                    Confirm
                                </button>
                                <button class="w-20 rounded btn btn-primary d-flex align-items-center" @click="editCommentToggle(comment)">
                                    <i class="fas fa-close fa-lg m-2"></i>
                                    Cancel
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
