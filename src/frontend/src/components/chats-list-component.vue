<script setup lang="ts">
import {onMounted, onBeforeUnmount, ref, watch, computed} from 'vue';
import {useRouter} from 'vue-router';
import {services, sockets} from '../services/api';
import type {ChatBo} from '../types/chat';
import type {UserProfileBo} from '../types/user';
import {useAuthStore} from '../stores/auth-store';
import {useNotification} from '../composables/use-notification.ts';
import {useContextMenu} from '../composables/use-context-menu';
import ProfileIconDisplay from './profile-icon-display.vue';

const router = useRouter();
const authStore = useAuthStore();
const notification = useNotification();
const {
    contextMenuItemId: contextMenuChatId,
    contextMenuPosition,
    showContextMenu,
    showMobileMenu,
    handleRightClick,
    toggleMobileMenu,
    closeAllMenus
} = useContextMenu();

const emit = defineEmits<{
    (e: 'update:chatSelected', chatId: string | undefined): void
}>();


const chats = ref<ChatBo[]>([]);
const chatsWithLastMessage = ref<Array<ChatBo & { lastMessageTime?: string, lastMessage?: string }>>([]);
const unreadCounts = ref<Record<string, number>>({});
const loading = ref(false);
const currentUserUid = ref<string>('');
const displayNames = ref<Map<string, string>>(new Map());
const userProfiles = ref<Map<string, UserProfileBo>>(new Map());
const searchQuery = ref<string>('');
const debouncedSearchQuery = ref<string>('');

