<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { services } from '../services/api';
import type {UserProfileBo, ActivistBo, JournalistBo} from '../types/user';
import { useAuthStore } from '../stores/auth-store';
import { useNotification } from '../composables/use-notification.ts';
import ProfileIconSelector from './profile-icon-selector.vue';
import { ProfileIcon, ProfileIconColor } from '../types/profile-icon.ts';

const router = useRouter();
const authStore = useAuthStore();
const notification = useNotification();

const userProfile = ref<UserProfileBo | null>(null);
const activistPrivacy = ref<ActivistBo | null>(null);
const loading = ref(false);
const saving = ref(false);

const firstName = ref('');
const lastName = ref('');
const emailAddress = ref('');
const userName = ref('');
const employer = ref('');

const isFirstNameVisible = ref(true);
const isLastNameVisible = ref(true);
const isEmailVisible = ref(true);

const iconSelection = ref({
    icon: ProfileIcon.User,
    color: ProfileIconColor.Blue
});

const isActivist = computed(() => {
    return activistPrivacy.value != null;
});

const isJournalist = computed(() => {
    return userProfile.value?.employer != null;
});

const loadProfile = async () => {
    const userId = authStore.userId.value || '';

    if (!userId) {
        notification.error('You must be logged in to edit your profile');
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
            
            iconSelection.value = {
                icon: (result.data.profileIcon as ProfileIcon) || ProfileIcon.User,
                color: (result.data.profileIconColor as ProfileIconColor) || ProfileIconColor.Blue
            };

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
            notification.error(result.responseMessage || 'Failed to load profile');
        }
    } catch (err) {
        notification.error('Error loading profile');
        console.error('Error loading profile:', err);
    } finally {
        loading.value = false;
    }
};

const saveProfile = async () => {
    const userId = authStore.userId.value || '';
    
    if (!userId) {
        notification.error('You must be logged in');
        return;
    }

    if (isJournalist.value) {
        if (!firstName.value.trim() || !lastName.value.trim() || !emailAddress.value.trim()) {
            notification.warning('First Name, Last Name, and Email are required for journalists');
            return;
        }
    }

    saving.value = true;

    try {
        let result;

        if (isActivist.value && activistPrivacy.value) {
            const activistData: ActivistBo = {
                uid: userId,
                password: '',
                firstName: firstName.value.trim() || undefined,
                lastName: lastName.value.trim() || undefined,
                emailAddress: emailAddress.value.trim() || undefined,
                profileIcon: iconSelection.value.icon,
                profileIconColor: iconSelection.value.color,
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
                profileIcon: iconSelection.value.icon,
                profileIconColor: iconSelection.value.color,
                employer: employer.value
            };

            result = await services.users.updateJournalist(userId, journalistData);
        } else {
            notification.error('Unable to determine user type');
            saving.value = false;
            return;
        }

        if (!result.isSuccess) {
            notification.error(result.responseMessage || 'Failed to update profile');
            saving.value = false;
            return;
        }

        notification.updated('Profile updated successfully!');
        setTimeout(() => {
            router.push({ path: '/profile', query: { id: userId } });
        }, 1500);
    } catch (err) {
        notification.error('Error saving profile');
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
                                <h5 class="border-bottom pb-2 mb-3">Profile Icon</h5>
                                <ProfileIconSelector v-model="iconSelection" />
                            </div>

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
