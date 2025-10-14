<script setup lang="ts">
import { computed } from 'vue'
import { useErrorStore } from '../../stores/error-store'
import type { ErrorNotification } from '../../stores/error-store'
import type { ErrorCategory } from '../../types/error'

const errorStore = useErrorStore()

const activeNotifications = computed(() => errorStore.getActiveNotifications())

function getIconClass(category: ErrorCategory): string {
    switch (category) {
        case 'authentication':
            return 'bi-shield-lock'
        case 'validation':
            return 'bi-exclamation-triangle'
        case 'not_found':
            return 'bi-search'
        case 'conflict':
            return 'bi-exclamation-diamond'
        case 'permission':
            return 'bi-shield-exclamation'
        case 'server':
        case 'network':
            return 'bi-exclamation-circle'
        default:
            return 'bi-info-circle'
    }
}

function getAlertClass(category: ErrorCategory): string {
    switch (category) {
        case 'authentication':
        case 'permission':
            return 'alert-warning'
        case 'validation':
            return 'alert-info'
        case 'not_found':
            return 'alert-secondary'
        case 'conflict':
            return 'alert-warning'
        case 'server':
        case 'network':
            return 'alert-danger'
        default:
            return 'alert-primary'
    }
}

function dismissNotification(notification: ErrorNotification) {
    errorStore.dismissError(notification.id)
}

function hasFieldErrors(notification: ErrorNotification): boolean {
    return !!(notification.error.fieldErrors && Object.keys(notification.error.fieldErrors).length > 0)
}
</script>

<template>
    <div class="position-fixed top-0 end-0 p-3" style="z-index: 1050; margin-top: 80px;">
        <div
            v-for="notification in activeNotifications"
            :key="notification.id"
            :class="['alert', getAlertClass(notification.error.category), 'alert-dismissible', 'fade', 'show', 'mb-2', 'shadow']"
            role="alert"
            style="max-width: 400px;"
        >
            <div class="d-flex align-items-start">
                <i :class="['bi', getIconClass(notification.error.category), 'me-2', 'fs-5']"></i>
                <div class="flex-grow-1">
                    <div class="fw-bold mb-1">
                        {{ notification.error.details?.title || 'Error' }}
                    </div>
                    <div class="mb-2">
                        {{ notification.error.message }}
                    </div>
                    
                    <!-- Field-specific validation errors -->
                    <div v-if="hasFieldErrors(notification)" class="mt-2">
                        <div class="small">
                            <div 
                                v-for="(errors, field) in notification.error.fieldErrors" 
                                :key="field"
                                class="mb-1"
                            >
                                <strong>{{ field }}:</strong>
                                <ul class="mb-0 ps-3">
                                    <li v-for="(error, index) in errors" :key="index">{{ error }}</li>
                                </ul>
                            </div>
                        </div>
                    </div>

                    <!-- Additional debug info in development -->
                    <div v-if="notification.error.details?.traceId" class="small text-muted mt-2">
                        Trace ID: {{ notification.error.details.traceId }}
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
