<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { services } from '../services/api';
import type { ReportDto } from '../services/api/reports-api.ts';
import { useAuthStore } from '../stores/auth-store';
import { useNotification } from '../composables/use-notification.ts';
import { useConfirm } from '../composables/use-confirm.ts';

const router = useRouter();
const authStore = useAuthStore();
const notification = useNotification();
const { confirm } = useConfirm();

const reports = ref<ReportDto[]>([]);
const loading = ref(false);
const accessDenied = ref(false);
const processingUid = ref<string | null>(null);

// 'Pending' is the default working queue; 'All' lists every report regardless of status.
const statusFilter = ref<string>('Pending');
const statusOptions = ['Pending', 'Reviewed', 'Resolved', 'Rejected', 'All'];

// Per-report admin note draft, keyed by report uid.
const adminNotes = ref<Record<string, string>>({});

const loadReports = async () => {
    loading.value = true;
    accessDenied.value = false;
    try {
        const result = statusFilter.value === 'All'
            ? await services.reports.getAllReports()
            : await services.reports.getByStatus(statusFilter.value);

        if (result.isSuccess && result.data) {
            reports.value = result.data;
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

const changeFilter = (status: string) => {
    statusFilter.value = status;
    loadReports();
};

const formatDate = (value: string | null) => {
    if (!value) {
        return '—';
    }
    return new Date(value).toLocaleString();
};

const statusBadgeClass = (status: string) => {
    switch (status) {
        case 'Pending':
            return 'bg-warning text-dark';
        case 'Reviewed':
            return 'bg-info text-dark';
        case 'Resolved':
            return 'bg-success';
        case 'Rejected':
            return 'bg-secondary';
        default:
            return 'bg-secondary';
    }
};

const targetIcon = (targetType: string) => {
    switch (targetType) {
        case 'Post':
            return 'fa-newspaper';
        case 'Comment':
            return 'fa-comment';
        case 'User':
            return 'fa-user';
        default:
            return 'fa-flag';
    }
};

// Only posts have a directly navigable detail route. Comments live on a post we don't
// know from the report alone, and users open via the profile page.
const canNavigateToTarget = (report: ReportDto) => {
    return report.targetType === 'Post' || report.targetType === 'User';
};

const navigateToTarget = (report: ReportDto) => {
    if (report.targetType === 'Post') {
        router.push(`/post/${report.targetUid}`);
    } else if (report.targetType === 'User') {
        router.push({ path: '/profile', query: { id: report.targetUid } });
    }
};

const reviewReport = async (report: ReportDto, status: string) => {
    if (!authStore.userId.value) {
        notification.error('You must be logged in as an admin to review reports');
        return;
    }

    processingUid.value = report.uid;
    try {
        const result = await services.reports.reviewReport(report.uid, {
            reviewedByUid: authStore.userId.value,
            status,
            adminNotes: adminNotes.value[report.uid]?.trim() || report.adminNotes || '',
        });
        if (result.isSuccess) {
            notification.success(`Report marked as ${status}`);
            delete adminNotes.value[report.uid];
            await loadReports();
        } else {
            notification.error(result.responseMessage || 'Failed to update report');
        }
    } catch (err) {
        notification.error(err instanceof Error ? err.message : 'Error updating report');
    } finally {
        processingUid.value = null;
    }
};

const deleteReport = async (report: ReportDto) => {
    if (!(await confirm({
        title: 'Delete report',
        message: 'Are you sure you want to delete this report?',
        confirmLabel: 'Delete',
        danger: true,
    }))) {
        return;
    }

    processingUid.value = report.uid;
    try {
        const result = await services.reports.deleteReport(report.uid);
        if (result.isSuccess) {
            notification.deleted('Report deleted');
            reports.value = reports.value.filter(r => r.uid !== report.uid);
        } else {
            notification.error(result.responseMessage || 'Failed to delete report');
        }
    } catch (err) {
        notification.error(err instanceof Error ? err.message : 'Error deleting report');
    } finally {
        processingUid.value = null;
    }
};

onMounted(() => {
    loadReports();
});
</script>

<template>
    <div class="container py-4">
        <div class="d-flex justify-content-between align-items-center mb-4">
            <h2 class="mb-0">
                <i class="fas fa-flag me-2"></i>
                Moderation
            </h2>
            <button
                type="button"
                class="btn btn-outline-secondary btn-sm"
                @click="loadReports"
                :disabled="loading"
            >
                <i class="fas fa-rotate-right me-1"></i>
                Refresh
            </button>
        </div>

        <p class="text-muted">
            Review reports submitted by users against posts, comments and other users.
            Use the actions to resolve or reject a report, or delete it entirely.
        </p>

        <div class="btn-group mb-4 flex-wrap" role="group" aria-label="Filter by status">
            <button
                v-for="status in statusOptions"
                :key="status"
                type="button"
                class="btn btn-sm"
                :class="statusFilter === status ? 'btn-primary' : 'btn-outline-primary'"
                @click="changeFilter(status)"
                :disabled="loading"
            >
                {{ status }}
            </button>
        </div>

        <div v-if="loading" class="text-center text-muted py-5">
            <span class="spinner-border me-2" role="status"></span>
            Loading reports...
        </div>

        <div v-else-if="accessDenied" class="alert alert-danger">
            <i class="fas fa-lock me-2"></i>
            You do not have permission to view reports. This page is for administrators only.
        </div>

        <template v-else>
            <div v-if="reports.length === 0" class="alert alert-info">
                <i class="fas fa-info-circle me-2"></i>
                There are no {{ statusFilter === 'All' ? '' : statusFilter.toLowerCase() + ' ' }}reports.
            </div>

            <div v-else class="d-flex flex-column gap-3">
                <div
                    v-for="report in reports"
                    :key="report.uid"
                    class="card"
                >
                    <div class="card-body">
                        <div class="d-flex justify-content-between align-items-start flex-wrap gap-2 mb-2">
                            <div>
                                <span class="badge bg-dark me-2">
                                    <i class="fas me-1" :class="targetIcon(report.targetType)"></i>
                                    {{ report.targetType }}
                                </span>
                                <span class="badge me-2" :class="statusBadgeClass(report.status)">
                                    {{ report.status }}
                                </span>
                                <small class="text-muted">{{ formatDate(report.timestamp) }}</small>
                            </div>
                            <button
                                v-if="canNavigateToTarget(report)"
                                type="button"
                                class="btn btn-outline-secondary btn-sm"
                                @click="navigateToTarget(report)"
                            >
                                <i class="fas fa-arrow-up-right-from-square me-1"></i>
                                View {{ report.targetType }}
                            </button>
                        </div>

                        <div class="mb-2">
                            <div class="text-muted small">Reason</div>
                            <div class="fw-medium text-break">{{ report.reason }}</div>
                        </div>

                        <div class="row g-2 mb-2">
                            <div class="col-12 col-md-6">
                                <div class="text-muted small">Target ID</div>
                                <code class="small text-break">{{ report.targetUid }}</code>
                            </div>
                            <div class="col-12 col-md-6">
                                <div class="text-muted small">Reporter</div>
                                <code class="small text-break">{{ report.reporterUid }}</code>
                            </div>
                        </div>

                        <div v-if="report.reviewedByUid" class="mb-2">
                            <div class="text-muted small">
                                Reviewed {{ formatDate(report.reviewedAt) }} by
                                <code class="small">{{ report.reviewedByUid }}</code>
                            </div>
                            <div v-if="report.adminNotes" class="fst-italic small">"{{ report.adminNotes }}"</div>
                        </div>

                        <div class="mb-3">
                            <label class="form-label small text-muted" :for="`notes-${report.uid}`">Admin notes (optional)</label>
                            <input
                                :id="`notes-${report.uid}`"
                                v-model="adminNotes[report.uid]"
                                type="text"
                                class="form-control form-control-sm"
                                :placeholder="report.adminNotes || 'Add a note about this decision...'"
                                :disabled="processingUid === report.uid"
                            />
                        </div>

                        <div class="d-flex flex-wrap gap-2">
                            <button
                                type="button"
                                class="btn btn-info btn-sm"
                                @click="reviewReport(report, 'Reviewed')"
                                :disabled="processingUid === report.uid"
                            >
                                <i class="fas fa-eye me-1"></i>
                                Mark Reviewed
                            </button>
                            <button
                                type="button"
                                class="btn btn-success btn-sm"
                                @click="reviewReport(report, 'Resolved')"
                                :disabled="processingUid === report.uid"
                            >
                                <i class="fas fa-check me-1"></i>
                                Resolve
                            </button>
                            <button
                                type="button"
                                class="btn btn-secondary btn-sm"
                                @click="reviewReport(report, 'Rejected')"
                                :disabled="processingUid === report.uid"
                            >
                                <i class="fas fa-ban me-1"></i>
                                Reject
                            </button>
                            <button
                                type="button"
                                class="btn btn-outline-danger btn-sm ms-auto"
                                @click="deleteReport(report)"
                                :disabled="processingUid === report.uid"
                            >
                                <span v-if="processingUid === report.uid" class="spinner-border spinner-border-sm me-1" role="status"></span>
                                <i v-else class="fas fa-trash me-1"></i>
                                Delete
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </template>
    </div>
</template>
