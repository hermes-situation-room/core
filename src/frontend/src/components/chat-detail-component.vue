<script setup lang="ts">
import {nextTick, onMounted, onUnmounted, ref, watch} from 'vue';
import {useRouter} from 'vue-router';
import {services, sockets} from '../services/api';
import type {ChatBo} from '../types/chat';
import type {UserProfileBo} from '../types/user';
import {useAuthStore} from '../stores/auth-store';
import type {CreateMessageDto, MessageBo} from "../types/message.ts";
import {useNotification} from '../composables/use-notification.ts';
import {useConfirm} from '../composables/use-confirm.ts';
import ProfileIconDisplay from './profile-icon-display.vue';
import AttachmentPicker from './attachments/attachment-picker.vue';
import AttachmentList from './attachments/attachment-list.vue';
import type { AttachmentDto } from '../services/api/attachments-api';

const router = useRouter();
const authStore = useAuthStore();
const notification = useNotification();
const {confirm} = useConfirm();

const props = defineProps({
    chatId: {
        type: String,
        required: false,
    }
});

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
const isSocketConnected = ref(false);
const otherUserDisplayName = ref<string>('');
const otherUserProfile = ref<UserProfileBo | null>(null);
const messageSenderProfiles = ref<Map<string, UserProfileBo>>(new Map());

// Attachments uploaded for the message currently being composed (linked on send via attachmentIds).
const pendingAttachments = ref<AttachmentDto[]>([]);
// messageUid -> its attachments, fetched lazily through the authorized by-message endpoint.
const messageAttachments = ref<Map<string, AttachmentDto[]>>(new Map());
const fetchingAttachmentsFor = ref<Set<string>>(new Set());

const noChat = ref<boolean>(true);

// Composer options for ephemeral messages.
const disappearAfterSeconds = ref<number>(0); // 0 = off
const isViewOnce = ref<boolean>(false);
// Maps a view-once message uid -> the plaintext revealed via the consume GET, so we can render it
// exactly once this session. Once revealed it stays in this map (cannot be re-fetched) until the
// message leaves the list, so re-renders keep showing the same revealed content.
const revealedViewOnceContent = ref<Map<string, string>>(new Map());
// View-once messages the current user is currently fetching/has fetched (the consume GET is fired
// once; subsequent taps are ignored even before the response arrives).
const revealingViewOnceIds = ref<Set<string>>(new Set());

const disappearOptions = [
    {label: 'Off', value: 0},
    {label: '1 hour', value: 3600},
    {label: '1 day', value: 86400},
];

const isExpired = (message: MessageBo): boolean => {
    if (!message.expiresAt) return false;
    const expires = new Date(message.expiresAt);
    return !isNaN(expires.getTime()) && expires.getTime() <= Date.now();
};

const normalizeMessage = (message: MessageBo, fallbackChatUid: string): MessageBo => ({
    uid: message.uid,
    chatUid: message.chatUid || fallbackChatUid,
    senderUid: message.senderUid || '',
    content: message.content || '',
    timestamp: message.timestamp || new Date().toISOString(),
    expiresAt: message.expiresAt ?? null,
    isViewOnce: message.isViewOnce ?? false,
    isConsumed: message.isConsumed ?? false,
});


watch(
    () => props.chatId,
    async (_) => {
        await loadChat();
    }
)

