<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { services } from '../services/api';
import type {UserProfileBo, ActivistBo, JournalistBo} from '../types/user';
import { useAuthStore } from '../stores/auth-store';
import { useNotification } from '../composables/use-notification.ts';
import ProfileIconSelector from '../components/profile-icon-selector.vue';
import { ProfileIcon, DEFAULT_COLOR } from '../types/profile-icon.ts';

const router = useRouter();
const authStore = useAuthStore();
const notification = useNotification();

const userProfile = ref<UserProfileBo | null>(null);
const activistPrivacy = ref<ActivistBo | null>(null);
const loading = ref(false);
const saving = ref(false);
const showDeleteModal = ref(false);
const deleteConfirmText = ref('');
const deleting = ref(false);

const firstName = ref('');
const lastName = ref('');
const emailAddress = ref('');
const userName = ref('');
const employer = ref('');
const bio = ref('');
const publishedWorkLink = ref('');
const linkedInUrl = ref('');
const twitterUrl = ref('');
const websiteUrl = ref('');

const isFirstNameVisible = ref(true);
const isLastNameVisible = ref(true);
const isEmailVisible = ref(true);

const iconSelection = ref<{ icon: ProfileIcon; color: string }>({
    icon: ProfileIcon.User,
    color: DEFAULT_COLOR
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
            bio.value = result.data.bio || '';
            publishedWorkLink.value = result.data.publishedWorkLink || '';
            linkedInUrl.value = result.data.linkedInUrl || '';
            twitterUrl.value = result.data.twitterUrl || '';
            websiteUrl.value = result.data.websiteUrl || '';
            
            iconSelection.value = {
                icon: (result.data.profileIcon as ProfileIcon) || ProfileIcon.User,
                color: result.data.profileIconColor ? result.data.profileIconColor : DEFAULT_COLOR
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
                employer: employer.value,
                bio: bio.value.trim() || undefined,
                publishedWorkLink: publishedWorkLink.value.trim() || undefined,
                linkedInUrl: linkedInUrl.value.trim() || undefined,
                twitterUrl: twitterUrl.value.trim() || undefined,
                websiteUrl: websiteUrl.value.trim() || undefined
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

const openDeleteModal = () => {
    showDeleteModal.value = true;
    deleteConfirmText.value = '';
};

const closeDeleteModal = () => {
    showDeleteModal.value = false;
    deleteConfirmText.value = '';
};

const deleteAccount = async () => {
    const userId = authStore.userId.value || '';
    
    if (!userId) {
        notification.error('You must be logged in');
        return;
    }
    
    if (deleteConfirmText.value !== 'DELETE MY ACCOUNT') {
        notification.warning('Please type the confirmation text exactly');
        return;
    }
    
    deleting.value = true;
    
    try {
        const result = await services.users.deleteUser(userId);
        
        if (result.isSuccess) {
            notification.success('Your account has been permanently deleted');
            closeDeleteModal();
            
            // Log out and redirect after a short delay
            setTimeout(async () => {
                await authStore.logout();
                router.push('/');
            }, 1500);
        } else {
            notification.error(result.responseMessage || 'Failed to delete account');
        }
    } catch (err) {
        notification.error('An error occurred while deleting your account');
        console.error('Delete account error:', err);
    } finally {
        deleting.value = false;
    }
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

                            <div v-if="isJournalist" class="mb-4">
                                <h5 class="border-bottom pb-2 mb-3">Professional Information</h5>
                                
                                <div class="mb-3">
                                    <label class="form-label">Bio</label>
                                    <textarea 
                                        class="form-control" 
                                        v-model="bio"
                                        rows="4"
                                        maxlength="1000"
                                        placeholder="Tell others about yourself and your work..."
                                    ></textarea>
                                    <small class="text-muted">{{ bio.length }}/1000 characters</small>
                                </div>
                                
                                <div class="mb-3">
                                    <label class="form-label">
                                        <i class="fas fa-file-alt me-1"></i>
                                        Published Work Link
                                    </label>
                                    <input 
                                        type="url" 
                                        class="form-control" 
                                        v-model="publishedWorkLink"
                                        placeholder="https://example.com/my-work"
                                        maxlength="500"
                                    />
                                </div>
                                
                                <div class="row">
                                    <div class="col-md-6 mb-3">
                                        <label class="form-label">
                                            <i class="fab fa-linkedin me-1"></i>
                                            LinkedIn URL
                                        </label>
                                        <input 
                                            type="url" 
                                            class="form-control" 
                                            v-model="linkedInUrl"
                                            placeholder="https://linkedin.com/in/..."
                                            maxlength="500"
                                        />
                                    </div>
                                    <div class="col-md-6 mb-3">
                                        <label class="form-label">
                                            <i class="fab fa-twitter me-1"></i>
                                            Twitter URL
                                        </label>
                                        <input 
                                            type="url" 
                                            class="form-control" 
                                            v-model="twitterUrl"
                                            placeholder="https://twitter.com/..."
                                            maxlength="500"
                                        />
                                    </div>
                                </div>
                                
                                <div class="mb-3">
                                    <label class="form-label">
                                        <i class="fas fa-globe me-1"></i>
                                        Website URL
                                    </label>
                                    <input 
                                        type="url" 
                                        class="form-control" 
                                        v-model="websiteUrl"
                                        placeholder="https://your-website.com"
                                        maxlength="500"
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

                            <div class="d-flex gap-2 justify-content-between">
                                <button 
                                    type="button" 
                                    class="btn btn-danger"
                                    @click="openDeleteModal"
                                    :disabled="saving || deleting"
                                >
                                    <i class="fas fa-trash me-1"></i>
                                    Delete Account
                                </button>
                                
                                <div class="d-flex gap-2">
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
                            </div>
                        </form>
                    </div>
                </div>
                
                <!-- Delete Account Confirmation Modal -->
                <div 
                    v-if="showDeleteModal"
                    class="modal fade show d-block" 
                    tabindex="-1" 
                    style="background-color: var(--modal-backdrop);"
                    @click.self="closeDeleteModal"
                >
                    <div class="modal-dialog modal-dialog-centered">
                        <div class="modal-content">
                            <div class="modal-header bg-danger text-white">
                                <h5 class="modal-title">
                                    <i class="fas fa-exclamation-triangle me-2"></i>
                                    Delete Account Permanently
                                </h5>
                                <button 
                                    type="button" 
                                    class="btn-close btn-close-white" 
                                    @click="closeDeleteModal"
                                    :disabled="deleting"
                                ></button>
                            </div>
                            <div class="modal-body">
                                <div class="alert alert-danger mb-3">
                                    <i class="fas fa-exclamation-circle me-2"></i>
                                    <strong>Warning:</strong> This action cannot be undone!
                                </div>
                                
                                <p class="mb-3">
                                    Deleting your account will permanently remove:
                                </p>
                                <ul class="mb-3">
                                    <li>Your profile and personal information</li>
                                    <li>All your posts and comments</li>
                                    <li>All your chat messages and conversations</li>
                                    <li>All your privacy settings</li>
                                </ul>
                                
                                <p class="fw-bold mb-2">
                                    To confirm deletion, please type: <code>DELETE MY ACCOUNT</code>
                                </p>
                                <input
                                    v-model="deleteConfirmText"
                                    type="text"
                                    class="form-control"
                                    placeholder="Type confirmation text here"
                                    :disabled="deleting"
                                />
                            </div>
                            <div class="modal-footer">
                                <button 
                                    type="button" 
                                    class="btn btn-secondary" 
                                    @click="closeDeleteModal"
                                    :disabled="deleting"
                                >
                                    Cancel
                                </button>
                                <button 
                                    type="button" 
                                    class="btn btn-danger" 
                                    @click="deleteAccount"
                                    :disabled="deleting || deleteConfirmText !== 'DELETE MY ACCOUNT'"
                                >
                                    <span v-if="deleting" class="spinner-border spinner-border-sm me-1"></span>
                                    <i v-else class="fas fa-trash me-1"></i>
                                    {{ deleting ? 'Deleting...' : 'Delete My Account' }}
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

