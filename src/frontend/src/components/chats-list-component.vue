<script setup lang="ts">
import {onMounted, ref} from 'vue';
import {useRouter} from 'vue-router';
import {services} from '../services/api';
import type {ChatBo} from '../types/chat';

const router = useRouter();

const chats = ref<ChatBo[]>([]);
const loading = ref(false);
const currentUserUid = ref<string>('');

const loadChats = async () => {
    loading.value = true;
    try {
        // Get the current user UID from localStorage
        currentUserUid.value = localStorage.getItem('userUid') || '';
        
        if (!currentUserUid.value) {
            console.error('No user UID found in localStorage');
            return;
        }

        const result = await services.chats.getChatsByUser(currentUserUid.value);
        if (result.isSuccess && result.data) {
            chats.value = result.data;
        } else {
            console.error('Failed to load chats:', result.responseMessage);
        }
    } catch (error) {
        console.error('Error loading chats:', error);
    } finally {
        loading.value = false;
    }
};

const viewChat = (chatId: string) => {
    router.push(`/chat/${chatId}`);
};

const getOtherUserUid = (chat: ChatBo) => {
    return chat.user1Uid === currentUserUid.value ? chat.user2Uid : chat.user1Uid;
};

const deleteChat = async (chatId: string, event: Event) => {
    event.stopPropagation();
    
    if (!confirm('Are you sure you want to delete this chat?')) {
        return;
    }

    try {
        const result = await services.chats.deleteChat(chatId);
        if (result.isSuccess) {
            chats.value = chats.value.filter(chat => chat.uid !== chatId);
        } else {
            console.error('Failed to delete chat:', result.responseMessage);
            alert('Failed to delete chat');
        }
    } catch (error) {
        console.error('Error deleting chat:', error);
        alert('Error deleting chat');
    }
};

onMounted(() => {
    loadChats();
});
</script>

<template>
    <div class="container py-4">
        <div class="row justify-content-center">
            <div class="col-12 col-lg-10">
                <div class="d-flex justify-content-between align-items-center mb-4">
                    <h2>My Chats</h2>
                    <button class="btn btn-primary" @click="$router.push('/chat/new')">
                        New Chat
                    </button>
                </div>

                <div v-if="loading" class="d-flex justify-content-center align-items-center py-5">
                    <div class="text-center">
                        <div class="spinner-border text-primary mb-3" role="status" style="width: 3rem; height: 3rem;">
                            <span class="visually-hidden">Loading...</span>
                        </div>
                        <div class="text-muted">Loading chats...</div>
                    </div>
                </div>

                <div v-else-if="chats.length > 0" class="row g-3">
                    <div 
                        v-for="chat in chats" 
                        :key="chat.uid"
                        class="col-12"
                    >
                        <div class="card shadow-sm" style="cursor: pointer;" @click="viewChat(chat.uid)">
                            <div class="card-body">
                                <div class="d-flex justify-content-between align-items-center">
                                    <div>
                                        <h5 class="card-title mb-1">Chat with User</h5>
                                        <p class="card-text text-muted small mb-0">
                                            Other User ID: {{ getOtherUserUid(chat) }}
                                        </p>
                                    </div>
                                    <button 
                                        class="btn btn-danger btn-sm" 
                                        @click="deleteChat(chat.uid, $event)"
                                    >
                                        Delete
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                
                <div v-else class="text-center py-5">
                    <i class="fas fa-comments fa-3x text-muted mb-3"></i>
                    <h5 class="text-muted">No chats found</h5>
                    <p class="text-muted">Start a new chat to begin messaging</p>
                    <button class="btn btn-primary mt-3" @click="$router.push('/chat/new')">
                        Create New Chat
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>

