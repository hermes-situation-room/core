<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { services } from '../services/api';
import type { UserProfileBo } from '../types/user';
import { useAuthStore } from '../stores/auth-store';
import { useNotification } from '../composables/useNotification';

const route = useRoute();
const router = useRouter();
const authStore = useAuthStore();
const notification = useNotification();

const userProfile = ref<UserProfileBo | null>(null);
const loading = ref(false);

const isOwnProfile = computed(() => {
    const userId = route.query.id as string;
    const currentUserId = authStore.userId.value || '';
    return userId === currentUserId;
});

const isActivist = computed(() => {
    return userProfile.value?.userName != null;
});

const isJournalist = computed(() => {
    return userProfile.value?.employer != null;
});

const loadUser = async () => {
    const userId = route.query.id as string;
    const consumerId = authStore.userId.value || '';

    if (!userId) {
        notification.error('User ID not found');
        return;
    }

    loading.value = true;
    userProfile.value = null;
    
    try {
        const result = await services.users.getUserProfile(userId, consumerId);
        if (result.isSuccess && result.data) {
            userProfile.value = result.data;
        } else {
            notification.error(result.responseMessage || 'Failed to load user profile');
        }
    } catch (err) {
        notification.error('Error loading user profile');
        console.error('Error loading user profile:', err);
    } finally {
        loading.value = false;
    }
};

onMounted(() => {
    loadUser();
});

watch(() => route.query.id, () => {
    loadUser();
});

</script>

<template>
    <div class="container py-4">
        <div class="row justify-content-center">
            <div class="col-12 col-lg-8 col-xl-6">

                <div v-if="loading" class="d-flex justify-content-center align-items-center py-5">
                    <div class="text-center">
                        <div class="spinner-border text-primary mb-3" role="status" style="width: 3rem; height: 3rem;">
                            <span class="visually-hidden">Loading...</span>
                        </div>
                        <div class="text-muted">Loading profile...</div>
                    </div>
                </div>

                <div v-else-if="userProfile" class="card shadow">
                    <div class="card-header bg-primary text-white">
                        <div class="d-flex justify-content-between align-items-center">
                            <h4 class="mb-0">
                                <i class="fas fa-user-circle me-2"></i>
                                Profile
                            </h4>
                            <button 
                                v-if="isOwnProfile"
                                class="btn btn-light btn-sm"
                                @click="router.push('/profile/edit')"
                            >
                                <i class="fas fa-edit me-1"></i>
                                Edit Profile
                            </button>
                        </div>
                    </div>

                    <div class="card-body p-4">
                        <div class="text-center mb-4">
                            <div class="mb-3">
                                <i class="fa-solid fa-circle-user text-muted" style="font-size: 6rem;"></i>
                            </div>
                            <span v-if="isActivist" class="badge border border-dark text-dark fs-6 px-3 py-2">
                                <i class="fas fa-bullhorn me-2"></i>
                                Activist
                            </span>
                            <span v-else-if="isJournalist" class="badge border border-dark text-dark fs-6 px-3 py-2">
                                <i class="fas fa-newspaper me-2"></i>
                                Journalist
                            </span>
                        </div>

                        <div v-if="isActivist">
                            <div class="mb-4">
                                <label class="text-muted small mb-1">Username</label>
                                <h5 class="mb-0">{{ userProfile.userName }}</h5>
                            </div>
                            
                            <div class="row">
                                <div class="col-md-6 mb-3">
                                    <label class="text-muted small mb-1">First Name</label>
                                    <div class="fw-medium">{{ userProfile.firstName || '[REDACTED]' }}</div>
                                </div>
                                <div class="col-md-6 mb-3">
                                    <label class="text-muted small mb-1">Last Name</label>
                                    <div class="fw-medium">{{ userProfile.lastName || '[REDACTED]' }}</div>
                                </div>
                            </div>

                            <div class="mb-3">
                                <label class="text-muted small mb-1">Email</label>
                                <div class="fw-medium">{{ userProfile.emailAddress || '[REDACTED]' }}</div>
                            </div>

                            <div v-if="isOwnProfile" class="alert alert-info mt-4">
                                <i class="fas fa-info-circle me-2"></i>
                                <small>To manage privacy settings, use the Edit Profile page.</small>
                            </div>

                            <div v-if="!isOwnProfile && (userProfile.firstName === '[REDACTED]' || userProfile.lastName === '[REDACTED]' || userProfile.emailAddress === '[REDACTED]')" class="alert alert-warning mt-4">
                                <i class="fas fa-shield-alt me-2"></i>
                                <small>Some information is hidden because the user has disabled visibility for these fields.</small>
                            </div>
                        </div>

                        <div v-else-if="isJournalist">
                            <div class="row">
                                <div class="col-md-6 mb-3">
                                    <label class="text-muted small mb-1">First Name</label>
                                    <h5 class="mb-0">{{ userProfile.firstName }}</h5>
                                </div>
                                <div class="col-md-6 mb-3">
                                    <label class="text-muted small mb-1">Last Name</label>
                                    <h5 class="mb-0">{{ userProfile.lastName }}</h5>
                                </div>
                            </div>

                            <div class="mb-3">
                                <label class="text-muted small mb-1">Email</label>
                                <div class="fw-medium">{{ userProfile.emailAddress }}</div>
                            </div>

                            <div class="mb-3">
                                <label class="text-muted small mb-1">Employer</label>
                                <div class="fw-medium">{{ userProfile.employer }}</div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
