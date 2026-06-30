<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { services } from '../services/api';
import type { TagRequestDto } from '../services/api/tag-requests-api.ts';
import { useNotification } from '../composables/use-notification.ts';

const notification = useNotification();

const pendingRequests = ref<TagRequestDto[]>([]);
const loading = ref(false);
const accessDenied = ref(false);
const processingUid = ref<string | null>(null);

const loadPendingRequests = async () => {
    loading.value = true;
    accessDenied.value = false;
    try {
        const result = await services.tagRequests.getPendingTagRequests();
        if (result.isSuccess && result.data) {
            pendingRequests.value = result.data;
        } else {
            // Server enforces [Authorize(Roles="Admin")]; a non-admin is rejected here.
            accessDenied.value = true;
        }
    } catch (err) {
        accessDenied.value = true;
    } finally {
        loading.value = false;
    }
};

const formatTagName = (name: string) => {
    return name
        .toLowerCase()
        .replace(/_/g, ' ')
        .replace(/\b\w/g, char => char.toUpperCase());
};

const formatDate = (value: string) => {
    return new Date(value).toLocaleString();
};

const approve = async (request: TagRequestDto) => {
    processingUid.value = request.uid;
    try {
        const result = await services.tagRequests.approveTagRequest(request.uid);
        if (result.isSuccess) {
            notification.success(`Tag "${formatTagName(request.name)}" approved`);
            pendingRequests.value = pendingRequests.value.filter(r => r.uid !== request.uid);
        } else {
            notification.error(result.responseMessage || 'Failed to approve tag request');
        }
    } catch (err) {
        notification.error(err instanceof Error ? err.message : 'Error approving tag request');
    } finally {
        processingUid.value = null;
    }
};

const reject = async (request: TagRequestDto) => {
    processingUid.value = request.uid;
    try {
        const result = await services.tagRequests.rejectTagRequest(request.uid);
        if (result.isSuccess) {
            notification.success(`Tag "${formatTagName(request.name)}" rejected`);
            pendingRequests.value = pendingRequests.value.filter(r => r.uid !== request.uid);
        } else {
            notification.error(result.responseMessage || 'Failed to reject tag request');
        }
    } catch (err) {
        notification.error(err instanceof Error ? err.message : 'Error rejecting tag request');
    } finally {
        processingUid.value = null;
    }
};

onMounted(() => {
    loadPendingRequests();
});
</script>

<template>
    <div class="container py-4">
        <div class="d-flex justify-content-between align-items-center mb-4">
            <h2 class="mb-0">
                <i class="fas fa-tags me-2"></i>
                Tag Requests
            </h2>
            <button
                type="button"
                class="btn btn-outline-secondary btn-sm"
                @click="loadPendingRequests"
                :disabled="loading"
            >
                <i class="fas fa-rotate-right me-1"></i>
                Refresh
            </button>
        </div>

        <div v-if="loading" class="text-center text-muted py-5">
            <span class="spinner-border me-2" role="status"></span>
            Loading pending tag requests...
        </div>

        <div v-else-if="accessDenied" class="alert alert-danger">
            <i class="fas fa-lock me-2"></i>
            You do not have permission to review tag requests. This page is for administrators only.
        </div>

        <div v-else-if="pendingRequests.length === 0" class="alert alert-info">
            <i class="fas fa-check-circle me-2"></i>
            There are no pending tag requests.
        </div>

        <div v-else class="card">
            <div class="list-group list-group-flush">
                <div
                    v-for="request in pendingRequests"
                    :key="request.uid"
                    class="list-group-item d-flex justify-content-between align-items-center flex-wrap gap-2"
                >
                    <div>
                        <span class="badge bg-secondary py-2 px-3 me-2">{{ formatTagName(request.name) }}</span>
                        <small class="text-muted">Requested {{ formatDate(request.createdAt) }}</small>
                    </div>
                    <div class="d-flex gap-2">
                        <button
                            type="button"
                            class="btn btn-success btn-sm"
                            @click="approve(request)"
                            :disabled="processingUid === request.uid"
                        >
                            <span v-if="processingUid === request.uid" class="spinner-border spinner-border-sm me-1" role="status"></span>
                            <i v-else class="fas fa-check me-1"></i>
                            Approve
                        </button>
                        <button
                            type="button"
                            class="btn btn-outline-danger btn-sm"
                            @click="reject(request)"
                            :disabled="processingUid === request.uid"
                        >
                            <i class="fas fa-times me-1"></i>
                            Reject
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
