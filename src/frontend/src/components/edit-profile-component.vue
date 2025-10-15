<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { services } from '../services/api';
import type {UserProfileBo, ActivistBo, JournalistBo} from '../types/user';
import { useAuthStore } from '../stores/auth-store';

const router = useRouter();
const authStore = useAuthStore();

const userProfile = ref<UserProfileBo | null>(null);
const activistPrivacy = ref<ActivistBo | null>(null);
const loading = ref(false);
const saving = ref(false);
const error = ref<string | null>(null);
const successMessage = ref<string | null>(null);

const firstName = ref('');
const lastName = ref('');
const emailAddress = ref('');
const userName = ref('');
const employer = ref('');

const isFirstNameVisible = ref(true);
const isLastNameVisible = ref(true);
const isEmailVisible = ref(true);

const isActivist = computed(() => {
    return activistPrivacy.value != null;
});

const isJournalist = computed(() => {
    return userProfile.value?.employer != null;
});

const loadProfile = async () => {
    const userId = authStore.userId.value || '';

    if (!userId) {
        error.value = 'You must be logged in to edit your profile';
        router.push('/login');
        return;
    }

    loading.value = true;
    try {
        const result = await services.users.getUserProfile(userId, userId);
        if (result.isSuccess && result.data) {
            userProfile.value = result.data;
            
            firstName.value = result.data.firstName || '';
            lastName.value = result.data.lastName || '';
            emailAddress.value = result.data.emailAddress || '';
            userName.value = result.data.userName || '';
            employer.value = result.data.employer || '';

            if (result.data.userName) {
                const privacyResult = await services.users.getActivistPrivacy(userId);
                if (privacyResult.isSuccess && privacyResult.data) {
                    activistPrivacy.value = privacyResult.data;
                    isFirstNameVisible.value = privacyResult.data.isFirstNameVisible;
                    isLastNameVisible.value = privacyResult.data.isLastNameVisible;
                    isEmailVisible.value = privacyResult.data.isEmailVisible;
                }
            }
        } else {
            error.value = result.responseMessage || 'Failed to load profile';
        }
    } catch (err) {
        error.value = 'Error loading profile';
        console.error('Error loading profile:', err);
    } finally {
        loading.value = false;
    }
};

const saveProfile = async () => {
    const userId = authStore.userId.value || '';
    
    if (!userId) {
        error.value = 'You must be logged in';
        return;
    }

    if (isJournalist.value) {
        if (!firstName.value.trim() || !lastName.value.trim() || !emailAddress.value.trim()) {
            error.value = 'First Name, Last Name, and Email are required for journalists';
            return;
        }
    }

    saving.value = true;
    error.value = null;
    successMessage.value = null;

    try {
        let result;

        if (isActivist.value && activistPrivacy.value) {
            const activistData: ActivistBo = {
                uid: userId,
                password: '',
                firstName: firstName.value.trim() || undefined,
                lastName: lastName.value.trim() || undefined,
                emailAddress: emailAddress.value.trim() || undefined,
                userName: userName.value,
                isFirstNameVisible: isFirstNameVisible.value,
                isLastNameVisible: isLastNameVisible.value,
                isEmailVisible: isEmailVisible.value
            };

            result = await services.users.updateActivist(userId, activistData);
        } else if (isJournalist.value) {
            const journalistData: JournalistBo = {
                password: '',
                uid: userId,
                firstName: firstName.value.trim(),
                lastName: lastName.value.trim(),
                emailAddress: emailAddress.value.trim(),
                employer: employer.value
            };

            result = await services.users.updateJournalist(userId, journalistData);
        } else {
            error.value = 'Unable to determine user type';
            saving.value = false;
            return;
        }

        if (!result.isSuccess) {
            error.value = result.responseMessage || 'Failed to update profile';
            saving.value = false;
            return;
        }

        successMessage.value = 'Profile updated successfully!';
        setTimeout(() => {
            router.push({ path: '/profile', query: { id: userId } });
        }, 1500);
    } catch (err) {
        error.value = 'Error saving profile';
        console.error('Error saving profile:', err);
    } finally {
        saving.value = false;
    }
};

const cancel = () => {
    const userId = authStore.userId.value || '';
    router.push({ path: '/profile', query: { id: userId } });
};

