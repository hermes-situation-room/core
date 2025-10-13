<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { services } from '../services/api';
import type { PostBo } from '../types/post';
import type { CreateChatRequest } from '../types/chat';

const route = useRoute();
const router = useRouter();

const post = ref<PostBo | null>(null);
const loading = ref(false);
const error = ref<string | null>(null);
const creatingChat = ref(false);

const currentUserUid = computed(() => localStorage.getItem('userUid') || '');

const canSendMessage = computed(() => {
    return post.value && currentUserUid.value && post.value.creatorUid !== currentUserUid.value;
});

const loadPost = async () => {
    const postId = route.params.id as string;
    if (!postId) {
        error.value = 'Post ID not found';
        return;
    }

    loading.value = true;
    try {
        const result = await services.posts.getPostById(postId);
        if (result.isSuccess && result.data) {
            post.value = result.data;
        } else {
            error.value = result.responseMessage || 'Failed to load post';
        }
    } catch (err) {
        error.value = 'Error loading post';
        console.error('Error loading post:', err);
    } finally {
        loading.value = false;
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
        alert('You cannot send a message to yourself');
        return;
    }

    creatingChat.value = true;

    try {
        const existingChatResult = await services.chats.getChatByUserPair(
            currentUserUid.value,
            post.value.creatorUid
        );

        if (existingChatResult.isSuccess && existingChatResult.data) {
            router.push(`/chat/${existingChatResult.data.uid}`);
            return;
        }

        const chatData: CreateChatRequest = {
            user1Uid: currentUserUid.value,
            user2Uid: post.value.creatorUid
        };

        const createResult = await services.chats.createChat(chatData);
        
        if (createResult.isSuccess && createResult.data) {
            router.push(`/chat/${createResult.data}`);
        } else {
            console.error('Failed to create chat:', createResult.responseMessage);
            alert('Failed to create chat. Please try again.');
        }
    } catch (err) {
        console.error('Error creating chat:', err);
        alert('An error occurred while creating the chat');
    } finally {
        creatingChat.value = false;
    }
};

const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-GB', { 
        day: 'numeric', 
        month: 'long', 
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
};

onMounted(() => {
    loadPost();
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

                <div v-else-if="error" class="card">
                    <div class="card-body text-center p-5">
                        <i class="fas fa-exclamation-triangle mb-4 text-danger" style="font-size: 3rem;"></i>
                        <h3 class="card-title">Error Loading Post</h3>
                        <p class="card-text text-muted">{{ error }}</p>
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
                            Created by: {{ post.creatorUid }}
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
                            <span v-else-if="post.creatorUid === currentUserUid" class="badge bg-secondary">
                                Your Post
                            </span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
