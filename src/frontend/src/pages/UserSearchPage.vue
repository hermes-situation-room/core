<script setup lang="ts">
import { ref, computed } from 'vue';
import { useRouter } from 'vue-router';
import { services } from '../services/api';
import type { UserProfileBo } from '../types/user';
import { useAuthStore } from '../stores/auth-store';
import { useNotification } from '../composables/use-notification.ts';
import ProfileIconDisplay from '../components/profile-icon-display.vue';

const router = useRouter();
const authStore = useAuthStore();
const notification = useNotification();

const searchQuery = ref('');
const searchResults = ref<UserProfileBo[]>([]);
const isSearching = ref(false);
const hasSearched = ref(false);

const currentUserId = computed(() => authStore.userId.value || '');

const performSearch = async () => {
    if (!searchQuery.value.trim()) {
        notification.warning('Please enter a search term');
        return;
    }

    isSearching.value = true;
    hasSearched.value = true;
    searchResults.value = [];

    try {
        // Try to find user by the search query
        const userIdResult = await services.users.getUserIdByUsernameOrEmail(searchQuery.value.trim());
        
        if (userIdResult.isSuccess && userIdResult.data) {
            // Get the full profile
            const profileResult = await services.users.getUserProfile(
                userIdResult.data,
                currentUserId.value || userIdResult.data
            );
            
            if (profileResult.isSuccess && profileResult.data) {
                searchResults.value = [profileResult.data];
            } else {
                notification.info('No users found matching your search');
            }
        } else {
            notification.info('No users found matching your search');
        }
    } catch (error) {
        console.error('Search error:', error);
        notification.error('An error occurred while searching');
    } finally {
        isSearching.value = false;
    }
};

const viewProfile = (userId: string) => {
    router.push({
        path: '/profile',
        query: { id: userId }
    });
};

const sendMessage = async (userId: string) => {
    if (!currentUserId.value) {
        notification.warning('Please log in to send messages');
        router.push('/login');
        return;
    }

    if (userId === currentUserId.value) {
        notification.warning('You cannot send a message to yourself');
        return;
    }

    try {
        const chatResult = await services.chats.getOrCreateChatByUserPair(
            currentUserId.value,
            userId
        );

        if (chatResult.isSuccess && chatResult.data) {
            router.push(`/chat/${chatResult.data.uid}`);
        } else {
            notification.error(chatResult.responseMessage || 'Failed to open chat');
        }
    } catch (err) {
        notification.error('An error occurred while opening the chat');
    }
};

const clearSearch = () => {
    searchQuery.value = '';
    searchResults.value = [];
    hasSearched.value = false;
};

const handleKeyPress = (event: KeyboardEvent) => {
    if (event.key === 'Enter') {
        performSearch();
    }
};
</script>