onMounted(() => {
    loadProfile();
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

                <div v-else class="card shadow">
                    <div class="card-header bg-primary text-white">
                        <h4 class="mb-0">
                            <i class="fas fa-user-edit me-2"></i>
                            Edit Profile
                        </h4>
                    </div>

                    <div class="card-body p-4">
                        <div v-if="successMessage" class="alert alert-success alert-dismissible fade show" role="alert">
                            <i class="fas fa-check-circle me-2"></i>
                            {{ successMessage }}
                            <button type="button" class="btn-close" @click="successMessage = null"></button>
                        </div>

                        <div v-if="error" class="alert alert-danger alert-dismissible fade show" role="alert">
                            <i class="fas fa-exclamation-triangle me-2"></i>
                            {{ error }}
                            <button type="button" class="btn-close" @click="error = null"></button>
                        </div>

                        <div class="text-center mb-4">
                            <span v-if="isActivist" class="badge border border-dark text-dark fs-6 px-3 py-2">
                                <i class="fas fa-bullhorn me-2"></i>
                                Activist Profile
                            </span>
                            <span v-else-if="isJournalist" class="badge border border-dark text-dark fs-6 px-3 py-2">
                                <i class="fas fa-newspaper me-2"></i>
                                Journalist Profile
                            </span>
                        </div>

                        <form @submit.prevent="saveProfile">
                            <div class="mb-4">
                                <h5 class="border-bottom pb-2 mb-3">Basic Information</h5>
                                
                                <div v-if="isActivist" class="mb-3">
                                    <label class="form-label">Username</label>
                                    <input 
                                        type="text" 
                                        class="form-control" 
                                        :value="userName" 
                                        disabled
                                    />
                                    <small class="text-muted">Username cannot be changed</small>
                                </div>

                                <div class="row">
                                    <div class="col-md-6 mb-3">
                                        <label class="form-label">
                                            First Name 
                                            <span v-if="isJournalist" class="text-danger">*</span>
                                            <span v-if="isActivist" class="text-muted">(optional)</span>
                                        </label>
                                        <input 
                                            type="text" 
                                            class="form-control" 
                                            v-model="firstName"
                                            :required="isJournalist"
                                            maxlength="100"
                                        />
                                    </div>
                                    <div class="col-md-6 mb-3">
                                        <label class="form-label">
                                            Last Name 
                                            <span v-if="isJournalist" class="text-danger">*</span>
                                            <span v-if="isActivist" class="text-muted">(optional)</span>
                                        </label>
                                        <input 
                                            type="text" 
                                            class="form-control" 
                                            v-model="lastName"
                                            :required="isJournalist"
                                            maxlength="100"
                                        />
                                    </div>
                                </div>

                                <div class="mb-3">
                                    <label class="form-label">
                                        Email 
                                        <span v-if="isJournalist" class="text-danger">*</span>
                                        <span v-if="isActivist" class="text-muted">(optional)</span>
                                    </label>
                                    <input 
                                        type="email" 
                                        class="form-control" 
                                        v-model="emailAddress"
                                        :required="isJournalist"
                                        maxlength="200"
                                    />
                                </div>

                                <div v-if="isJournalist" class="mb-3">
                                    <label class="form-label">Employer</label>
                                    <input 
                                        type="text" 
                                        class="form-control" 
                                        v-model="employer"
                                        maxlength="200"
                                    />
                                </div>
                            </div>

                            <div v-if="isActivist" class="mb-4">
                                <h5 class="border-bottom pb-2 mb-3">Privacy Settings</h5>
                                <p class="text-muted small mb-3">
                                    <i class="fas fa-shield-alt me-1"></i>
                                    Control what information is visible to other users. Note: First Name, Last Name, and Email are optional for activists.
                                </p>

                                <div class="mb-3">
                                    <div class="form-check form-switch">
                                        <input 
                                            class="form-check-input" 
                                            type="checkbox" 
                                            id="firstNameVisible"
                                            v-model="isFirstNameVisible"
                                        />
                                        <label class="form-check-label" for="firstNameVisible">
                                            Show First Name to others
                                        </label>
                                    </div>
                                </div>

                                <div class="mb-3">
                                    <div class="form-check form-switch">
                                        <input 
                                            class="form-check-input" 
                                            type="checkbox" 
                                            id="lastNameVisible"
                                            v-model="isLastNameVisible"
                                        />
                                        <label class="form-check-label" for="lastNameVisible">
                                            Show Last Name to others
                                        </label>
                                    </div>
                                </div>

                                <div class="mb-3">
                                    <div class="form-check form-switch">
                                        <input 
                                            class="form-check-input" 
                                            type="checkbox" 
                                            id="emailVisible"
                                            v-model="isEmailVisible"
                                        />
                                        <label class="form-check-label" for="emailVisible">
                                            Show Email to others
                                        </label>
                                    </div>
                                </div>

                                <div class="alert alert-info">
                                    <i class="fas fa-lightbulb me-2"></i>
                                    <small>Your username is always visible. Privacy settings only affect First Name, Last Name, and Email.</small>
                                </div>
                            </div>

                            <div class="d-flex gap-2 justify-content-end">
                                <button 
                                    type="button" 
                                    class="btn btn-secondary"
                                    @click="cancel"
                                    :disabled="saving"
                                >
                                    <i class="fas fa-times me-1"></i>
                                    Cancel
                                </button>
                                <button 
                                    type="submit" 
                                    class="btn btn-primary"
                                    :disabled="saving"
                                >
                                    <span v-if="saving" class="spinner-border spinner-border-sm me-1"></span>
                                    <i v-else class="fas fa-save me-1"></i>
                                    Save Changes
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
