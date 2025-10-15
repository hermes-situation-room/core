<script setup lang="ts">
import {ref} from 'vue';
import {useRouter} from 'vue-router';
import {services} from '../services/api';
import { useAuthStore } from '../stores/auth-store';

const router = useRouter();
const authStore = useAuthStore();

const otherUserMailOrName = ref('');
const creating = ref(false);
const error = ref('');

const createChat = async () => {
    if (!otherUserMailOrName.value.trim()) {
        error.value = 'Please enter a user ID';
        return;
    }

    creating.value = true;
    error.value = '';

    try {
        const currentUserUid = authStore.userId.value || '';
        
        if (!currentUserUid) {
            error.value = 'You must be logged in to create a chat';
            creating.value = false;
            return;
        }

        const userId = await services.users.getUserIdByUsernameOrEmail(otherUserMailOrName.value.trim());
        
        if (!userId.isSuccess || !userId.data) {
            error.value = 'User not found';
            creating.value = false;
            return;
        }
        
        const chatResult = await services.chats.getOrCreateChatByUserPair(
            currentUserUid,
            userId.data
        );

        if (chatResult.isSuccess && chatResult.data) {
            router.push(`/chat/${chatResult.data.uid}`);
        } else {
            error.value = chatResult.responseMessage || 'Failed to open chat';
        }
    } catch (err) {
        error.value = 'An error occurred while opening the chat';
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
                                <label for="otherUserMailOrName" class="form-label">Username or Email</label>
                                <input 
                                    id="otherUserMailOrName"
                                    v-model="otherUserMailOrName"
                                    type="text"
                                    class="form-control"
                                    :class="{'is-invalid': error}"
                                    placeholder="Username or Email"
                                    :disabled="creating"
                                    required
                                />
                                <div v-if="error" class="invalid-feedback d-block">
                                    {{ error }}
                                </div>
                                <div class="form-text">
                                    Enter the username of an activist or the email of a journalist to start a chat.
                                </div>
                            </div>

                            <div class="d-flex gap-2">
                                <button 
                                    type="submit"
                                    class="btn btn-primary"
                                    :disabled="creating || !otherUserMailOrName.trim()"
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

