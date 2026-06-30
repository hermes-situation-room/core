<template>
    <div class="position-relative">
        <button 
            class="btn btn-link text-white position-relative"
            @click="toggleDropdown"
            :disabled="loading"
        >
            <i class="fas fa-bell"></i>
            <span 
                v-if="unreadCount > 0" 
                class="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger"
                style="font-size: 0.6rem;"
            >
                {{ unreadCount > 99 ? '99+' : unreadCount }}
            </span>
        </button>

        <!-- Notification Dropdown -->
        <div 
            v-if="showDropdown" 
            class="dropdown-menu show position-absolute notification-dropdown"
            style="right: 0; top: 100%; min-width: 350px; max-width: 400px;"
        >
            <div class="d-flex justify-content-between align-items-center p-3 border-bottom">
                <h6 class="mb-0">Notifications</h6>
                <div class="d-flex gap-2">
                    <button 
                        v-if="unreadCount > 0"
                        class="btn btn-sm btn-outline-primary"
                        @click="markAllAsRead"
                        :disabled="markingAllAsRead"
                    >
                        <i class="fas fa-check-double me-1"></i>
                        Mark all read
                    </button>
                    <button 
                        class="btn btn-sm btn-outline-secondary"
                        @click="refreshNotifications"
                        :disabled="loading"
                    >
                        <i class="fas fa-sync-alt me-1" :class="{ 'fa-spin': loading }"></i>
                        Refresh
                    </button>
                </div>
            </div>

            <div class="notification-list" style="max-height: 400px; overflow-y: auto;">
                <div v-if="loading" class="p-3 text-center">
                    <div class="spinner-border spinner-border-sm" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                </div>
                
                <div v-else-if="notifications.length === 0" class="p-3 text-center text-muted">
                    <i class="fas fa-bell-slash fa-2x mb-2"></i>
                    <div>No notifications</div>
                </div>
                
                <div v-else>
                    <div 
                        v-for="notification in notifications" 
                        :key="notification.uid"
                        class="notification-item p-3 border-bottom"
                        :class="{ 'bg-light': !notification.isRead }"
                        @click="handleNotificationClick(notification)"
                        style="cursor: pointer;"
                    >
                        <div class="d-flex align-items-start gap-2">
                            <div class="flex-shrink-0">
                                <i 
                                    :class="getNotificationIcon(notification.type)" 
                                    class="fa-lg"
                                    :style="{ color: getNotificationColor(notification.type) }"
                                ></i>
                            </div>
                            <div class="flex-grow-1">
                                <div class="d-flex justify-content-between align-items-start">
                                    <h6 class="mb-1" :class="{ 'fw-bold': !notification.isRead }">
                                        {{ notification.title }}
                                    </h6>
                                    <small class="text-muted">
                                        {{ formatTime(notification.createdAt) }}
                                    </small>
                                </div>
                                <p class="mb-1 text-muted small">
                                    {{ notification.message }}
                                </p>
                                <div class="d-flex justify-content-between align-items-center">
                                    <small class="text-muted">
                                        {{ formatDate(notification.createdAt) }}
                                    </small>
                                    <button 
                                        v-if="!notification.isRead"
                                        class="btn btn-sm btn-outline-primary"
                                        @click.stop="markAsRead(notification.uid)"
                                        :disabled="markingAsRead === notification.uid"
                                    >
                                        <i class="fas fa-check me-1"></i>
                                        Mark read
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { services } from '../services/api';
import { useAuthStore } from '../stores/auth-store';
import { useNotification } from '../composables/use-notification';
import type { NotificationBo } from '../types/notification';

const router = useRouter();
const authStore = useAuthStore();
const notification = useNotification();

const showDropdown = ref(false);
const loading = ref(false);
const notifications = ref<NotificationBo[]>([]);
const unreadCount = ref(0);
const markingAsRead = ref<string | null>(null);
const markingAllAsRead = ref(false);

const currentUserUid = computed(() => authStore.userId.value || '');

const getNotificationIcon = (type: string) => {
    switch (type) {
        case 'message': return 'fas fa-comment';
        case 'comment': return 'fas fa-comment-dots';
        case 'reply': return 'fas fa-reply';
        case 'like': return 'fas fa-heart';
        default: return 'fas fa-bell';
    }
};