let searchDebounceTimer: ReturnType<typeof setTimeout> | null = null;

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

            const otherUserUids = [...new Set(chats.value.map(chat => getOtherUserUid(chat)))];
            for (const userUid of otherUserUids) {
                if (userUid) {
                    const userProfileResult = await services.users.getUserProfile(userUid, currentUserUid.value);
                    if (userProfileResult.isSuccess && userProfileResult.data) {
                        userProfiles.value.set(userUid, userProfileResult.data);
                        displayNames.value.set(userUid, getDisplayUserName(
                            userUid,
                            userProfileResult.data.userName,
                            userProfileResult.data.firstName,
                            userProfileResult.data.lastName
                        ));
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

const filteredChats = computed(() => {
    if (!debouncedSearchQuery.value.trim()) {
        return chatsWithLastMessage.value;
    }
    
    const query = debouncedSearchQuery.value.toLowerCase().trim();
    return chatsWithLastMessage.value.filter(chat => {
        const displayName = getDisplayName(chat).toLowerCase();
        const lastMessage = (chat.lastMessage || '').toLowerCase();
        
        const hasRealTimestamp = chat.lastMessageTime && chat.lastMessageTime !== chat.uid;
        
        return displayName.includes(query) || (hasRealTimestamp && lastMessage.includes(query));
    });
});

const debounceSearch = () => {
    if (searchDebounceTimer) {
        clearTimeout(searchDebounceTimer);
    }
    
    searchDebounceTimer = setTimeout(() => {
        debouncedSearchQuery.value = searchQuery.value;
    }, 200);
};

watch(searchQuery, debounceSearch);

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
    emit('update:chatSelected', chatId);
    router.replace(`/chat/${chatId}`);
};

const getOtherUserUid = (chat: ChatBo) => {
    return chat.user1Uid === currentUserUid.value ? chat.user2Uid : chat.user1Uid;
};

const getDisplayName = (chat: ChatBo): string => {
    const otherUserUid = getOtherUserUid(chat);
    return displayNames.value.get(otherUserUid) || otherUserUid.substring(0, 8) + '...';
};

const markChatAsRead = async (chatId: string, event?: Event) => {
    if (event) {
        event.stopPropagation();
    }

    closeAllMenus();

    try {
        await sockets.hub.updateReadChat(chatId);
        const normalizedChatId = chatId.toLowerCase();
        unreadCounts.value[normalizedChatId] = 0;
        notification.success('Chat marked as read');
    } catch (error) {
        notification.error('Error marking chat as read');
    }
};

function getDisplayUserName(creatorUid: string, userName?: string, firstName?: string, lastName?: string): string {
    return userName ||
        (firstName && lastName
            ? `${firstName} ${lastName}`
            : creatorUid.substring(0, 8) + '...');
}

const viewProfile = (chatId: string, event?: Event) => {
    if (event) {
        event.stopPropagation();
    }

    closeAllMenus();

    const chat = chatsWithLastMessage.value.find(c => c.uid === chatId);
    if (chat) {
        const otherUserUid = getOtherUserUid(chat);
        router.push({path: '/profile', query: {id: otherUserUid}});
    }
};

const deleteChat = async (chatId: string, event?: Event) => {
    if (event) {
        event.stopPropagation();
    }

    closeAllMenus();

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
            return date.toLocaleDateString('en-US', {weekday: 'short'});
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

onBeforeUnmount(() => {
    if (searchDebounceTimer) {
        clearTimeout(searchDebounceTimer);
    }
});
</script>

<template>
    <div class="pb-4 flex-column">
        <div class="d-flex p-2 chat-list-header gap-5">
            <div class="flex-grow-1 align-items-center">
                <div class="input-group ms-4">
                    <span class="input-group-text bg-white border-end-0">
                        <i class="fas fa-search text-muted"></i>
                    </span>
                    <input
                        v-model="searchQuery"
                        type="text"
                        class="form-control border-start-0"
                        placeholder="Search chats..."
                    />
                </div>
            </div>
            <div class="flex-shrink-0">
                <button class="btn btn-primary" @click="router.push('/chat/new')">
                    New Chat
                </button>
            </div>
        </div>
        <div class="d-flex">
            <div class="col-12">
                <div v-if="loading" class="d-flex justify-content-center align-items-center py-5">
                    <div class="text-center">
                        <div class="spinner-border text-primary mb-3" role="status" style="width: 3rem; height: 3rem;">
                            <span class="visually-hidden">Loading...</span>
                        </div>
                        <div class="text-muted">Loading chats...</div>
                    </div>
                </div>

                <div v-else-if="filteredChats.length > 0" class="d-flex flex-column g-3 chat-list scrollable">
                    <div
                        v-for="chat in filteredChats"
                        :key="chat.uid"
                        class="col-12"
                    >
                        <div
                            class="shadow-sm"
                            style="cursor: pointer;"
                            @click="viewChat(chat.uid)"
                            @contextmenu="handleRightClick($event, chat.uid)"
                        >
                            <div class="card-body p-2">
                                <div class="d-flex align-items-center gap-3">
                                    <a
                                        href="#"
                                        class="text-primary text-decoration-none"
                                        @click.prevent.stop="router.push({ path: '/profile', query: { id: getOtherUserUid(chat) } })"
                                    >
                                        <ProfileIconDisplay 
                                            :icon="userProfiles.get(getOtherUserUid(chat))?.profileIcon" 
                                            :color="userProfiles.get(getOtherUserUid(chat))?.profileIconColor" 
                                            size="xl"
                                            class="bg-white"
                                        />
                                    </a>
                                    <div class="flex-grow-1">
                                        <div class="d-flex justify-content-between align-items-start mb-1">
                                            <div>
                                                <h5 class="card-title mb-1"
                                                    style="max-width: 150px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;">
                                                    <a
                                                        href="#"
                                                        class="text-primary text-decoration-none"
                                                        @click.prevent.stop="router.push({ path: '/profile', query: { id: getOtherUserUid(chat) } })"
                                                    >
                                                        {{ getDisplayName(chat) }}
                                                    </a>
                                                </h5>
                                                <p class="card-text text-muted small mb-0"
                                                   style="max-width: 200px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;">
                                                    {{ chat.lastMessage }}
                                                </p>
                                            </div>
                                            <div class="d-flex align-items-center gap-2">
                                                <span
                                                    v-if="unreadCounts[chat.uid.toLowerCase()] && unreadCounts[chat.uid.toLowerCase()]! > 0"
                                                    class="badge bg-primary rounded-pill"
                                                >
                                                    {{ unreadCounts[chat.uid.toLowerCase()] }}
                                                </span>
                                                <small v-if="chat.lastMessage !== 'No messages yet'" class="text-muted">{{
                                                    formatLastMessageTime(chat.lastMessageTime)
                                                }}</small>
                                                <div class="position-relative">
                                                    <button
                                                        class="btn btn-link text-dark p-1"
                                                        @click="toggleMobileMenu($event, chat.uid)"
                                                        style="line-height: 1;"
                                                    >
                                                        <i class="fas fa-ellipsis-v"></i>
                                                    </button>
                                                <div
                                                    v-if="showMobileMenu === chat.uid"
                                                    class="dropdown-menu show position-absolute"
                                                    style="right: 0; top: 100%;"
                                                >
                                                    <button
                                                        class="dropdown-item"
                                                        @click="markChatAsRead(chat.uid, $event)"
                                                        v-if="unreadCounts[chat.uid.toLowerCase()] && unreadCounts[chat.uid.toLowerCase()]! > 0"
                                                    >
                                                        <i class="fas fa-check me-2"></i>Mark as Read
                                                    </button>
                                                    <button
                                                        class="dropdown-item"
                                                        @click="viewProfile(chat.uid, $event)"
                                                    >
                                                        <i class="fas fa-user me-2"></i>View Profile
                                                    </button>
                                                    <div class="dropdown-divider"></div>
                                                    <button
                                                        class="dropdown-item text-danger"
                                                        @click="deleteChat(chat.uid, $event)"
                                                    >
                                                        <i class="fas fa-trash me-2"></i>Delete
                                                    </button>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
                
                <div v-else-if="chatsWithLastMessage.length > 0 && filteredChats.length === 0" class="text-center py-5">
                    <div class="text-muted">
                        <i class="fas fa-search fa-3x mb-3"></i>
                        <h4>No chats found</h4>
                        <p>Try adjusting your search terms</p>
                    </div>
                </div>
                
                <div v-else class="text-center py-5">
                    <div class="text-muted">
                        <i class="fas fa-comments fa-3x mb-3"></i>
                        <h4>No chats yet</h4>
                        <p>Start a conversation with someone!</p>
                        <button class="btn btn-primary" @click="router.push('/chat/new')">
                            <i class="fas fa-plus me-1"></i>
                            New Chat
                        </button>
                    </div>
                </div>
            </div>
        </div>

        <div
            v-if="showContextMenu && contextMenuChatId"
            class="dropdown-menu show position-fixed"
            :style="{ 
                left: contextMenuPosition.x + 'px', 
                top: contextMenuPosition.y + 'px' 
            }"
        >
            <button
                v-if="unreadCounts[contextMenuChatId.toLowerCase()] && unreadCounts[contextMenuChatId.toLowerCase()]! > 0"
                class="dropdown-item"
                @click="markChatAsRead(contextMenuChatId)"
            >
                <i class="fas fa-check me-2"></i>Mark as Read
            </button>
            <button
                class="dropdown-item"
                @click="viewProfile(contextMenuChatId)"
            >
                <i class="fas fa-user me-2"></i>View Profile
            </button>
            <div class="dropdown-divider"></div>
            <button
                class="dropdown-item text-danger"
                @click="deleteChat(contextMenuChatId)"
            >
                <i class="fas fa-trash me-2"></i>Delete Chat
            </button>
        </div>
    </div>
</template>

