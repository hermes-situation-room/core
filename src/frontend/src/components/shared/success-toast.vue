<script setup lang="ts">
import { computed } from 'vue'
import { useNotificationStore } from '../../stores/notification-store'
import type { NotificationItem } from '../../stores/notification-store'
import type { SuccessCategory } from '../../types/notification'

const notificationStore = useNotificationStore()

const activeNotifications = computed(() => notificationStore.getActiveNotifications())

function getIconClass(category: SuccessCategory): string {
    switch (category) {
        case 'registration':
            return 'bi-person-check'
        case 'login':
            return 'bi-box-arrow-in-right'
        case 'update':
            return 'bi-pencil-square'
        case 'delete':
            return 'bi-trash'
        case 'create':
            return 'bi-plus-circle'
        default:
            return 'bi-check-circle'
    }
}

function dismissNotification(notification: NotificationItem) {
    notificationStore.dismissNotification(notification.id)
}
</script>

<template>
    <div class="position-fixed top-0 end-0 p-3" style="z-index: 1055; margin-top: 80px;">
        <div
            v-for="notification in activeNotifications"
            :key="notification.id"
            class="alert alert-success alert-dismissible fade show mb-2 shadow"
            role="alert"
            style="max-width: 400px; min-width: 300px;"
        >
            <div class="d-flex align-items-start">
                <i :class="['bi', getIconClass(notification.notification.category), 'me-2', 'fs-5']"></i>
                <div class="flex-grow-1">
                    <div v-if="notification.notification.title" class="fw-bold mb-1">
                        {{ notification.notification.title }}
                    </div>
                    <div>
                        {{ notification.notification.message }}
                    </div>
                </div>
                <button
                    type="button"
                    class="btn-close"
                    @click="dismissNotification(notification)"
                    aria-label="Close"
                ></button>
            </div>
        </div>
    </div>
</template>

