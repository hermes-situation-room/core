<script setup lang="ts">
import {ref} from 'vue';
import {useRouter} from 'vue-router';
import {services} from '../services/api';
import type {CreateChatRequest} from '../types/chat';
import { useAuthStore } from '../stores/auth-store';

const router = useRouter();
const authStore = useAuthStore();

const otherUserUid = ref('');
const creating = ref(false);
const error = ref('');

const createChat = async () => {
    if (!otherUserUid.value.trim()) {
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

        const existingChatResult = await services.chats.getChatByUserPair(
            currentUserUid,
            otherUserUid.value.trim()
        );

        if (existingChatResult.isSuccess && existingChatResult.data) {
            router.push(`/chat/${existingChatResult.data.uid}`);
            return;
        }

        const chatData: CreateChatRequest = {
            user1Uid: currentUserUid,
            user2Uid: otherUserUid.value.trim()
        };

        const result = await services.chats.createChat(chatData);
        
        if (result.isSuccess && result.data) {
            router.push(`/chat/${result.data}`);
        } else {
            error.value = result.responseMessage || 'Failed to create chat';
        }
    } catch (err) {
        error.value = 'An error occurred while creating the chat';
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
                                    :class="{'is-invalid': error}"
                                    placeholder="Enter the UID of the user you want to chat with"
                                    :disabled="creating"
                                    required
                                />
                                <div v-if="error" class="invalid-feedback d-block">
                                    {{ error }}
                                </div>
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

