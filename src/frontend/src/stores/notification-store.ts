import { reactive, computed } from 'vue'
import type { SuccessNotification } from '../types/notification'

export interface NotificationItem {
    id: string
    notification: SuccessNotification
    timestamp: Date
    dismissed: boolean
}

interface NotificationState {
    notifications: NotificationItem[]
}

const state = reactive<NotificationState>({
    notifications: []
})

const maxNotifications = 5

/**
 * Add a new success notification
 */
function addSuccess(notification: SuccessNotification): string {
    const id = `notification-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`
    
    const item: NotificationItem = {
        id,
        notification,
        timestamp: new Date(),
        dismissed: false
    }

    state.notifications.unshift(item)

    // Keep only the most recent notifications
    if (state.notifications.length > maxNotifications) {
        state.notifications = state.notifications.slice(0, maxNotifications)
    }

    // Auto-dismiss after 4 seconds
    setTimeout(() => {
        dismissNotification(id)
    }, 4000)

    return id
}

/**
 * Dismiss a notification
 */
function dismissNotification(id: string) {
    const notification = state.notifications.find(n => n.id === id)
    if (notification) {
        notification.dismissed = true
        
        // Remove from array after animation
        setTimeout(() => {
            state.notifications = state.notifications.filter(n => n.id !== id)
        }, 300)
    }
}

/**
 * Clear all notifications
 */
function clearAll() {
    state.notifications.forEach(n => n.dismissed = true)
    setTimeout(() => {
        state.notifications = []
    }, 300)
}

/**
 * Get active (non-dismissed) notifications
 */
function getActiveNotifications(): NotificationItem[] {
    return state.notifications.filter(n => !n.dismissed)
}

export const useNotificationStore = () => {
    const notifications = computed(() => state.notifications)

    return {
        notifications,
        addSuccess,
        dismissNotification,
        clearAll,
        getActiveNotifications
    }
}

