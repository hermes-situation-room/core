<script setup lang="ts">
import {nextTick, onMounted, onUnmounted, ref} from 'vue';
import {useRoute, useRouter} from 'vue-router';
import {services, sockets} from '../services/api';
import type {ChatBo} from '../types/chat';
import { useAuthStore } from '../stores/auth-store';
import type {CreateMessageDto, MessageBo} from "../types/message.ts";

const route = useRoute();
const router = useRouter();
const authStore = useAuthStore();

const chat = ref<ChatBo | null>(null);
const messages = ref<MessageBo[]>([]);
const newMessage = ref('');
const loading = ref(false);
const loadingMessages = ref(false);
const sending = ref(false);
const currentUserUid = ref<string>('');
const editingMessageId = ref<string | null>(null);
const editingContent = ref('');
const messageInputRef = ref<HTMLInputElement | null>(null);
const errorMessage = ref('');
const otherUserDisplayName = ref<string>('');
const isSocketConnected = ref(false);

const clearError = () => {
    errorMessage.value = '';
};

const showError = (message: string) => {
    errorMessage.value = message;
    setTimeout(clearError, 5000);
};

const loadChat = async () => {
    loading.value = true;
    try {
        const chatId = route.params.id as string;
        currentUserUid.value = authStore.userId.value || '';

        if (!currentUserUid.value) {
            showError('You must be logged in to view chats');
            router.push('/chats');
            return;
        }

        const result = await services.chats.getChatById(chatId);
        if (result.isSuccess && result.data) {
            chat.value = result.data;
            
            // Load display name for other user
            const otherUserUid = getOtherUserUid();
            if (otherUserUid) {
                const displayNameResult = await services.users.getDisplayName(otherUserUid);
                if (displayNameResult.isSuccess && displayNameResult.data) {
                    otherUserDisplayName.value = displayNameResult.data;
                }
            }
            
            await loadMessages(chatId);
            
            // Try to initialize socket connection for real-time updates
            try {
                await sockets.hub.initialize();
                sockets.hub.registerToEvent('ReceiveMessage', handleIncomingMessage);
                sockets.hub.registerToEvent('UpdateMessage', handleMessageUpdate);
                sockets.hub.registerToEvent('DeleteMessage', handleMessageDelete);
                sockets.hub.joinChat(chatId);
                isSocketConnected.value = true;
            } catch (socketError) {
                console.warn('Failed to connect to real-time messaging. Messages will not update automatically:', socketError);
                isSocketConnected.value = false;
            }
        } else {
            if (result.responseCode === 404) {
                showError('Chat not found');
                router.push('/chats');
            }
        }
    } catch (error) {
        showError('Error loading chat');
    } finally {
        loading.value = false;
    }
};

const loadMessages = async (chatId: string) => {
    loadingMessages.value = true;
    try {
        const result = await services.messages.getMessagesByChat(chatId);
        if (result.isSuccess && result.data) {
            messages.value = result.data
                .map(msg => ({
                    uid: msg.uid,
                    chatUid: chatId,
                    senderUid: msg.senderUid || '',
                    content: msg.content || '',
                    timestamp: msg.timestamp || new Date().toISOString()
                }))
                .sort((a, b) => {
                    const dateA = new Date(a.timestamp);
                    const dateB = new Date(b.timestamp);
                    if (isNaN(dateA.getTime()) && isNaN(dateB.getTime())) return 0;
                    if (isNaN(dateA.getTime())) return 1;
                    if (isNaN(dateB.getTime())) return -1;
                    return dateA.getTime() - dateB.getTime();
                });
            setTimeout(() => scrollToBottom(false), 100);
        }
    } catch (error) {
    } finally {
        loadingMessages.value = false;
    }
};

const handleIncomingMessage = (message: MessageBo) => {
    if (!chat.value || message.chatUid !== chat.value.uid) {
        return;
    }

    const completeMessage: MessageBo = {
        uid: message.uid,
        chatUid: chat.value.uid,
        senderUid: message.senderUid || '',
        content: message.content || '',
        timestamp: message.timestamp || new Date().toISOString()
    };

    if (!completeMessage.timestamp || isNaN(new Date(completeMessage.timestamp).getTime())) {
        completeMessage.timestamp = new Date().toISOString();
    }

    const existingIndex = messages.value.findIndex(m => m.uid === completeMessage.uid);

    if (existingIndex !== -1) {
        messages.value[existingIndex] = completeMessage;
    } else {
        if (message.senderUid !== currentUserUid.value) {
            messages.value.push(completeMessage);
            scrollToBottom();
        }
    }
};

