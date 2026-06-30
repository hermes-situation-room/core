<script setup lang="ts">
import { ref, watch } from 'vue';
import { services } from '../services/api';
import { useAuthStore } from '../stores/auth-store';
import { useNotification } from '../composables/use-notification.ts';

interface Props {
    show: boolean;
    targetType: string;
    targetUid: string;
    targetLabel?: string;
}

const props = withDefaults(defineProps<Props>(), {
    targetLabel: '',
});

const emit = defineEmits<{
    (e: 'close'): void;
    (e: 'reported'): void;
}>();

const authStore = useAuthStore();
const notification = useNotification();

const reason = ref('');
const submitting = ref(false);

watch(() => props.show, (visible) => {
    if (!visible) {
        reason.value = '';
        submitting.value = false;
    }
});

const handleClose = () => {
    if (submitting.value) {
        return;
    }
    emit('close');
};

const handleSubmit = async () => {
    if (!authStore.userId.value) {
        notification.warning('You must be logged in to report content');
        return;
    }

    const trimmedReason = reason.value.trim();
    if (!trimmedReason) {
        notification.warning('Please describe why you are reporting this');
        return;
    }

    submitting.value = true;
    try {
        const result = await services.reports.createReport({
            reporterUid: authStore.userId.value,
            targetType: props.targetType,
            targetUid: props.targetUid,
            reason: trimmedReason,
        });

        if (result.isSuccess) {
            notification.success('Thank you. Your report has been submitted for review.');
            emit('reported');
            emit('close');
        } else {
            notification.error(result.responseMessage || 'Failed to submit report');
        }
    } catch (err) {
        notification.error(err instanceof Error ? err.message : 'An error occurred while submitting the report');
    } finally {
        submitting.value = false;
    }
};
</script>

<template>
    <div
        v-if="show"
        class="modal fade show d-block"
        tabindex="-1"
        style="background-color: var(--modal-backdrop); z-index: 1060;"
        @click.self="handleClose"
    >
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">
                        <i class="fas fa-flag me-2"></i>
                        Report {{ targetLabel || targetType }}
                    </h5>
                    <button
                        type="button"
                        class="btn-close"
                        @click="handleClose"
                        :disabled="submitting"
                    ></button>
                </div>
                <div class="modal-body">
                    <p class="text-muted small">
                        Tell us what's wrong with this {{ (targetLabel || targetType).toLowerCase() }}.
                        A moderator will review your report.
                    </p>
                    <form @submit.prevent="handleSubmit">
                        <div class="mb-2">
                            <label for="reportReason" class="form-label">Reason <span class="text-danger">*</span></label>
                            <textarea
                                id="reportReason"
                                v-model="reason"
                                class="form-control"
                                rows="4"
                                maxlength="1000"
                                placeholder="e.g. Spam, harassment, misinformation..."
                                :disabled="submitting"
                                required
                            ></textarea>
                        </div>
                    </form>
                </div>
                <div class="modal-footer">
                    <button
                        type="button"
                        class="btn btn-secondary"
                        @click="handleClose"
                        :disabled="submitting"
                    >
                        Cancel
                    </button>
                    <button
                        type="button"
                        class="btn btn-danger"
                        @click="handleSubmit"
                        :disabled="submitting || !reason.trim()"
                    >
                        <span v-if="submitting" class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                        <i v-else class="fas fa-flag me-1"></i>
                        {{ submitting ? 'Submitting...' : 'Submit Report' }}
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>
