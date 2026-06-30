<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { services } from '../services/api';
import type { ContentFilterRuleDto } from '../services/api/content-filter-api.ts';
import { useNotification } from '../composables/use-notification.ts';

const notification = useNotification();

const rules = ref<ContentFilterRuleDto[]>([]);
const loading = ref(false);
const accessDenied = ref(false);
const processingUid = ref<string | null>(null);

const newPattern = ref('');
const newAction = ref('Redact');
const creating = ref(false);

const editingUid = ref<string | null>(null);
const editPattern = ref('');
const editAction = ref('Redact');

const loadRules = async () => {
    loading.value = true;
    accessDenied.value = false;
    try {
        const result = await services.contentFilter.getAllRules();
        if (result.isSuccess && result.data) {
            rules.value = result.data;
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

const formatDate = (value: string) => {
    return new Date(value).toLocaleString();
};

const createRule = async () => {
    if (!newPattern.value.trim()) {
        notification.error('Pattern must not be empty');
        return;
    }
    creating.value = true;
    try {
        const result = await services.contentFilter.createRule({
            pattern: newPattern.value.trim(),
            action: newAction.value,
        });
        if (result.isSuccess) {
            notification.success(`Rule for "${newPattern.value.trim()}" created`);
            newPattern.value = '';
            newAction.value = 'Redact';
            await loadRules();
        } else {
            notification.error(result.responseMessage || 'Failed to create rule');
        }
    } catch (err) {
        notification.error(err instanceof Error ? err.message : 'Error creating rule');
    } finally {
        creating.value = false;
    }
};

const startEdit = (rule: ContentFilterRuleDto) => {
    editingUid.value = rule.uid;
    editPattern.value = rule.pattern;
    editAction.value = rule.action;
};

const cancelEdit = () => {
    editingUid.value = null;
    editPattern.value = '';
    editAction.value = 'Redact';
};

const saveEdit = async (rule: ContentFilterRuleDto) => {
    if (!editPattern.value.trim()) {
        notification.error('Pattern must not be empty');
        return;
    }
    processingUid.value = rule.uid;
    try {
        const result = await services.contentFilter.updateRule(rule.uid, {
            pattern: editPattern.value.trim(),
            action: editAction.value,
            enabled: rule.enabled,
        });
        if (result.isSuccess) {
            notification.success('Rule updated');
            cancelEdit();
            await loadRules();
        } else {
            notification.error(result.responseMessage || 'Failed to update rule');
        }
    } catch (err) {
        notification.error(err instanceof Error ? err.message : 'Error updating rule');
    } finally {
        processingUid.value = null;
    }
};

const toggleEnabled = async (rule: ContentFilterRuleDto) => {
    processingUid.value = rule.uid;
    try {
        const result = await services.contentFilter.updateRule(rule.uid, {
            pattern: rule.pattern,
            action: rule.action,
            enabled: !rule.enabled,
        });
        if (result.isSuccess) {
            rule.enabled = !rule.enabled;
            notification.success(rule.enabled ? 'Rule enabled' : 'Rule disabled');
        } else {
            notification.error(result.responseMessage || 'Failed to update rule');
        }
    } catch (err) {
        notification.error(err instanceof Error ? err.message : 'Error updating rule');
    } finally {
        processingUid.value = null;
    }
};

const deleteRule = async (rule: ContentFilterRuleDto) => {
    processingUid.value = rule.uid;
    try {
        const result = await services.contentFilter.deleteRule(rule.uid);
        if (result.isSuccess) {
            notification.success(`Rule for "${rule.pattern}" deleted`);
            rules.value = rules.value.filter(r => r.uid !== rule.uid);
        } else {
            notification.error(result.responseMessage || 'Failed to delete rule');
        }
    } catch (err) {
        notification.error(err instanceof Error ? err.message : 'Error deleting rule');
    } finally {
        processingUid.value = null;
    }
};

onMounted(() => {
    loadRules();
});
</script>

<template>
    <div class="container py-4">
        <div class="d-flex justify-content-between align-items-center mb-4">
            <h2 class="mb-0">
                <i class="fas fa-filter me-2"></i>
                Content Filter
            </h2>
            <button
                type="button"
                class="btn btn-outline-secondary btn-sm"
                @click="loadRules"
                :disabled="loading"
            >
                <i class="fas fa-rotate-right me-1"></i>
                Refresh
            </button>
        </div>

        <p class="text-muted">
            Rules apply to public posts and comments only. Private messages are never filtered.
            <strong>Redact</strong> replaces whole-word matches with asterisks; <strong>Block</strong> rejects the content.
        </p>

        <div v-if="loading" class="text-center text-muted py-5">
            <span class="spinner-border me-2" role="status"></span>
            Loading content filter rules...
        </div>

        <div v-else-if="accessDenied" class="alert alert-danger">
            <i class="fas fa-lock me-2"></i>
            You do not have permission to manage content filter rules. This page is for administrators only.
        </div>

        <template v-else>
            <div class="card mb-4">
                <div class="card-body">
                    <h5 class="card-title mb-3">Add Rule</h5>
                    <form class="row g-2 align-items-end" @submit.prevent="createRule">
                        <div class="col-12 col-md-6">
                            <label class="form-label" for="new-pattern">Pattern</label>
                            <input
                                id="new-pattern"
                                v-model="newPattern"
                                type="text"
                                class="form-control"
                                placeholder="e.g. badword"
                                :disabled="creating"
                            />
                        </div>
                        <div class="col-8 col-md-3">
                            <label class="form-label" for="new-action">Action</label>
                            <select id="new-action" v-model="newAction" class="form-select" :disabled="creating">
                                <option value="Redact">Redact</option>
                                <option value="Block">Block</option>
                            </select>
                        </div>
                        <div class="col-4 col-md-3 d-grid">
                            <button type="submit" class="btn btn-primary" :disabled="creating">
                                <span v-if="creating" class="spinner-border spinner-border-sm me-1" role="status"></span>
                                <i v-else class="fas fa-plus me-1"></i>
                                Add
                            </button>
                        </div>
                    </form>
                </div>
            </div>

            <div v-if="rules.length === 0" class="alert alert-info">
                <i class="fas fa-info-circle me-2"></i>
                There are no content filter rules yet.
            </div>

            <div v-else class="card">
                <div class="list-group list-group-flush">
                    <div
                        v-for="rule in rules"
                        :key="rule.uid"
                        class="list-group-item d-flex justify-content-between align-items-center flex-wrap gap-2"
                    >
                        <template v-if="editingUid === rule.uid">
                            <div class="d-flex flex-wrap gap-2 align-items-center flex-grow-1">
                                <input
                                    v-model="editPattern"
                                    type="text"
                                    class="form-control form-control-sm"
                                    style="max-width: 16rem;"
                                />
                                <select v-model="editAction" class="form-select form-select-sm" style="max-width: 9rem;">
                                    <option value="Redact">Redact</option>
                                    <option value="Block">Block</option>
                                </select>
                            </div>
                            <div class="d-flex gap-2">
                                <button
                                    type="button"
                                    class="btn btn-success btn-sm"
                                    @click="saveEdit(rule)"
                                    :disabled="processingUid === rule.uid"
                                >
                                    <span v-if="processingUid === rule.uid" class="spinner-border spinner-border-sm me-1" role="status"></span>
                                    <i v-else class="fas fa-check me-1"></i>
                                    Save
                                </button>
                                <button type="button" class="btn btn-outline-secondary btn-sm" @click="cancelEdit">
                                    <i class="fas fa-times me-1"></i>
                                    Cancel
                                </button>
                            </div>
                        </template>
                        <template v-else>
                            <div>
                                <code class="me-2">{{ rule.pattern }}</code>
                                <span
                                    class="badge me-2"
                                    :class="rule.action === 'Block' ? 'bg-danger' : 'bg-warning text-dark'"
                                >
                                    {{ rule.action }}
                                </span>
                                <span
                                    class="badge me-2"
                                    :class="rule.enabled ? 'bg-success' : 'bg-secondary'"
                                >
                                    {{ rule.enabled ? 'Enabled' : 'Disabled' }}
                                </span>
                                <small class="text-muted">Created {{ formatDate(rule.createdAt) }}</small>
                            </div>
                            <div class="d-flex gap-2">
                                <button
                                    type="button"
                                    class="btn btn-outline-secondary btn-sm"
                                    @click="toggleEnabled(rule)"
                                    :disabled="processingUid === rule.uid"
                                >
                                    <i class="fas me-1" :class="rule.enabled ? 'fa-toggle-off' : 'fa-toggle-on'"></i>
                                    {{ rule.enabled ? 'Disable' : 'Enable' }}
                                </button>
                                <button
                                    type="button"
                                    class="btn btn-outline-primary btn-sm"
                                    @click="startEdit(rule)"
                                    :disabled="processingUid === rule.uid"
                                >
                                    <i class="fas fa-pen me-1"></i>
                                    Edit
                                </button>
                                <button
                                    type="button"
                                    class="btn btn-outline-danger btn-sm"
                                    @click="deleteRule(rule)"
                                    :disabled="processingUid === rule.uid"
                                >
                                    <i class="fas fa-trash me-1"></i>
                                    Delete
                                </button>
                            </div>
                        </template>
                    </div>
                </div>
            </div>
        </template>
    </div>
</template>
