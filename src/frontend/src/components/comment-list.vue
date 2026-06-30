<script setup lang="ts">
import type { CommentBo } from '../types/comment';
import ProfileIconDisplay from './profile-icon-display.vue';

interface CommentWithReplies extends CommentBo {
    replies: CommentBo[];
}

interface Props {
    comments: CommentWithReplies[];
    currentUserUid: string;
    showReplies: Set<string>;
    replyingToComment: string | null;
    editCommentContent: string;
    replyContent: string;
}

defineProps<Props>();

const emit = defineEmits<{
    (event: 'update:editCommentContent', value: string): void;
    (event: 'update:replyContent', value: string): void;
    (event: 'edit-toggle', comment: CommentBo): void;
    (event: 'delete', comment: CommentBo): void;
    (event: 'update', comment: CommentBo): void;
    (event: 'report', comment: CommentBo): void;
    (event: 'start-reply', commentUid: string): void;
    (event: 'cancel-reply'): void;
    (event: 'post-reply', commentUid: string): void;
    (event: 'toggle-replies', commentUid: string): void;
    (event: 'navigate-profile', creatorUid: string): void;
}>();

const formatDate = (dateString: string) => {
    const date = new Date(dateString + 'Z') || new Date().toISOString();
    return date.toLocaleDateString(navigator.language || 'en-US', {
        day: 'numeric',
        month: 'long',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
};
</script>

<template>
    <div class="comment-list d-flex flex-column">
        <div v-for="comment in comments" :key="comment.uid" class="comment border mb-2 p-3 rounded" style="min-height: 50px;">
            <!-- Main Comment -->
            <div v-if="!comment.inEdit" class="d-flex align-items-start gap-2 h-100">
                <div class="d-flex align-items-center" style="min-height: 40px;">
                    <a
                        href="#"
                        class="text-decoration-none"
                        @click.prevent="emit('navigate-profile', comment.creatorUid)"
                        title="View profile"
                    >
                        <ProfileIconDisplay
                            :icon="comment?.profileIcon"
                            :color="comment?.profileIconColor"
                            size="lg"
                            class="bg-white"
                        />
                    </a>
                </div>

                <div class="flex-grow-1 d-flex flex-column justify-content-between h-100">
                    <div class="d-flex justify-content-between align-items-center mb-1">
                        <a
                            href="#"
                            class="text-primary text-decoration-none fw-bold"
                            @click.prevent="emit('navigate-profile', comment.creatorUid)"
                        >
                            {{ comment.displayName }}
                        </a>
                        <div class="d-flex align-items-center gap-2">
                            <small class="text-muted">{{ formatDate(comment.timestamp) }}</small>
                            <div v-if="comment.creatorUid == currentUserUid && !comment.inEdit" class="d-flex gap-2">
                                <i class="fas fa-edit text-muted" style="cursor: pointer;" @click="emit('edit-toggle', comment)" title="Edit comment"></i>
                                <i class="fas fa-trash text-muted" style="cursor: pointer;" @click="emit('delete', comment)" title="Delete comment"></i>
                            </div>
                            <i
                                v-else-if="currentUserUid && comment.creatorUid != currentUserUid"
                                class="fas fa-flag text-muted"
                                style="cursor: pointer;"
                                @click="emit('report', comment)"
                                title="Report comment"
                            ></i>
                        </div>
                    </div>

                    <div class="text-break text-dark flex-grow-1">
                        {{ comment.content }}
                    </div>

                    <!-- Comment Actions -->
                    <div class="d-flex align-items-center gap-3 mt-2">
                        <button
                            v-if="currentUserUid"
                            class="btn btn-sm btn-outline-secondary"
                            @click="emit('start-reply', comment.uid)"
                        >
                            <i class="fas fa-reply me-1"></i>
                            Reply
                        </button>

                        <button
                            v-if="comment.replies && comment.replies.length > 0"
                            class="btn btn-sm btn-link text-muted p-0"
                            @click="emit('toggle-replies', comment.uid)"
                        >
                            <i :class="showReplies.has(comment.uid) ? 'fas fa-chevron-down' : 'fas fa-chevron-right'" class="me-1"></i>
                            {{ comment.replies.length }} {{ comment.replies.length === 1 ? 'reply' : 'replies' }}
                        </button>
                    </div>
                </div>
            </div>

            <!-- Edit Comment Form -->
            <div v-else class="d-flex justify-content-between align-items-center gap-2">
                <input
                    :value="editCommentContent"
                    @input="emit('update:editCommentContent', ($event.target as HTMLInputElement).value)"
                    type="text"
                    maxlength="255"
                    spellcheck="true"
                    lang="en"
                    class="rounded w-100 form-control"
                >

                <button class="btn btn-primary btn-sm d-flex align-items-center" @click="emit('update', comment)">
                    <i class="fas fa-check me-1"></i>
                    Confirm
                </button>
                <button class="btn btn-secondary btn-sm d-flex align-items-center" @click="emit('edit-toggle', comment)">
                    <i class="fas fa-times me-1"></i>
                    Cancel
                </button>
            </div>

            <!-- Reply Form -->
            <div v-if="replyingToComment === comment.uid" class="mt-3 p-3 bg-light rounded">
                <div class="d-flex gap-2">
                    <input
                        :value="replyContent"
                        @input="emit('update:replyContent', ($event.target as HTMLInputElement).value)"
                        type="text"
                        maxlength="255"
                        spellcheck="true"
                        lang="en"
                        class="form-control"
                        placeholder="Write a reply..."
                    >
                    <button
                        class="btn btn-primary btn-sm"
                        @click="emit('post-reply', comment.uid)"
                    >
                        <i class="fas fa-paper-plane me-1"></i>
                        Reply
                    </button>
                    <button
                        class="btn btn-secondary btn-sm"
                        @click="emit('cancel-reply')"
                    >
                        <i class="fas fa-times me-1"></i>
                        Cancel
                    </button>
                </div>
            </div>

            <!-- Replies Section -->
            <div v-if="comment.replies && comment.replies.length > 0 && showReplies.has(comment.uid)" class="mt-3">
                <div class="replies-container ms-4">
                    <div v-for="reply in comment.replies" :key="reply.uid" class="reply mb-2 p-2 bg-light rounded">
                        <div class="d-flex align-items-start gap-2">
                            <div class="d-flex align-items-center" style="min-height: 30px;">
                                <a
                                    href="#"
                                    class="text-decoration-none"
                                    @click.prevent="emit('navigate-profile', reply.creatorUid)"
                                    title="View profile"
                                >
                                    <ProfileIconDisplay
                                        :icon="reply?.profileIcon"
                                        :color="reply?.profileIconColor"
                                        size="sm"
                                        class="bg-white"
                                    />
                                </a>
                            </div>

                            <div class="flex-grow-1">
                                <div class="d-flex justify-content-between align-items-center mb-1">
                                    <a
                                        href="#"
                                        class="text-primary text-decoration-none fw-bold small"
                                        @click.prevent="emit('navigate-profile', reply.creatorUid)"
                                    >
                                        {{ reply.displayName }}
                                    </a>
                                    <div class="d-flex align-items-center gap-2">
                                        <small class="text-muted">{{ formatDate(reply.timestamp) }}</small>
                                        <div v-if="reply.creatorUid == currentUserUid" class="d-flex gap-2">
                                            <i class="fas fa-edit text-muted small" style="cursor: pointer;" @click="emit('edit-toggle', reply)" title="Edit reply"></i>
                                            <i class="fas fa-trash text-muted small" style="cursor: pointer;" @click="emit('delete', reply)" title="Delete reply"></i>
                                        </div>
                                        <i
                                            v-else-if="currentUserUid && reply.creatorUid != currentUserUid"
                                            class="fas fa-flag text-muted small"
                                            style="cursor: pointer;"
                                            @click="emit('report', reply)"
                                            title="Report reply"
                                        ></i>
                                    </div>
                                </div>

                                <div class="text-break text-dark small">
                                    {{ reply.content }}
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
.replies-container {
    border-left: 2px solid var(--border-color);
    padding-left: 1rem;
}

.reply {
    background-color: var(--bg-secondary);
    border-radius: 0.375rem;
    margin-bottom: 0.5rem;
}

.reply:hover {
    background-color: var(--bg-tertiary);
}
</style>
