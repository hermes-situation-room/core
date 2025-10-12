<script setup lang="ts">
import {onMounted, onUnmounted, ref, watch, nextTick} from 'vue';
import {useRoute, useRouter} from 'vue-router';
import {services, sockets} from '../services/api';
import type {ChatBo, MessageBo, CreateMessageRequest} from '../types/chat';

const route = useRoute();
const router = useRouter();

const chat = ref<ChatBo | null>(null);
const messages = ref<MessageBo[]>([]);
const newMessage = ref('');
const loading = ref(false);
const loadingMessages = ref(false);
const sending = ref(false);
const currentUserUid = ref<string>('');
const editingMessageId = ref<string | null>(null);
const editingContent = ref('');
const sendError = ref<string>('');
const messageInputRef = ref<HTMLInputElement | null>(null);

const loadChat = async () => {
    loading.value = true;
    try {
        const chatId = route.params.id as string;
        currentUserUid.value = localStorage.getItem('userUid') || '';
        
        if (!currentUserUid.value) {
            console.error('No user UID found in localStorage');
            router.push('/chats');
            return;
        }

        const result = await services.chats.getChatById(chatId);
        if (result.isSuccess && result.data) {
            chat.value = result.data;
            
            // Load messages for this chat
            await loadMessages(chatId);
            
            // Initialize socket connection and join chat
            await sockets.hub.ensureSocketInitialization();
            sockets.hub.joinChat(currentUserUid.value, chatId);
            
            // Register listener for incoming messages
            sockets.hub.registerToEvent('ReceiveMessage', handleIncomingMessage);
        } else {
            console.error('Failed to load chat:', result.responseMessage);
            if (result.responseCode === 404) {
                alert('Chat not found');
                router.push('/chats');
            }
        }
    } catch (error) {
        console.error('Error loading chat:', error);
    } finally {
        loading.value = false;
    }
};

const loadMessages = async (chatId: string) => {
    loadingMessages.value = true;
    try {
        const result = await services.messages.getMessagesByChat(chatId);
        if (result.isSuccess && result.data) {
            // Ensure all messages have required fields and sort by timestamp
            messages.value = result.data
                .map(msg => ({
                    uid: msg.uid || msg.id || crypto.randomUUID(),
                    chatUid: chatId,
                    senderUid: msg.senderUid || '',
                    content: msg.content || '',
                    timestamp: msg.timestamp || new Date().toISOString()
                }))
                .sort((a, b) => {
                    const dateA = new Date(a.timestamp);
                    const dateB = new Date(b.timestamp);
                    
                    // If either date is invalid, put it at the end
                    if (isNaN(dateA.getTime()) && isNaN(dateB.getTime())) return 0;
                    if (isNaN(dateA.getTime())) return 1;
                    if (isNaN(dateB.getTime())) return -1;
                    
                    return dateA.getTime() - dateB.getTime();
                });
            
            // Scroll to bottom after messages are loaded
            setTimeout(() => scrollToBottom(false), 100);
        } else {
            console.error('Failed to load messages:', result.responseMessage);
        }
    } catch (error) {
        console.error('Error loading messages:', error);
    } finally {
        loadingMessages.value = false;
    }
};

const handleIncomingMessage = (message: MessageBo) => {
    // Don't add our own messages from WebSocket - we already added them via API
    if (message.senderUid === currentUserUid.value) {
        return;
    }
    
    // Only process messages for the current chat
    if (!chat.value || message.chatUid !== chat.value.uid) {
        return;
    }
    
    // Ensure message has all required fields
    const completeMessage: MessageBo = {
        uid: message.uid || message.id || `ws-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`,
        chatUid: chat.value.uid,
        senderUid: message.senderUid || '',
        content: message.content || '',
        timestamp: message.timestamp || new Date().toISOString()
    };
    
    // Validate and fix timestamp
    if (!completeMessage.timestamp || isNaN(new Date(completeMessage.timestamp).getTime())) {
        completeMessage.timestamp = new Date().toISOString();
    }
    
    // Check if message already exists (avoid duplicates)
    const exists = messages.value.some(m => m.uid === completeMessage.uid);
    
    console.log('WebSocket message exists check:', exists);
    console.log('Current messages count before WebSocket:', messages.value.length);
    
    if (!exists) {
        messages.value.push(completeMessage);
        scrollToBottom();
    }
};

const sendMessage = async () => {
    if (!newMessage.value.trim() || !chat.value) {
        return;
    }

    sending.value = true;
    const messageContent = newMessage.value.trim();
    newMessage.value = ''; // Clear input immediately for better UX
    
    // Ensure no message is in edit mode
    editingMessageId.value = null;
    editingContent.value = '';

    try {
        const messageData: CreateMessageRequest = {
            content: messageContent,
            senderUid: currentUserUid.value,
            chatUid: chat.value.uid
        };

        const result = await services.messages.createMessage(messageData);
        
        if (result.isSuccess && result.data) {
            // Simple approach: just add the message with the data we have
            const message: MessageBo = {
                uid: result.data.uid || result.data.id || crypto.randomUUID(),
                chatUid: chat.value.uid,
                senderUid: currentUserUid.value,
                content: messageContent,
                timestamp: result.data.timestamp || new Date().toISOString()
            };

            // Check if message already exists (from socket)
            const exists = messages.value.some(m => m.uid === message.uid);
            if (!exists) {
                messages.value.push(message);
            }
            scrollToBottom();
        } else {
            console.error('Failed to send message:', result.responseMessage);
            alert('Failed to send message');
            newMessage.value = messageContent; // Restore message on error
        }
    } catch (error) {
        console.error('Error sending message:', error);
        alert('Failed to send message');
        newMessage.value = messageContent; // Restore message on error
    } finally {
        sending.value = false;
    }
};