const getNotificationColor = (type: string) => {
    switch (type) {
        case 'message': return 'var(--notification-message)';
        case 'comment': return 'var(--notification-comment)';
        case 'reply': return 'var(--notification-reply)';
        case 'like': return 'var(--notification-like)';
        default: return 'var(--notification-default)';
    }
};

const formatTime = (dateString: string) => {
    const date = new Date(dateString + 'Z');
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins}m ago`;
    if (diffHours < 24) return `${diffHours}h ago`;
    if (diffDays < 7) return `${diffDays}d ago`;
    return date.toLocaleDateString();
};

const formatDate = (dateString: string) => {
    const date = new Date(dateString + 'Z');
    return date.toLocaleDateString();
};

const toggleDropdown = () => {
    showDropdown.value = !showDropdown.value;
    if (showDropdown.value) {
        loadNotifications();
    }
};

const loadNotifications = async () => {
    if (!currentUserUid.value) return;
    
    loading.value = true;
    try {
        const result = await services.notifications.getByUserUid(currentUserUid.value);
        if (result.isSuccess && result.data) {
            notifications.value = result.data;
        }
        
        const countResult = await services.notifications.getUnreadCount(currentUserUid.value);
        if (countResult.isSuccess && countResult.data !== undefined) {
            unreadCount.value = countResult.data;
        }
    } catch (error) {
        console.error('Error loading notifications:', error);
        notification.error('Failed to load notifications');
    } finally {
        loading.value = false;
    }
};

const markAsRead = async (uid: string) => {
    markingAsRead.value = uid;
    try {
        const result = await services.notifications.markAsRead(uid);
        if (result.isSuccess) {
            const notification = notifications.value.find(n => n.uid === uid);
            if (notification) {
                notification.isRead = true;
            }
            unreadCount.value = Math.max(0, unreadCount.value - 1);
        }
    } catch (error) {
        console.error('Error marking notification as read:', error);
        notification.error('Failed to mark notification as read');
    } finally {
        markingAsRead.value = null;
    }
};

const markAllAsRead = async () => {
    if (!currentUserUid.value) return;
    
    markingAllAsRead.value = true;
    try {
        const result = await services.notifications.markAllAsRead(currentUserUid.value);
        if (result.isSuccess) {
            notifications.value.forEach(n => n.isRead = true);
            unreadCount.value = 0;
        }
    } catch (error) {
        console.error('Error marking all notifications as read:', error);
        notification.error('Failed to mark all notifications as read');
    } finally {
        markingAllAsRead.value = false;
    }
};

const refreshNotifications = () => {
    loadNotifications();
};

const handleNotificationClick = (notification: NotificationBo) => {
    // Mark as read if not already read
    if (!notification.isRead) {
        markAsRead(notification.uid);
    }
    
    // Navigate based on notification type
    if (notification.relatedEntityType === 'chat' && notification.relatedEntityUid) {
        router.push(`/chat/${notification.relatedEntityUid}`);
    } else if (notification.relatedEntityType === 'post' && notification.relatedEntityUid) {
        router.push(`/post/${notification.relatedEntityUid}`);
    }
    
    showDropdown.value = false;
};

// Close dropdown when clicking outside
const handleClickOutside = (event: Event) => {
    const target = event.target as HTMLElement;
    if (!target.closest('.notification-bell')) {
        showDropdown.value = false;
    }
};

onMounted(() => {
    document.addEventListener('click', handleClickOutside);
    // Load initial unread count
    if (currentUserUid.value) {
        loadNotifications();
    }
    
    // Listen for real-time notifications
    window.addEventListener('new-notification', handleNewNotification as EventListener);
});

onUnmounted(() => {
    document.removeEventListener('click', handleClickOutside);
    window.removeEventListener('new-notification', handleNewNotification as EventListener);
});

const handleNewNotification = (event: CustomEvent) => {
    const notification = event.detail;
    // Add new notification to the top of the list
    notifications.value.unshift(notification);
    // Increment unread count
    if (!notification.isRead) {
        unreadCount.value++;
    }
};
</script>

<style scoped>
.notification-dropdown {
    z-index: 1050;
}

.notification-item:hover {
    background-color: var(--dropdown-hover) !important;
}

.notification-item:last-child {
    border-bottom: none !important;
}
</style>