const loadChat = async () => {
    loading.value = true;
    try {
        const chatId = props.chatId;
        if (!chatId) {
            noChat.value = true;
            return;
        }
        currentUserUid.value = authStore.userId.value || '';

        if (!currentUserUid.value) {
            notification.error('You must be logged in to view chats');
            return;
        }

        const result = await services.chats.getChatById(chatId);
        if (result.isSuccess && result.data) {
            chat.value = result.data;

            if (chat.value.isGroup) {
                // Resolve every other member's profile so the header can list participants.
                const otherMemberUids = getMemberUids().filter(uid => uid && uid !== currentUserUid.value);
                const names: string[] = [];
                for (const memberUid of otherMemberUids) {
                    const profileResult = await services.users.getUserProfile(memberUid, currentUserUid.value);
                    if (profileResult.isSuccess && profileResult.data) {
                        messageSenderProfiles.value.set(memberUid, profileResult.data);
                        names.push(getDisplayUserName(
                            memberUid,
                            profileResult.data.userName,
                            profileResult.data.firstName,
                            profileResult.data.lastName
                        ));
                    } else {
                        names.push(memberUid.substring(0, 8) + '...');
                    }
                }
                otherUserDisplayName.value = (chat.value.name && chat.value.name.trim())
                    ? chat.value.name
                    : (names.length > 0 ? names.join(', ') : 'Group chat');
            } else {
                const otherUserUid = getOtherUserUid();
                if (otherUserUid) {
                    const displayNameResult = await services.users.getUserProfile(otherUserUid, currentUserUid.value);
                    if (displayNameResult.isSuccess && displayNameResult.data) {
                        otherUserProfile.value = displayNameResult.data;
                        otherUserDisplayName.value = getDisplayUserName(
                            otherUserUid,
                            displayNameResult.data.userName,
                            displayNameResult.data.firstName,
                            displayNameResult.data.lastName
                        );
                    }
                }
            }

            await loadMessages(chatId);

            // Try to initialize socket connection for real-time updates
            try {
                await sockets.hub.initialize();
                await sockets.hub.joinChat(chatId);
                await sockets.hub.registerToEvent('ReceiveMessage', handleIncomingMessage);
                await sockets.hub.registerToEvent('UpdateMessage', handleMessageUpdate);
                await sockets.hub.registerToEvent('DeleteMessage', handleMessageDelete);
                await sockets.hub.registerToEvent('MessageExpired', handleMessageExpired);
                isSocketConnected.value = true;
            } catch (socketError) {
                console.warn('Failed to connect to real-time messaging. Messages will not update automatically:', socketError);
                isSocketConnected.value = false;
            }
            noChat.value = false;
        } else {
            if (result.responseCode === 404) {
                notification.error('Chat not found');
            }
        }
    } catch (error) {
        notification.error('Error loading chat');
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
                .map(msg => normalizeMessage(msg, chatId))
                // Filter expired (disappeared) messages on load as a client-side safety net.
                .filter(msg => !isExpired(msg))
                .sort((a, b) => {
                    const dateA = new Date(a.timestamp);
                    const dateB = new Date(b.timestamp);
                    if (isNaN(dateA.getTime()) && isNaN(dateB.getTime())) return 0;
                    if (isNaN(dateA.getTime())) return 1;
                    if (isNaN(dateB.getTime())) return -1;
                    return dateA.getTime() - dateB.getTime();
                });

            // Load profile data for unique message senders
            const uniqueSenders = [...new Set(messages.value.map(m => m.senderUid))];
            for (const senderUid of uniqueSenders) {
                if (senderUid && senderUid !== currentUserUid.value) {
                    try {
                        const profileResult = await services.users.getUserProfile(senderUid, currentUserUid.value);
                        if (profileResult.isSuccess && profileResult.data) {
                            messageSenderProfiles.value.set(senderUid, profileResult.data);
                        }
                    } catch (err) {
                        console.warn(`Failed to load profile for sender ${senderUid}:`, err);
                    }
                }
            }

            // Fetch attachments for the loaded messages (authorized per-message endpoint).
            for (const loadedMessage of messages.value) {
                void loadMessageAttachments(loadedMessage.uid);
            }

            setTimeout(() => scrollToBottom(false), 100);
        }
    } catch (error) {
    } finally {
        loadingMessages.value = false;
    }
};

