<script setup lang="ts">
import {ref} from 'vue';
import {useRouter} from 'vue-router';
import {services} from '../services/api';
import { useAuthStore } from '../stores/auth-store';
import { useErrorStore } from '../stores/error-store';
import { useNotifications } from '../composables/use-notifications';

const router = useRouter();
const authStore = useAuthStore();
const errorStore = useErrorStore();
const { showCreateSuccess } = useNotifications();

const otherUserUid = ref('');
const creating = ref(false);

const createChat = async () => {
    if (!otherUserUid.value.trim()) {
        errorStore.addError({
            category: 'validation',
            message: 'Please enter a user ID'
        });
        return;
    }

    creating.value = true;

    try {
        const currentUserUid = authStore.userId.value || '';
        
        if (!currentUserUid) {
            errorStore.addError({
                category: 'authentication',
                message: 'You must be logged in to create a chat'
            });
            creating.value = false;
            return;
        }

        const chatResult = await services.chats.getOrCreateChatByUserPair(
            currentUserUid,
            otherUserUid.value.trim()
        );

        if (chatResult.isSuccess && chatResult.data) {
            showCreateSuccess('Chat');
            router.push(`/chat/${chatResult.data.uid}`);
        } else if (chatResult.error) {
            errorStore.addError(chatResult.error);
        }
    } catch (err) {
        errorStore.addError({
            category: 'unknown',
            message: 'An error occurred while opening the chat'
        });
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
                        <h4 class="mb-0">Create New Chat</h4>
                    </div>
                    <div class="card-body">
                        <form @submit.prevent="createChat">
                            <div class="mb-3">
                                <label for="otherUserUid" class="form-label">User ID</label>
                                <input 
                                    id="otherUserUid"
                                    v-model="otherUserUid"
                                    type="text"
                                    class="form-control"
                                    placeholder="Enter the UID of the user you want to chat with"
                                    :disabled="creating"
                                    required
                                />
                                <div class="form-text">
                                    Enter the unique ID of the user you want to start a chat with.
                                </div>
                            </div>

                            <div class="d-flex gap-2">
                                <button 
                                    type="submit"
                                    class="btn btn-primary"
                                    :disabled="creating || !otherUserUid.trim()"
                                >
                                    <span v-if="creating" class="spinner-border spinner-border-sm me-2"></span>
                                    {{ creating ? 'Creating...' : 'Create Chat' }}
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

