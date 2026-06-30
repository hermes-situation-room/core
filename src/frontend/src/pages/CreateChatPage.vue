<script setup lang="ts">
import {computed, ref} from 'vue';
import {useRouter} from 'vue-router';
import {services} from '../services/api';
import { useAuthStore } from '../stores/auth-store';
import { useNotification } from '../composables/use-notification.ts';

const router = useRouter();
const authStore = useAuthStore();
const notification = useNotification();

const otherUserMailOrName = ref('');
const recipients = ref<string[]>([]);
const groupName = ref('');
const creating = ref(false);

const isGroup = computed(() => recipients.value.length > 1);

const addRecipient = () => {
    const value = otherUserMailOrName.value.trim();
    if (!value) {
        return;
    }
    if (recipients.value.includes(value)) {
        notification.warning('That user is already added');
        otherUserMailOrName.value = '';
        return;
    }
    recipients.value.push(value);
    otherUserMailOrName.value = '';
};

const removeRecipient = (value: string) => {
    recipients.value = recipients.value.filter(recipient => recipient !== value);
};

const resolveUserId = async (usernameOrEmail: string): Promise<string | null> => {
    const result = await services.users.getUserIdByUsernameOrEmail(usernameOrEmail);
    if (!result.isSuccess || !result.data) {
        return null;
    }
    return result.data;
};

const createChat = async () => {
    // Allow submitting with a single value still in the input (1:1 quick path).
    if (recipients.value.length === 0 && otherUserMailOrName.value.trim()) {
        addRecipient();
    }

    if (recipients.value.length === 0) {
        notification.warning('Please add at least one user');
        return;
    }

    creating.value = true;

    try {
        const currentUserUid = authStore.userId.value || '';

        if (!currentUserUid) {
            notification.error('You must be logged in to create a chat');
            creating.value = false;
            router.push('/login');
            return;
        }

        const resolvedIds: string[] = [];
        for (const recipient of recipients.value) {
            const userId = await resolveUserId(recipient);
            if (!userId) {
                notification.warning(`User not found: ${recipient}`);
                creating.value = false;
                return;
            }
            resolvedIds.push(userId);
        }

        if (isGroup.value) {
            const memberUids = [currentUserUid, ...resolvedIds];
            const chatResult = await services.chats.createGroupChat(memberUids, groupName.value.trim() || undefined);
            if (chatResult.isSuccess && chatResult.data) {
                router.push(`/chat/${chatResult.data.uid}`);
            } else {
                notification.error(chatResult.responseMessage || 'Failed to create group chat');
            }
        } else {
            const chatResult = await services.chats.getOrCreateChatByUserPair(
                currentUserUid,
                resolvedIds[0]!
            );
            if (chatResult.isSuccess && chatResult.data) {
                router.push(`/chat/${chatResult.data.uid}`);
            } else {
                notification.error(chatResult.responseMessage || 'Failed to open chat');
            }
        }
    } catch (err) {
        notification.error('An error occurred while opening the chat');
    } finally {
        creating.value = false;
    }
};

const cancel = () => {
    router.push('/chats');
};
</script>

<template>
    <div class="container py-4">
        <div class="row justify-content-center">
            <div class="col-12 col-md-8 col-lg-6">
                <div class="card shadow">
                    <div class="card-header bg-primary text-white">
                        <h4 class="mb-0">{{ isGroup ? 'Create Group Chat' : 'Create New Chat' }}</h4>
                    </div>
                    <div class="card-body">
                        <form @submit.prevent="createChat">
                            <div class="mb-3">
                                <label for="otherUserMailOrName" class="form-label">Username or Email</label>
                                <div class="input-group">
                                    <input
                                        id="otherUserMailOrName"
                                        v-model="otherUserMailOrName"
                                        type="text"
                                        class="form-control"
                                        placeholder="Username or Email"
                                        :disabled="creating"
                                        @keydown.enter.prevent="addRecipient"
                                    />
                                    <button
                                        type="button"
                                        class="btn btn-outline-primary"
                                        :disabled="creating || !otherUserMailOrName.trim()"
                                        @click="addRecipient"
                                    >
                                        <i class="fas fa-plus me-1"></i>Add
                                    </button>
                                </div>
                                <div class="form-text">
                                    Enter the username of an activist or the email of a journalist. Add more than one
                                    person to start a group chat.
                                </div>
                            </div>

                            <div v-if="recipients.length > 0" class="mb-3">
                                <label class="form-label">Participants</label>
                                <div class="d-flex flex-wrap gap-2">
                                    <span
                                        v-for="recipient in recipients"
                                        :key="recipient"
                                        class="badge bg-light text-dark border d-inline-flex align-items-center gap-2"
                                    >
                                        {{ recipient }}
                                        <button
                                            type="button"
                                            class="btn-close btn-close-sm"
                                            style="font-size: 0.6rem;"
                                            :disabled="creating"
                                            @click="removeRecipient(recipient)"
                                            aria-label="Remove"
                                        ></button>
                                    </span>
                                </div>
                            </div>

                            <div v-if="isGroup" class="mb-3">
                                <label for="groupName" class="form-label">Group name (optional)</label>
                                <input
                                    id="groupName"
                                    v-model="groupName"
                                    type="text"
                                    class="form-control"
                                    placeholder="Group name"
                                    :disabled="creating"
                                    maxlength="200"
                                />
                            </div>

                            <div class="d-flex gap-2">
                                <button
                                    type="submit"
                                    class="btn btn-primary"
                                    :disabled="creating || (recipients.length === 0 && !otherUserMailOrName.trim())"
                                >
                                    <span v-if="creating" class="spinner-border spinner-border-sm me-2"></span>
                                    {{ creating ? 'Creating...' : (isGroup ? 'Create Group' : 'Create Chat') }}
                                </button>
                                <button
                                    type="button"
                                    class="btn btn-secondary"
                                    :disabled="creating"
                                    @click="cancel"
                                >
                                    Cancel
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