<template>
    <div class="container py-4">
        <div class="row justify-content-center">
            <div class="col-12 col-lg-10 col-xl-8">
                <div class="card shadow">
                    <div class="card-header bg-primary text-white">
                        <h4 class="mb-0">
                            <i class="fas fa-search me-2"></i>
                            Search Users
                        </h4>
                    </div>
                    
                    <div class="card-body">
                        <div class="alert alert-info mb-4">
                            <i class="fas fa-info-circle me-2"></i>
                            <small>
                                Search for activists by <strong>username</strong> or journalists by <strong>email address</strong> or <strong>name</strong>.
                            </small>
                        </div>

                        <div class="mb-4">
                            <div class="input-group input-group-lg">
                                <span class="input-group-text bg-white">
                                    <i class="fas fa-search text-muted"></i>
                                </span>
                                <input
                                    v-model="searchQuery"
                                    @keypress="handleKeyPress"
                                    type="text"
                                    class="form-control"
                                    placeholder="Enter username or email..."
                                    :disabled="isSearching"
                                />
                                <button 
                                    v-if="searchQuery"
                                    class="btn btn-outline-secondary"
                                    type="button"
                                    @click="clearSearch"
                                    :disabled="isSearching"
                                >
                                    <i class="fas fa-times"></i>
                                </button>
                                <button 
                                    class="btn btn-primary"
                                    type="button"
                                    @click="performSearch"
                                    :disabled="isSearching || !searchQuery.trim()"
                                >
                                    <span v-if="isSearching" class="spinner-border spinner-border-sm me-2"></span>
                                    {{ isSearching ? 'Searching...' : 'Search' }}
                                </button>
                            </div>
                        </div>

                        <div v-if="isSearching" class="text-center py-5">
                            <div class="spinner-border text-primary mb-3" role="status" style="width: 3rem; height: 3rem;">
                                <span class="visually-hidden">Searching...</span>
                            </div>
                            <div class="text-muted">Searching for users...</div>
                        </div>

                        <div v-else-if="hasSearched && searchResults.length === 0" class="text-center py-5">
                            <i class="fas fa-user-slash fa-3x text-muted mb-3"></i>
                            <h5 class="text-muted">No users found</h5>
                            <p class="text-muted">Try searching with a different term</p>
                        </div>

                        <div v-else-if="searchResults.length > 0" class="search-results">
                            <h6 class="mb-3">
                                Found {{ searchResults.length }} user{{ searchResults.length !== 1 ? 's' : '' }}
                            </h6>
                            
                            <div class="list-group">
                                <div
                                    v-for="user in searchResults"
                                    :key="user.uid"
                                    class="list-group-item list-group-item-action"
                                >
                                    <div class="d-flex align-items-center gap-3">
                                        <div class="flex-shrink-0">
                                            <ProfileIconDisplay 
                                                :icon="user.profileIcon" 
                                                :color="user.profileIconColor" 
                                                size="lg" 
                                            />
                                        </div>
                                        
                                        <div class="flex-grow-1">
                                            <div class="d-flex align-items-center gap-2 mb-1">
                                                <h6 class="mb-0">
                                                    {{ user.userName || `${user.firstName} ${user.lastName}` }}
                                                </h6>
                                                <span v-if="user.userName" class="badge bg-dark">
                                                    <i class="fas fa-bullhorn me-1"></i>
                                                    Activist
                                                </span>
                                                <span v-else class="badge bg-dark">
                                                    <i class="fas fa-newspaper me-1"></i>
                                                    Journalist
                                                </span>
                                            </div>
                                            
                                            <div class="text-muted small">
                                                <div v-if="user.userName">
                                                    <span v-if="user.firstName && user.firstName !== '[REDACTED]'">
                                                        {{ user.firstName }}
                                                    </span>
                                                    <span v-if="user.lastName && user.lastName !== '[REDACTED]'">
                                                        {{ user.lastName }}
                                                    </span>
                                                </div>
                                                <div v-if="user.employer">
                                                    <i class="fas fa-briefcase me-1"></i>
                                                    {{ user.employer }}
                                                </div>
                                                <div v-if="user.emailAddress && user.emailAddress !== '[REDACTED]'">
                                                    <i class="fas fa-envelope me-1"></i>
                                                    {{ user.emailAddress }}
                                                </div>
                                            </div>
                                        </div>
                                        
                                        <div class="flex-shrink-0 d-flex gap-2">
                                            <button
                                                @click="viewProfile(user.uid)"
                                                class="btn btn-sm btn-outline-primary"
                                            >
                                                <i class="fas fa-user me-1"></i>
                                                Profile
                                            </button>
                                            <button
                                                v-if="currentUserId && user.uid !== currentUserId"
                                                @click="sendMessage(user.uid)"
                                                class="btn btn-sm btn-primary"
                                            >
                                                <i class="fas fa-comment me-1"></i>
                                                Message
                                            </button>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div v-else class="text-center py-5 text-muted">
                            <i class="fas fa-search fa-3x mb-3"></i>
                            <h5>Search for Users</h5>
                            <p>Enter a username or email address to find other users</p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
.search-results {
    animation: fadeIn 0.3s ease-in;
}

@keyframes fadeIn {
    from {
        opacity: 0;
        transform: translateY(-10px);
    }
    to {
        opacity: 1;
        transform: translateY(0);
    }
}
</style>

