<script setup lang="ts">
import {onMounted, ref} from 'vue';
import {useRouter} from 'vue-router';
import {services, sockets} from '../services/api';
import type {ChatBo} from '../types/chat';
import { useAuthStore } from '../stores/auth-store';
import { useNotification } from '../composables/use-notification.ts';

const router = useRouter();
const authStore = useAuthStore();
const notification = useNotification();

const chats = ref<ChatBo[]>([]);
const chatsWithLastMessage = ref<Array<ChatBo & { lastMessageTime?: string, lastMessage?: string }>>([]);
const unreadCounts = ref<Record<string, number>>({});
const loading = ref(false);
const currentUserUid = ref<string>('');
const displayNames = ref<Map<string, string>>(new Map());

const loadChats = async () => {
    loading.value = true;
    try {
        currentUserUid.value = authStore.userId.value || '';
        
        if (!currentUserUid.value) {
            notification.error('You must be logged in to view chats');
            router.push("/login");
            return;
        }

        const result = await services.chats.getChatsByUser(currentUserUid.value);
        if (result.isSuccess && result.data) {
            chats.value = result.data;
            
            // Load display names for all other users in chats
            const otherUserUids = [...new Set(chats.value.map(chat => getOtherUserUid(chat)))];
            for (const userUid of otherUserUids) {
                if (userUid) {
                    const displayNameResult = await services.users.getDisplayName(userUid);
                    if (displayNameResult.isSuccess && displayNameResult.data) {
                        displayNames.value.set(userUid, displayNameResult.data);
                    }
                }
            }
            
            await Promise.all([
                loadChatsWithLastMessage(),
                loadUnreadCounts()
            ]);
        } else {
            notification.error(result.responseMessage || 'Failed to load chats');
        }
    } catch (error) {
        notification.error('Error loading chats');
    } finally {
        loading.value = false;
    }
};

const loadChatsWithLastMessage = async () => {
    try {
        const chatsWithMessages = await Promise.all(
            chats.value.map(async (chat) => {
                try {
                    const messagesResult = await services.messages.getMessagesByChat(chat.uid);
                    if (messagesResult.isSuccess && messagesResult.data && messagesResult.data.length > 0) {
                        const lastMessage = messagesResult.data[messagesResult.data.length - 1];
                        if (lastMessage) {
                            return {
                                ...chat,
                                lastMessageTime: lastMessage.timestamp,
                                lastMessage: lastMessage.content
                            };
                        }
                    }
                    return {
                        ...chat,
                        lastMessageTime: chat.uid,
                        lastMessage: 'No messages yet'
                    };
                } catch (error) {
                    return {
                        ...chat,
                        lastMessageTime: chat.uid,
                        lastMessage: 'No messages yet'
                    };
                }
            })
        );

        chatsWithLastMessage.value = chatsWithMessages.sort((a, b) => {
            const timeA = new Date(a.lastMessageTime || '').getTime();
            const timeB = new Date(b.lastMessageTime || '').getTime();
            return timeB - timeA;
        });
    } catch (error) {
        chatsWithLastMessage.value = chats.value.map(chat => ({
            ...chat,
            lastMessageTime: chat.uid,
            lastMessage: 'No messages yet'
        }));
    }
};

const loadUnreadCounts = async () => {
    try {
        const counts: Record<string, number> = {};
        await Promise.all(
            chats.value.map(async (chat) => {
                try {
                    const result = await services.userChatMessageStatus.getUnreadMessageCountPerChat(
                        currentUserUid.value,
                        chat.uid
                    );
                    const normalizedChatId = chat.uid.toLowerCase();
                    if (result.isSuccess && result.data) {
                        counts[normalizedChatId] = result.data.countUnreadMessages;
                    } else {
                        counts[normalizedChatId] = 0;
                    }
                } catch (error) {
                    counts[chat.uid.toLowerCase()] = 0;
                }
            })
        );
        unreadCounts.value = counts;
    } catch (error) {
        console.error('Error loading unread counts:', error);
    }
};

const viewChat = (chatId: string) => {
    router.push(`/chat/${chatId}`);
};

const getOtherUserUid = (chat: ChatBo) => {
    return chat.user1Uid === currentUserUid.value ? chat.user2Uid : chat.user1Uid;
};