// Lazily fetch and cache the attachments linked to a message.
const loadMessageAttachments = async (messageUid: string) => {
    if (messageAttachments.value.has(messageUid) || fetchingAttachmentsFor.value.has(messageUid)) {
        return;
    }
    fetchingAttachmentsFor.value.add(messageUid);
    try {
        const result = await services.attachments.getByMessage(messageUid);
        if (result.isSuccess && result.data && result.data.length > 0) {
            messageAttachments.value.set(messageUid, result.data);
        }
    } catch (err) {
        // Non-fatal: the message still renders without its attachments.
    } finally {
        fetchingAttachmentsFor.value.delete(messageUid);
    }
};

const attachmentsForMessage = (messageUid: string): AttachmentDto[] => {
    return messageAttachments.value.get(messageUid) ?? [];
};

const handleIncomingMessage = async (message: MessageBo) => {
    if (!chat.value || message.chatUid !== chat.value.uid) {
        return;
    }

    const completeMessage: MessageBo = normalizeMessage(message, chat.value.uid);

    if (isExpired(completeMessage)) {
        return;
    }

    if (!completeMessage.timestamp) {
        completeMessage.timestamp = new Date().toISOString();
    } else {
        const testDate = new Date(completeMessage.timestamp);
        if (isNaN(testDate.getTime())) {
            completeMessage.timestamp = new Date().toISOString();
        }
    }

    const existingIndex = messages.value.findIndex(m => m.uid === completeMessage.uid);

    if (existingIndex !== -1) {
        messages.value[existingIndex] = completeMessage;
    } else {
        if (message.senderUid !== currentUserUid.value) {
            messages.value.push(completeMessage);
            void loadMessageAttachments(completeMessage.uid);

            if (message.senderUid && !messageSenderProfiles.value.has(message.senderUid)) {
                try {
                    const profileResult = await services.users.getUserProfile(message.senderUid, currentUserUid.value);
                    if (profileResult.isSuccess && profileResult.data) {
                        messageSenderProfiles.value.set(message.senderUid, profileResult.data);
                    }
                } catch (err) {
                    console.warn(`Failed to load profile for new sender ${message.senderUid}:`, err);
                }
            }

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

// A message disappeared (timer expired) or was consumed (view-once) for this user. The backend
// sends (chatUid, messageId); chatUid may be null for a global scheduled purge.
const handleMessageExpired = (_chatUid: string | null, messageId: string) => {
    messages.value = messages.value.filter(m => m.uid !== messageId);
    revealedViewOnceContent.value.delete(messageId);
    revealingViewOnceIds.value.delete(messageId);
    if (editingMessageId.value === messageId) {
        editingMessageId.value = null;
        editingContent.value = '';
    }
};


const sendMessage = async () => {
    // Allow sending a message that carries only attachments (no text).
    if ((!newMessage.value.trim() && pendingAttachments.value.length === 0) || !chat.value) {
        return;
    }

    sending.value = true;
    const messageContent = newMessage.value.trim();
    const attachmentIds = pendingAttachments.value.map(attachment => attachment.uid);
    newMessage.value = '';
    editingMessageId.value = null;
    editingContent.value = '';

    try {
        const messageData: CreateMessageDto = {
            content: messageContent,
            senderUid: currentUserUid.value,
            chatUid: chat.value.uid,
            disappearAfterSeconds: disappearAfterSeconds.value > 0 ? disappearAfterSeconds.value : null,
            isViewOnce: isViewOnce.value,
            attachmentIds: attachmentIds.length > 0 ? attachmentIds : undefined,
        };

        const result = await services.messages.createMessage(messageData);

        if (result.isSuccess && result.data) {
            // Fetch the persisted message to render it. The sender always sees their own content
            // (incl. view-once); if the GET 404s (e.g. a disappear race purged it) we simply skip
            // re-adding it rather than crashing or re-showing an already-gone message.
            const fetchResult = await services.messages.getMessageById(result.data);
            if (fetchResult.isSuccess && fetchResult.data) {
                messages.value.push(normalizeMessage(fetchResult.data, chat.value.uid));
                // Show the just-uploaded attachments immediately under the sent message.
                if (pendingAttachments.value.length > 0) {
                    messageAttachments.value.set(result.data, [...pendingAttachments.value]);
                }
                scrollToBottom();
            }
            // Reset ephemeral options and the composer attachments after a successful send.
            isViewOnce.value = false;
            pendingAttachments.value = [];
        } else {
            notification.error('Failed to send message');
            newMessage.value = messageContent;
        }
    } catch (error) {
        notification.error('Failed to send message');
        newMessage.value = messageContent;
    } finally {
        sending.value = false;
        focusMessageInput();
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
            notification.error('Failed to update message');
        }
    } catch (error) {
        notification.error('Failed to update message');
    }
};

const deleteMessage = async (messageId: string) => {
    if (!(await confirm({
        title: 'Delete message',
        message: 'Are you sure you want to delete this message?',
        confirmLabel: 'Delete',
        danger: true,
    }))) {
        return;
    }

    try {
        const result = await services.messages.deleteMessage(messageId);
        if (result.isSuccess) {
            messages.value = messages.value.filter(m => m.uid !== messageId);
            notification.deleted('Message deleted');
        } else {
            notification.error('Failed to delete message');
        }
    } catch (error) {
        notification.error('Failed to delete message');
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

const focusMessageInput = async () => {
    await nextTick();
    const inputRefValue = messageInputRef.value;
    if (inputRefValue) {
        inputRefValue.focus();
    }
};

const getMemberUids = (): string[] => {
    if (!chat.value) return [];
    if (chat.value.memberUids && chat.value.memberUids.length > 0) {
        return chat.value.memberUids;
    }
    return [chat.value.user1Uid, chat.value.user2Uid];
};

const getOtherUserUid = () => {
    if (!chat.value) return '';
    const others = getMemberUids().filter(uid => uid && uid !== currentUserUid.value);
    return others[0] ?? (chat.value.user1Uid === currentUserUid.value ? chat.value.user2Uid : chat.value.user1Uid);
};

const getSenderDisplayName = (senderUid: string): string => {
    const profile = messageSenderProfiles.value.get(senderUid);
    if (profile) {
        return getDisplayUserName(senderUid, profile.userName, profile.firstName, profile.lastName);
    }
    return senderUid.substring(0, 8) + '...';
};

function getDisplayUserName(creatorUid: string, userName?: string, firstName?: string, lastName?: string): string {
    return userName ||
        (firstName && lastName
            ? `${firstName} ${lastName}`
            : creatorUid.substring(0, 8) + '...');
}

const isMyMessage = (message: MessageBo) => {
    return message.senderUid === currentUserUid.value;
};

// A view-once message that this user (a recipient) has not yet revealed: show a tap-to-reveal
// affordance instead of the content. The sender always sees their own content. Once revealed, the
// uid is in revealedViewOnceContent and this returns false so the content (not the lock) renders.
const isHiddenViewOnce = (message: MessageBo): boolean => {
    return !!message.isViewOnce
        && !isMyMessage(message)
        && !message.isConsumed
        && !revealedViewOnceContent.value.has(message.uid);
};

// Content to render in a bubble: the revealed plaintext for a consumed view-once message we opened
// this session, otherwise the message's own content (normal/disappearing, or the sender's own).
const displayedContent = (message: MessageBo): string => {
    const revealed = revealedViewOnceContent.value.get(message.uid);
    return revealed !== undefined ? revealed : message.content;
};

const revealViewOnce = async (message: MessageBo) => {
    if (!isHiddenViewOnce(message) || revealingViewOnceIds.value.has(message.uid)) {
        return;
    }
    // The consume GET reveals the content exactly once; guard against double taps firing it twice.
    revealingViewOnceIds.value.add(message.uid);
    try {
        const result = await services.messages.getMessageById(message.uid);
        if (result.isSuccess && result.data) {
            // Show the returned content once; it stays in the map so it cannot be revealed again.
            revealedViewOnceContent.value.set(message.uid, result.data.content || '');
        } else {
            // Already consumed elsewhere / expired: the backend returns 404 (no content).
            revealingViewOnceIds.value.delete(message.uid);
            notification.error('This message is no longer available');
        }
    } catch (error) {
        revealingViewOnceIds.value.delete(message.uid);
        notification.error('Failed to open message');
    }
};

const shouldShowEditMode = (message: MessageBo) => {
    return editingMessageId.value === message.uid &&
        isMyMessage(message) &&
        currentUserUid.value &&
        message.uid;
};

const formatTime = (timestamp: string) => {
    let dateString = timestamp;

    if (!timestamp.includes('Z') && !timestamp.includes('+') && !timestamp.includes('-', 10)) {
        dateString = timestamp + 'Z';
    }

    const date = new Date(dateString);

    if (isNaN(date.getTime())) {
        console.error("Invalid Date:", dateString)
        return '';
    }

    return date.toLocaleTimeString(navigator.language || 'en-US', {
        hour: '2-digit',
        minute: '2-digit'
    });
};

onMounted(() => {
    loadChat();
});

onUnmounted(async () => {
    // Remove this component's message listeners so they don't accumulate across remounts
    // (otherwise navigating back into a chat would handle each incoming message multiple times).
    await sockets.hub.unregisterFromEvent('ReceiveMessage', handleIncomingMessage);
    await sockets.hub.unregisterFromEvent('UpdateMessage', handleMessageUpdate);
    await sockets.hub.unregisterFromEvent('DeleteMessage', handleMessageDelete);
    await sockets.hub.unregisterFromEvent('MessageExpired', handleMessageExpired);
    if (chat.value && isSocketConnected.value) {
        try {
            await sockets.hub.leaveChat(chat.value.uid);
        } catch (error) {
            console.warn('Error leaving chat:', error);
        }
    }
});
</script>

<template>
    <div style="height: calc(100vh - 80px);">
        <div class="d-flex justify-content-center h-100">
            <div class="col-12 d-flex flex-column h-100">
                <div v-if="loading" class="d-flex justify-content-center align-items-center flex-grow-1">
                    <div class="text-center">
                        <div class="spinner-border text-primary mb-3" role="status" style="width: 3rem; height: 3rem;">
                            <span class="visually-hidden">Loading...</span>
                        </div>
                        <div class="text-muted">Loading chat...</div>
                    </div>
                </div>

                <template v-else-if="chat">
                    <div class="chat-header">
                        <div class="card-body py-3">
                            <div class="d-flex justify-content-between align-items-center">
                                <div>
                                    <a
                                        v-if="!chat.isGroup"
                                        href="#"
                                        class="text-primary text-decoration-none d-flex align-items-center gap-2 fw-bold"
                                        @click.prevent="router.push({ path: '/profile', query: { id: getOtherUserUid() } })"
                                    >
                                      <span class="me-2 d-md-none" @click.stop="router.replace('/chats')" style="cursor: pointer;">&lt;</span>
                                      <ProfileIconDisplay
                                          :icon="otherUserProfile?.profileIcon"
                                          :color="otherUserProfile?.profileIconColor"
                                          size="lg"
                                          class="bg-white"
                                      />
                                      {{ otherUserDisplayName || getOtherUserUid() }}
                                    </a>
                                    <div
                                        v-else
                                        class="text-primary d-flex align-items-center gap-2 fw-bold"
                                    >
                                      <span class="me-2 d-md-none" @click.stop="router.replace('/chats')" style="cursor: pointer;">&lt;</span>
                                      <span
                                          class="d-inline-flex align-items-center justify-content-center rounded-circle bg-secondary text-white"
                                          style="width: 40px; height: 40px;"
                                      >
                                          <i class="fas fa-users"></i>
                                      </span>
                                      {{ otherUserDisplayName || 'Group chat' }}
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="flex-grow-1 d-flex flex-column" style="min-height: 0;">
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
                                    class="mb-3 d-flex"
                                    :class="{'justify-content-end': isMyMessage(message), 'justify-content-start': !isMyMessage(message)}"
                                >

                                    <div
                                        class="rounded position-relative"
                                        :class="isMyMessage(message) ? 'bg-primary text-white' : 'bg-light'"
                                        style="max-width: 70%;"
                                    >
                                        <div v-if="shouldShowEditMode(message)" class="px-3 py-2">
                                            <input
                                                v-model="editingContent"
                                                type="text"
                                                spellcheck="true"
                                                lang="en"
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
                                            <div
                                                v-if="chat.isGroup && !isMyMessage(message)"
                                                class="small fw-bold mb-1 text-primary"
                                            >
                                                {{ getSenderDisplayName(message.senderUid) }}
                                            </div>

                                            <!-- View-once recipient view: tap to reveal, then consume -->
                                            <div
                                                v-if="isHiddenViewOnce(message)"
                                                class="d-flex align-items-center gap-2 fst-italic"
                                                role="button"
                                                tabindex="0"
                                                title="Tap to view once"
                                                @click="revealViewOnce(message)"
                                                @keyup.enter="revealViewOnce(message)"
                                            >
                                                <i class="fas fa-eye-slash"></i>
                                                <span>View once · tap to reveal</span>
                                            </div>
                                            <div v-else-if="displayedContent(message)">{{ displayedContent(message) }}</div>

                                            <!-- Attachments linked to this message -->
                                            <AttachmentList
                                                v-if="attachmentsForMessage(message.uid).length"
                                                :attachments="attachmentsForMessage(message.uid)"
                                                compact
                                            />

                                            <!-- Ephemeral indicators -->
                                            <div
                                                v-if="message.isViewOnce || message.expiresAt"
                                                class="small mt-1"
                                                :class="isMyMessage(message) ? 'text-white-50' : 'text-muted'"
                                            >
                                                <span v-if="message.isViewOnce" class="me-2">
                                                    <i class="fas fa-eye"></i> View once
                                                </span>
                                                <span v-if="message.expiresAt">
                                                    <i class="fas fa-clock"></i> Disappears
                                                </span>
                                            </div>
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
                                <div class="d-flex align-items-center gap-3 flex-wrap">
                                    <label class="d-flex align-items-center gap-1 small text-muted mb-0">
                                        <i class="fas fa-clock"></i>
                                        Disappear
                                        <select
                                            v-model.number="disappearAfterSeconds"
                                            class="form-select form-select-sm"
                                            style="width: auto;"
                                            :disabled="sending"
                                        >
                                            <option v-for="opt in disappearOptions" :key="opt.value" :value="opt.value">
                                                {{ opt.label }}
                                            </option>
                                        </select>
                                    </label>
                                    <div class="form-check form-switch mb-0">
                                        <input
                                            id="viewOnceToggle"
                                            v-model="isViewOnce"
                                            class="form-check-input"
                                            type="checkbox"
                                            role="switch"
                                            :disabled="sending"
                                        />
                                        <label class="form-check-label small text-muted" for="viewOnceToggle">
                                            <i class="fas fa-eye"></i> View once
                                        </label>
                                    </div>
                                </div>
                                <AttachmentPicker
                                    v-model="pendingAttachments"
                                    :disabled="sending"
                                    compact
                                />
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
                                        spellcheck="true"
                                        lang="en"
                                    />
                                    <button
                                        type="submit"
                                        class="btn btn-primary"
                                        :disabled="(!newMessage.trim() && pendingAttachments.length === 0) || sending"
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

                <div v-else-if="noChat" class="text-center py-5">
                    <i class="fas fa-info-circle fa-3x text-primary mb-3"></i>
                    <h5 class="text-muted">Select chat</h5>
                </div>

                <div v-else class="text-center py-5">
                    <i class="fas fa-exclamation-circle fa-3x text-danger mb-3"></i>
                    <h5 class="text-muted">Chat not found</h5>
                    <button class="btn btn-primary mt-3" @click="router.replace('/chats')">
                        Back to Chats
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>

