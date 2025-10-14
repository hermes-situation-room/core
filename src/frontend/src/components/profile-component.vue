<script setup lang="ts">
import { ref, onMounted, onUpdated } from 'vue';
import { useRoute } from 'vue-router';
import { services } from '../services/api';
import type { UserProfileBo, ActivistBo, UpdateActivistPrivacy } from '../types/user';
import { useAuthStore } from '../stores/auth-store';

const route = useRoute();
const authStore = useAuthStore();

const userProfile = ref<UserProfileBo | null>(null);
const activistPrivacy = ref<ActivistBo | null>(null);
const loading = ref(false);
const error = ref<string | null>(null);

const loadUser = async () => {
    const userId = route.query.id as string;
    const consumerId = authStore.userId.value || '';

    if (!userId) {
        error.value = 'User ID not found';
        return;
    }

    loading.value = true;
    try {
        const result = await services.users.getUserProfile(userId, consumerId);
        if (result.isSuccess && result.data) {
            userProfile.value = result.data;

            if (consumerId === userId) {
                const result = await services.users.getActivistPrivacy(userId);
                if (result.isSuccess && result.data) {
                    activistPrivacy.value = result.data;
                }
            }

        } else {
            error.value = result.responseMessage || 'Failed to load user profile';
        }
    } catch (err) {
        error.value = 'Error loading user profile';
        console.error('Error loading user profile:', err);
    } finally {
        loading.value = false;
    }
};

onMounted(() => {
    loadUser();
});

onUpdated(async () => {
    if (activistPrivacy.value) {
        const updateActivistPrivacy: UpdateActivistPrivacy = {
            isFirstNameVisible: activistPrivacy.value.isFirstNameVisible,
            isLastNameVisible: activistPrivacy.value.isLastNameVisible,
            isEmailVisible: activistPrivacy.value.isEmailVisible,
        }

        await services.users.updateActivistPrivacy(activistPrivacy.value.uid, updateActivistPrivacy);
    }
});

</script>

<template>
    <div class="container py-4">
        <div class="row justify-content-center">
            <div class="col-12 col-lg-10 col-xl-8">

                <div v-if="loading" class="d-flex justify-content-center align-items-center py-5">
                    <div class="text-center">
                        <div class="spinner-border text-primary mb-3" role="status" style="width: 3rem; height: 3rem;">
                            <span class="visually-hidden">Loading...</span>
                        </div>
                        <div class="text-muted">Loading user profile...</div>
                    </div>
                </div>

                <div v-else-if="error" class="card">
                    <div class="card-body text-center p-5">
                        <i class="fas fa-exclamation-triangle mb-4 text-danger" style="font-size: 3rem;"></i>
                        <h3 class="card-title">Error Loading Profile</h3>
                        <p class="card-text text-muted">{{ error }}</p>
                    </div>
                </div>

                <div v-if="userProfile" class="card shadow">
                    <div class="card-body px-4 py-5">

                        <div class="text-center mb-4">
                            <i class="fa-solid fa-circle-user fa-10x"></i>
                        </div>

                        <div class="row">
                            <div class="col-md-6">
                                <div v-if="userProfile.userName != null" class="mb-3">
                                    <strong class="d-block">UserName</strong>
                                    <div>{{ userProfile.userName }}</div>
                                </div>
                                <div class="mb-3">
                                    <strong class="d-block">FirstName</strong>
                                    <div>{{ userProfile.firstName || '' }}</div>
                                </div>
                                <div class="mb-3">
                                    <strong class="d-block">LastName</strong>
                                    <div>{{ userProfile.lastName || '' }}</div>
                                </div>
                                <div class="mb-3">
                                    <strong class="d-block">EmailAddress</strong>
                                    <div>{{ userProfile.emailAddress || '' }}</div>
                                </div>
                                <div v-if="userProfile.employer != null" class="mb-3">
                                    <strong class="d-block">Employer</strong>
                                    <div>{{ userProfile.employer }}</div>
                                </div>
                            </div>

                            <div v-if="activistPrivacy" class="col-md-6">
                                <div class="mb-3 text-end">
                                    <strong>Privacy Settings</strong>
                                </div>
                                <div class="mb-3 text-end">
                                    <label class="form-label invisible">Visibility</label>
                                    <input
                                        class="form-check-input"
                                        type="checkbox"
                                        role="switch"
                                        id="visibilitySwitch"
                                        v-model="activistPrivacy.isFirstNameVisible"
                                        />
                                </div>
                                <div class="mb-3 text-end">
                                    <label class="form-label invisible">Visibility</label>
                                    <input
                                        class="form-check-input"
                                        type="checkbox"
                                        role="switch"
                                        id="visibilitySwitch"
                                        v-model="activistPrivacy.isLastNameVisible"
                                        />
                                </div>
                                <div class="mb-3 text-end">
                                    <label class="form-label invisible">Visibility</label>
                                    <input
                                        class="form-check-input"
                                        type="checkbox"
                                        role="switch"
                                        id="visibilitySwitch"
                                        v-model="activistPrivacy.isEmailVisible"
                                        />
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