const getDisplayName = (chat: ChatBo): string => {
    const otherUserUid = getOtherUserUid(chat);
    return displayNames.value.get(otherUserUid) || otherUserUid.substring(0, 8) + '...';
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
            chatsWithLastMessage.value = chatsWithLastMessage.value.filter(chat => chat.uid !== chatId);
            notification.deleted('Chat deleted successfully');
        } else {
            notification.error(result.responseMessage || 'Failed to delete chat');
        }
    } catch (error) {
        notification.error('Error deleting chat');
    }
};

const formatLastMessageTime = (timestamp?: string) => {
    if (!timestamp) return '';
    
    try {
        let dateString = timestamp;
        
        if (!timestamp.includes('Z') && !timestamp.includes('+') && !timestamp.includes('-', 10)) {
            dateString = timestamp + 'Z';
        }
        
        const date = new Date(dateString);
        
        if (isNaN(date.getTime())) {
            return '';
        }
        const now = new Date();
        const diffMs = now.getTime() - date.getTime();
        const diffHours = diffMs / (1000 * 60 * 60);
        const diffDays = diffMs / (1000 * 60 * 60 * 24);
        
        if (diffHours < 24) {
            return date.toLocaleTimeString(navigator.language || 'en-US', {
                hour: '2-digit',
                minute: '2-digit'
            });
        } else if (diffDays < 7) {
            return date.toLocaleDateString('en-US', { weekday: 'short' });
        } else {
            return date.toLocaleDateString('en-US', {
                month: 'short',
                day: 'numeric'
            });
        }
    } catch (error) {
        return '';
    }
};

const handleUnreadMessageUpdate = (chatId: string, count: number) => {
    const normalizedChatId = chatId.toLowerCase();
    unreadCounts.value[normalizedChatId] = count;
};

const isSocketConnected = ref(false);

onMounted(async () => {
    await loadChats();
        try {
        await sockets.hub.initialize();
        await sockets.hub.registerToEvent('NewUnreadChatMessage', handleUnreadMessageUpdate);
        await sockets.hub.joinMessaging();
        isSocketConnected.value = true;
    } catch (error) {
        console.warn('Failed to connect to real-time messaging. Badge counts will not update automatically:', error);
        isSocketConnected.value = false;
    }
});
</script>

<template>
    <div class="container py-4">
        <div class="row justify-content-center">
            <div class="col-12 col-lg-10">
                <div class="d-flex justify-content-between align-items-center mb-4">
                    <h2>My Chats</h2>
                    <button class="btn btn-primary" @click="router.push('/chat/new')">
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

                <div v-else-if="chatsWithLastMessage.length > 0" class="row g-3">
                    <div 
                        v-for="chat in chatsWithLastMessage" 
                        :key="chat.uid"
                        class="col-12"
                    >
                        <div class="card shadow-sm" style="cursor: pointer;" @click="viewChat(chat.uid)">
                            <div class="card-body">
                                <div class="d-flex justify-content-between align-items-start">
                                    <div class="flex-grow-1">
                                        <div class="d-flex justify-content-between align-items-start mb-2">
                                            <div class="d-flex align-items-center gap-2">
                                                <h5 class="card-title mb-0" style="max-width: 200px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;">
                                                    <a 
                                                        href="#" 
                                                        class="text-primary text-decoration-none"
                                                        @click.prevent.stop="router.push({ path: '/profile', query: { id: getOtherUserUid(chat) } })"
                                                    >
                                                        {{ getDisplayName(chat) }}
                                                    </a>
                                                </h5>
                                                <span 
                                                    v-if="unreadCounts[chat.uid.toLowerCase()] && unreadCounts[chat.uid.toLowerCase()]! > 0" 
                                                    class="badge bg-primary rounded-pill"
                                                >
                                                    {{ unreadCounts[chat.uid.toLowerCase()] }}
                                                </span>
                                            </div>
                                            <small v-if="chat.lastMessage !== 'No messages yet'" class="text-muted">{{ formatLastMessageTime(chat.lastMessageTime) }}</small>
                                        </div>
                                        <p class="card-text text-muted small mb-0" style="max-width: 200px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;">
                                            {{ chat.lastMessage }}
                                        </p>
                                    </div>
                                    <button 
                                        class="btn btn-danger btn-sm ms-3" 
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
                    <button class="btn btn-primary mt-3" @click="router.push('/chat/new')">
                        Create New Chat
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>