const handleMessageUpdate = (message: MessageBo) => {
    if (!chat.value || message.chatUid !== chat.value.uid) {
        return;
    }

    if (message.senderUid === currentUserUid.value) {
        return;
    }

    const messageIndex = messages.value.findIndex(m => m.uid === message.uid);
    if (messageIndex !== -1) {
        messages.value.splice(messageIndex, 1, message);
        if (editingMessageId.value === message.uid) {
            editingMessageId.value = null;
            editingContent.value = '';
        }
    } else {
        messages.value.push(message);
        scrollToBottom();
    }
};

const handleMessageDelete = (messageId: string) => {
    if (!chat.value) {
        return;
    }

    messages.value = messages.value.filter(m => m.uid !== messageId);
    if (editingMessageId.value === messageId) {
        editingMessageId.value = null;
        editingContent.value = '';
    }
};


const sendMessage = async () => {
    if (!newMessage.value.trim() || !chat.value) {
        return;
    }

    sending.value = true;
    const messageContent = newMessage.value.trim();
    newMessage.value = '';
    editingMessageId.value = null;
    editingContent.value = '';

    try {
        const messageData: CreateMessageDto = {
            content: messageContent,
            senderUid: currentUserUid.value,
            chatUid: chat.value.uid
        };

        const result = await services.messages.createMessage(messageData);

        if (result.isSuccess && result.data) {
            const message = (await services.messages.getMessageById(result.data)).data!;
            const exists = messages.value.some(m =>
                m.uid === message.uid ||
                (m.senderUid === message.senderUid &&
                    m.content === message.content &&
                    Math.abs(new Date(m.timestamp).getTime() - new Date(message.timestamp).getTime()) < 5000)
            );
            if (!exists) {
                messages.value.push(message);
            }
            scrollToBottom();
        } else {
            showError('Failed to send message');
            newMessage.value = messageContent;
        }
    } catch (error) {
        showError('Failed to send message');
        newMessage.value = messageContent;
    } finally {
        sending.value = false;
    }
};

const startEditMessage = (message: MessageBo) => {
    if (message.senderUid === currentUserUid.value) {
        editingMessageId.value = message.uid;
        editingContent.value = message.content;
    }
};

const cancelEdit = () => {
    editingMessageId.value = null;
    editingContent.value = '';
};

const saveEdit = async (messageId: string) => {
    if (!editingContent.value.trim()) {
        return;
    }

    const newContent = editingContent.value.trim();

    try {
        const result = await services.messages.updateMessage(messageId, {
            content: newContent
        });

        if (result.isSuccess) {
            const messageIndex = messages.value.findIndex(m => m.uid === messageId);
            if (messageIndex !== -1 && messages.value[messageIndex]) {
                messages.value[messageIndex].content = newContent;
            }
             cancelEdit();
         } else {
             showError('Failed to update message');
         }
     } catch (error) {
         showError('Failed to update message');
     }
};

const deleteMessage = async (messageId: string) => {
    if (!confirm('Are you sure you want to delete this message?')) {
        return;
    }

    try {
        const result = await services.messages.deleteMessage(messageId);
         if (result.isSuccess) {
             messages.value = messages.value.filter(m => m.uid !== messageId);
         } else {
             showError('Failed to delete message');
         }
     } catch (error) {
         showError('Failed to delete message');
     }
};

const scrollToBottom = async (smooth = true) => {
    await nextTick();
    const container = document.getElementById('messages-container');
    if (container) {
        setTimeout(() => {
            if (smooth) {
                container.scrollTo({
                    top: container.scrollHeight,
                    behavior: 'smooth'
                });
            } else {
                container.scrollTop = container.scrollHeight;
            }
        }, 50);
    }
};

const getOtherUserUid = () => {
    if (!chat.value) return '';
    return chat.value.user1Uid === currentUserUid.value ? chat.value.user2Uid : chat.value.user1Uid;
};

const isMyMessage = (message: MessageBo) => {
    return message.senderUid === currentUserUid.value;
};

const shouldShowEditMode = (message: MessageBo) => {
    return editingMessageId.value === message.uid &&
        isMyMessage(message) &&
        currentUserUid.value &&
        message.uid;
};