const startEditMessage = (message: MessageBo) => {
    // Only allow editing own messages
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

    try {
        const result = await services.messages.updateMessage(messageId, {
            content: editingContent.value.trim()
        });

        if (result.isSuccess) {
            // Update message in local list
            const messageIndex = messages.value.findIndex(m => m.uid === messageId);
            if (messageIndex !== -1) {
                messages.value[messageIndex].content = editingContent.value.trim();
            }
            cancelEdit();
        } else {
            console.error('Failed to update message:', result.responseMessage);
            alert('Failed to update message');
        }
    } catch (error) {
        console.error('Error updating message:', error);
        alert('Failed to update message');
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
            console.error('Failed to delete message:', result.responseMessage);
            alert('Failed to delete message');
        }
    } catch (error) {
        console.error('Error deleting message:', error);
        alert('Failed to delete message');
    }
};

const sortMessages = () => {
    messages.value.sort((a, b) => {
        const dateA = new Date(a.timestamp);
        const dateB = new Date(b.timestamp);
        
        if (isNaN(dateA.getTime()) && isNaN(dateB.getTime())) return 0;
        if (isNaN(dateA.getTime())) return 1;
        if (isNaN(dateB.getTime())) return -1;
        
        return dateA.getTime() - dateB.getTime();
    });
};

const scrollToBottom = async (smooth = true) => {
    await nextTick();
    const container = document.getElementById('messages-container');
    if (container) {
        // Use a slight delay to ensure DOM is fully rendered
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
        return 'Just now';
    }
    
    try {
        // Handle different timestamp formats
        let date: Date;
        
        // Try parsing as ISO string first
        if (timestamp.includes('T') && timestamp.includes('Z')) {
            date = new Date(timestamp);
        }
        // Try parsing as Unix timestamp (if it's a number)
        else if (/^\d+$/.test(timestamp)) {
            date = new Date(parseInt(timestamp));
        }
        // Try parsing as regular date string
        else {
            date = new Date(timestamp);
        }
        
        // Check if the date is valid
        if (isNaN(date.getTime())) {
            return 'Just now';
        }
        
        // Check if date is in the future (more than 1 hour ahead) - might be server time issue
        const now = new Date();
        const diffMs = date.getTime() - now.getTime();
        
        if (diffMs > 3600000) { // More than 1 hour in future
            return 'Just now';
        }
        
        // Check if date is too old (more than 1 year ago) - might be invalid
        const oneYearAgo = new Date(now.getFullYear() - 1, now.getMonth(), now.getDate());
        if (date < oneYearAgo) {
            return 'Just now';
        }
        
        return date.toLocaleTimeString('en-US', {
            hour: '2-digit',
            minute: '2-digit'
        });
    } catch (error) {
        return 'Just now';
    }
};

// Watch for changes in newMessage to clear errors
watch(newMessage, () => {
    if (sendError.value) {
        sendError.value = '';
    }
});

// Debug watcher for editing state
watch(editingMessageId, (newVal, oldVal) => {
    console.log('Editing state changed:', { oldVal, newVal, messageCount: messages.value.length });
});

onMounted(() => {
    loadChat();
});

onUnmounted(() => {
    // Leave chat room when component is unmounted
    if (chat.value) {
        sockets.hub.leaveChat(chat.value.uid);
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
                    <!-- Chat Header -->
                    <div class="card mb-3">
                        <div class="card-body py-3">
                            <div class="d-flex justify-content-between align-items-center">
                                <div>
                                    <button class="btn btn-link text-decoration-none p-0 me-3" @click="router.push('/chats')">
                                        ← Back
                                    </button>
                                    <span class="fw-bold">Chat with User: {{ getOtherUserUid() }}</span>
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- Messages Container -->
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

                            <!-- Debug info -->
                            <div class="alert alert-info small mb-3">
                                <strong>Debug:</strong> Messages count: {{ messages.length }} | 
                                Chat ID: {{ chat?.uid }} | 
                                User ID: {{ currentUserUid }}
                                <br>
                                <strong>Last 3 messages:</strong>
                                <div v-for="(msg, index) in messages.slice(-3)" :key="'debug-' + msg.uid" class="small">
                                    {{ index + 1 }}. {{ msg.content }} ({{ msg.senderUid?.substring(0, 8) }}...)
                                </div>
                            </div>

                            <div v-if="messages.length === 0" class="text-center text-muted py-5">
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
                                        <!-- Editing mode -->
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

                                        <!-- Display mode -->
                                        <div v-else class="px-3 py-2">
                                            <div>{{ message.content }}</div>
                                            <div class="d-flex justify-content-between align-items-center mt-1">
                                                <small 
                                                    class="d-block"
                                                    :class="isMyMessage(message) ? 'text-white-50' : 'text-muted'"
                                                >
                                                    {{ message.timestamp ? formatTime(message.timestamp) : 'Just now' }}
                                                </small>
                                                
                                                <!-- Edit/Delete buttons for own messages -->
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

                        <!-- Message Input -->
                        <div class="card-footer bg-white border-top">
                            <div v-if="sendError" class="alert alert-danger alert-dismissible fade show mb-2 py-2" role="alert">
                                <small>{{ sendError }}</small>
                                <button type="button" class="btn-close py-2" @click="sendError = ''" aria-label="Close"></button>
                            </div>
                            <form @submit.prevent="sendMessage" class="d-flex flex-column gap-2">
                                <div class="d-flex gap-2">
                                    <input 
                                        ref="messageInputRef"
                                        v-model="newMessage"
                                        type="text"
                                        class="form-control"
                                        :class="{'is-invalid': sendError}"
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

