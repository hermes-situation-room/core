import { reactive, computed } from 'vue'
import type { AppError } from '../types/error'

export interface ErrorNotification {
    id: string
    error: AppError
    timestamp: Date
    dismissed: boolean
}

interface ErrorState {
    notifications: ErrorNotification[]
}

const state = reactive<ErrorState>({
    notifications: []
})

const maxNotifications = 5

function addError(error: AppError): string {
    const id = `error-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`
    
    const notification: ErrorNotification = {
        id,
        error,
        timestamp: new Date(),
        dismissed: false
    }

    state.notifications.unshift(notification)

    if (state.notifications.length > maxNotifications) {
        state.notifications = state.notifications.slice(0, maxNotifications)
    }

    if (error.category !== 'server' && error.category !== 'network') {
        setTimeout(() => {
            dismissError(id)
        }, 5000)
    }

    return id
}

function dismissError(id: string) {
    const notification = state.notifications.find(n => n.id === id)
    if (notification) {
        notification.dismissed = true
        
        setTimeout(() => {
            state.notifications = state.notifications.filter(n => n.id !== id)
        }, 300)
    }
}

function clearAll() {
    state.notifications.forEach(n => n.dismissed = true)
    setTimeout(() => {
        state.notifications = []
    }, 300)
}

function getActiveNotifications(): ErrorNotification[] {
    return state.notifications.filter(n => !n.dismissed)
}

export const useErrorStore = () => {
    const notifications = computed(() => state.notifications)

    return {
        notifications,
        addError,
        dismissError,
        clearAll,
        getActiveNotifications
    }
}

