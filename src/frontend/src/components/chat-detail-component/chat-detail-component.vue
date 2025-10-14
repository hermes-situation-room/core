<script setup lang="ts">
import { useRouter } from 'vue-router';
import { useAuthStore } from '../../stores/auth-store';
import { useChat } from './use-chat';
import { useMessages } from './use-messages';

const authStore = useAuthStore();
const router = useRouter();
const currentUserUid = authStore.userId;

const { chat, messages, loading, loadingMessages, errorMessage, clearError, getOtherUserUid } = useChat();
const { newMessage, sending, editingContent, sendMessage, startEditMessage, saveEdit, deleteMessage, isMyMessage, shouldShowEditMode, cancelEdit } =
    useMessages(messages, currentUserUid, chat);

const formatTime = (timestamp: string): string => {
    const date = new Date(timestamp);
    return date.toLocaleString('en-US', {
        hour: '2-digit',
        minute: '2-digit',
        hour12: true,
        month: 'short',
        day: 'numeric',
    });
};
</script>

<template>
    <div class="container-fluid py-4" style="height: calc(100vh - 80px);">
        <div class="row justify-content-center h-100">
            <div class="col-12 col-lg-8 d-flex flex-column h-100">
                <div v-if="loading" class="d-flex justify-content-center align-items-center flex-grow-1">
                    <div class="text-center">
                        <div class="spinner-border text-primary mb-3" role="status" style="width: 3rem; height: 3rem;">
                            <span class="visually-hidden">Loading...</span>
                        </div>
                        <div class="text-muted">Loading chat...</div>
                    </div>
                </div>

                <template v-else-if="chat">
                    <div class="card mb-3">
                        <div class="card-body py-3">
                            <div class="d-flex justify-content-between align-items-center">
                                <div>
                                    <button class="btn btn-link text-decoration-none p-0 me-3"
                                        @click="router.push('/chats')">
                                        ← Back
                                    </button>
                                    <span class="fw-bold">Chat with User: {{ getOtherUserUid() }}</span>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div v-if="errorMessage" class="alert alert-danger alert-dismissible fade show mb-3" role="alert">
                        {{ errorMessage }}
                        <button type="button" class="btn-close" @click="clearError" aria-label="Close"></button>
                    </div>

                    <div class="card flex-grow-1 d-flex flex-column" style="min-height: 0;">
                        <div id="messages-container" class="card-body overflow-auto flex-grow-1"
                            style="max-height: 100%;">
                            <div v-if="loadingMessages" class="text-center py-5">
                                <div class="spinner-border text-primary" role="status">
                                    <span class="visually-hidden">Loading messages...</span>
                                </div>
                            </div>

                            <div v-if="!loadingMessages && messages.length === 0" class="text-center text-muted py-5">
                                <i class="fas fa-comments fa-3x mb-3"></i>
                                <p>No messages yet. Start the conversation!</p>
                            </div>

                            <div v-else>
                                <div v-for="message in messages" :key="message.uid" class="mb-3"
                                    :class="{ 'text-end': isMyMessage(message) }">
                                    <div class="d-inline-block rounded position-relative"
                                        :class="isMyMessage(message) ? 'bg-primary text-white' : 'bg-light'"
                                        style="max-width: 70%;">
                                        <div v-if="shouldShowEditMode(message)" class="px-3 py-2">
                                            <input v-model="editingContent" type="text"
                                                class="form-control form-control-sm mb-2"
                                                @keyup.enter="saveEdit(message.uid)" @keyup.esc="cancelEdit" />
                                            <div class="d-flex gap-1">
                                                <button class="btn btn-success btn-sm" @click="saveEdit(message.uid)">
                                                    Save
                                                </button>
                                                <button class="btn btn-secondary btn-sm" @click="cancelEdit">
                                                    Cancel
                                                </button>
                                            </div>
                                        </div>

                                        <div v-else class="px-3 py-2">
                                            <div>{{ message.content }}</div>
                                            <div class="d-flex justify-content-between align-items-center mt-1">
                                                <small class="d-block"
                                                    :class="isMyMessage(message) ? 'text-white-50' : 'text-muted'">
                                                    {{ message.timestamp ? formatTime(message.timestamp) : 'Just now' }}
                                                </small>

                                                <div v-if="isMyMessage(message)" class="d-flex gap-1 ms-2">
                                                    <button class="btn btn-sm p-0 border-0 bg-transparent"
                                                        :class="isMyMessage(message) ? 'text-white-50' : 'text-muted'"
                                                        @click="startEditMessage(message)" title="Edit message">
                                                        <i class="fas fa-edit"></i>
                                                    </button>
                                                    <button class="btn btn-sm p-0 border-0 bg-transparent"
                                                        :class="isMyMessage(message) ? 'text-white-50' : 'text-muted'"
                                                        @click="deleteMessage(message.uid)" title="Delete message">
                                                        <i class="fas fa-trash"></i>
                                                    </button>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div class="card-footer bg-white border-top">
                            <form @submit.prevent="sendMessage" class="d-flex flex-column gap-2">
                                <div class="d-flex gap-2">
                                    <input ref="messageInputRef" v-model="newMessage" type="text" class="form-control"
                                        placeholder="Type a message..." :disabled="sending" maxlength="1000"
                                        autocomplete="off" />
                                    <button type="submit" class="btn btn-primary"
                                        :disabled="!newMessage.trim() || sending">
                                        <span v-if="sending" class="spinner-border spinner-border-sm me-1"></span>
                                        Send
                                    </button>
                                </div>
                                <small v-if="newMessage.length > 800" class="text-muted text-end">
                                    {{ newMessage.length }}/1000 characters
                                </small>
                            </form>
                        </div>
                    </div>
                </template>

                <div v-else class="text-center py-5">
                    <i class="fas fa-exclamation-circle fa-3x text-danger mb-3"></i>
                    <h5 class="text-muted">Chat not found</h5>
                    <button class="btn btn-primary mt-3" @click="router.push('/chats')">
                        Back to Chats
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>
