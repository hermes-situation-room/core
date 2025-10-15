<script setup lang="ts">
import {type NotificationType, useNotification} from '../composables/useNotification';

const {toasts, remove} = useNotification();

const getHeaderClass = (type: NotificationType): string => {
    const classes: Record<NotificationType, string> = {
        success: 'bg-success text-white',
        created: 'bg-success text-white',
        updated: 'bg-info text-white',
        deleted: 'bg-secondary text-white',
        error: 'bg-danger text-white',
        warning: 'bg-warning text-dark',
        info: 'bg-info text-white',
    };
    return classes[type] || 'bg-light';
};

const getIconClass = (type: NotificationType): string => {
    const icons: Record<NotificationType, string> = {
        success: 'bi bi-check-circle-fill',
        created: 'bi bi-plus-circle-fill',
        updated: 'bi bi-pencil-square',
        deleted: 'bi bi-trash-fill',
        error: 'bi bi-exclamation-circle-fill',
        warning: 'bi bi-exclamation-triangle-fill',
        info: 'bi bi-info-circle-fill',
    };
    return icons[type] || 'bi bi-bell-fill';
};

const isLightBackground = (type: NotificationType): boolean => {
    return type === 'warning';
};

const getTimeAgo = (timestamp: Date): string => {
    const seconds = Math.floor((new Date().getTime() - timestamp.getTime()) / 1000);

    if (seconds < 10) return 'just now';
    if (seconds < 60) return `${seconds}s ago`;

    const minutes = Math.floor(seconds / 60);
    if (minutes < 60) return `${minutes}m ago`;

    const hours = Math.floor(minutes / 60);
    return `${hours}h ago`;
};
</script>

<template>
    <div
        class="toast-container position-fixed top-0 end-0 p-3"
        style="z-index: 9999"
    >
        <div
            v-for="toast in toasts"
            :key="toast.id"
            class="toast show"
            role="alert"
            aria-live="assertive"
            aria-atomic="true"
        >
            <div
                class="toast-header"
                :class="getHeaderClass(toast.type)"
            >
                <i
                    class="me-2"
                    :class="getIconClass(toast.type)"
                ></i>
                <strong class="me-auto">{{ toast.title }}</strong>
                <small>{{ getTimeAgo(toast.timestamp) }}</small>
                <button
                    type="button"
                    class="btn-close"
                    :class="isLightBackground(toast.type) ? '' : 'btn-close-white'"
                    @click="remove(toast.id)"
                    aria-label="Close"
                ></button>
            </div>
            <div class="toast-body">
                {{ toast.message }}
            </div>
        </div>
    </div>
</template>
