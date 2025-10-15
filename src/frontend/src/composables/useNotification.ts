import {readonly, ref} from 'vue';

export type NotificationType = 'success' | 'error' | 'warning' | 'info' | 'created' | 'updated' | 'deleted';

export interface Toast {
    id: string;
    type: NotificationType;
    title?: string;
    message: string;
    timestamp: Date;
}

interface ToastConfig {
    title?: string;
    duration?: number;
}

const toasts = ref<Toast[]>([]);
let idCounter = 0;

const generateId = (): string => {
    return `toast-${Date.now()}-${idCounter++}`;
};

const addToast = (
    type: NotificationType,
    message: string,
    config: ToastConfig = {}
): string => {
    const id = generateId();
    const {title, duration = 5000} = config;

    const toast: Toast = {
        id,
        type,
        title,
        message,
        timestamp: new Date(),
    };

    toasts.value.push(toast);

    if (duration > 0) {
        setTimeout(() => {
            removeToast(id);
        }, duration);
    }

    return id;
};

const removeToast = (id: string): void => {
    const index = toasts.value.findIndex((toast) => toast.id === id);
    if (index !== -1) {
        toasts.value.splice(index, 1);
    }
};

const clearAll = (): void => {
    toasts.value = [];
};

export const useNotification = () => {
    return {
        toasts: readonly(toasts),

        success: (message: string, config?: ToastConfig) => {
            return addToast('success', message, {title: 'Success', ...config});
        },

        created: (message: string, config?: ToastConfig) => {
            return addToast('created', message, {title: 'Created', ...config});
        },

        updated: (message: string, config?: ToastConfig) => {
            return addToast('updated', message, {title: 'Updated', ...config});
        },

        deleted: (message: string, config?: ToastConfig) => {
            return addToast('deleted', message, {title: 'Deleted', ...config});
        },

        error: (message: string, config?: ToastConfig) => {
            return addToast('error', message, {title: 'Error', ...config});
        },

        warning: (message: string, config?: ToastConfig) => {
            return addToast('warning', message, {title: 'Warning', ...config});
        },

        info: (message: string, config?: ToastConfig) => {
            return addToast('info', message, {title: 'Info', ...config});
        },

        remove: removeToast,
        clearAll,
    };
};