const formatTime = (timestamp: string) => {
    if (!timestamp || timestamp.trim() === '') {
        return new Date().toLocaleTimeString('en-US', {
            hour: '2-digit',
            minute: '2-digit'
        });
    }

    try {
        let date: Date;
        if (timestamp.includes('T')) {
            date = new Date(timestamp);
        } else if (/^\d+$/.test(timestamp)) {
            date = new Date(parseInt(timestamp));
        } else {
            date = new Date(timestamp);
        }

        if (isNaN(date.getTime())) {
            return new Date().toLocaleTimeString('en-US', {
                hour: '2-digit',
                minute: '2-digit'
            });
        }

        return date.toLocaleTimeString('en-US', {
            hour: '2-digit',
            minute: '2-digit'
        });
    } catch (error) {
        return new Date().toLocaleTimeString('en-US', {
            hour: '2-digit',
            minute: '2-digit'
        });
    }
};

onMounted(() => {
    loadChat();
});

onUnmounted(() => {
    if (chat.value && isSocketConnected.value) {
        try {
            sockets.hub.leaveChat(chat.value.uid);
        } catch (error) {
            console.warn('Error leaving chat:', error);
        }
    }
});
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
                  <button class="btn btn-link text-decoration-none p-0 me-3" @click="router.push('/chats')">
                    ← Back
                  </button>
                  <span class="fw-bold">
                    Chat with: 
                    <a 
                      href="#" 
                      class="text-primary text-decoration-none"
                      @click.prevent="router.push({ path: '/profile', query: { id: getOtherUserUid() } })"
                    >
                      {{ otherUserDisplayName || getOtherUserUid() }}
                    </a>
                  </span>
                </div>
              </div>
            </div>
          </div>

          <div v-if="errorMessage" class="alert alert-danger alert-dismissible fade show mb-3" role="alert">
            {{ errorMessage }}
            <button type="button" class="btn-close" @click="clearError" aria-label="Close"></button>
          </div>

          <div class="card flex-grow-1 d-flex flex-column" style="min-height: 0;">
            <div
                id="messages-container"
                class="card-body overflow-auto flex-grow-1"
                style="max-height: 100%;"
            >
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
                <div
                    v-for="message in messages"
                    :key="message.uid"
                    class="mb-3"
                    :class="{'text-end': isMyMessage(message)}"
                >
                  <div
                      class="d-inline-block rounded position-relative"
                      :class="isMyMessage(message) ? 'bg-primary text-white' : 'bg-light'"
                      style="max-width: 70%;"
                  >
                    <div v-if="shouldShowEditMode(message)" class="px-3 py-2">
                      <input
                          v-model="editingContent"
                          type="text"
                          class="form-control form-control-sm mb-2"
                          @keyup.enter="saveEdit(message.uid)"
                          @keyup.esc="cancelEdit"
                      />
                      <div class="d-flex gap-1">
                        <button
                            class="btn btn-success btn-sm"
                            @click="saveEdit(message.uid)"
                        >
                          Save
                        </button>
                        <button
                            class="btn btn-secondary btn-sm"
                            @click="cancelEdit"
                        >
                          Cancel
                        </button>
                      </div>
                    </div>

                    <div v-else class="px-3 py-2">
                      <div>{{ message.content }}</div>
                      <div class="d-flex justify-content-between align-items-center mt-1">
                        <small
                            class="d-block"
                            :class="isMyMessage(message) ? 'text-white-50' : 'text-muted'"
                        >
                          {{ message.timestamp ? formatTime(message.timestamp) : 'Just now' }}
                        </small>

                        <div v-if="isMyMessage(message)" class="d-flex gap-1 ms-2">
                          <button
                              class="btn btn-sm p-0 border-0 bg-transparent"
                              :class="isMyMessage(message) ? 'text-white-50' : 'text-muted'"
                              @click="startEditMessage(message)"
                              title="Edit message"
                          >
                            <i class="fas fa-edit"></i>
                          </button>
                          <button
                              class="btn btn-sm p-0 border-0 bg-transparent"
                              :class="isMyMessage(message) ? 'text-white-50' : 'text-muted'"
                              @click="deleteMessage(message.uid)"
                              title="Delete message"
                          >
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
                  <input
                      ref="messageInputRef"
                      v-model="newMessage"
                      type="text"
                      class="form-control"
                      placeholder="Type a message..."
                      :disabled="sending"
                      maxlength="1000"
                      autocomplete="off"
                  />
                  <button
                      type="submit"
                      class="btn btn-primary"
                      :disabled="!newMessage.trim() || sending"
                  >
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

