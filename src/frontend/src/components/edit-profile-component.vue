<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { services } from '../services/api';
import type { ActivistBo, JournalistBo, UserProfileBo } from '../types/user';
import { useAuthStore } from '../stores/auth-store';
import { useErrorStore } from "../stores/error-store.ts";
import { useNotifications } from "../composables/use-notifications.ts";

const router = useRouter();
const authStore = useAuthStore();

const userProfile = ref<UserProfileBo | null>(null);
const activistPrivacy = ref<ActivistBo | null>(null);
const loading = ref(false);
const saving = ref(false);
const error = useErrorStore();
const { showUpdateSuccess } = useNotifications();

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

const canShowEditForm = computed(() => {
    return userProfile.value != null && !hasLoadingError();
});

const hasLoadingError = () => {
    const notifications = error.getActiveNotifications();
    return notifications.some(notification =>
        notification.error.category === 'server' ||
        notification.error.category === 'unknown' ||
        notification.error.category === 'not_found'
    );
};

const hasValidationErrors = () => {
    const notifications = error.getActiveNotifications();
    return notifications.some(notification =>
        notification.error.category === 'validation' ||
        notification.error.category === 'conflict' ||
        notification.error.category === 'authentication'
    );
};

const loadProfile = async () => {
    const userId = authStore.userId.value || '';

    if (!userId) {
        error.addError({ category: 'validation', message: 'You must be logged in to edit your profile' });
        router.push('/login');
        return;
    }

    loading.value = true;
    error.clearAll();
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
            error.addError({
                category: 'unknown',
                message: result.responseMessage || 'Failed to load profile'
            });
        }
    } catch (err) {
        error.addError({
            category: 'server',
            message: 'Error loading profile'
        });
    } finally {
        loading.value = false;
    }
};

const saveProfile = async () => {
    const userId = authStore.userId.value || '';

    if (!userId) {
        error.addError({
            category: 'authentication',
            message: 'You must be logged in'
        });
        return;
    }

    if (isJournalist.value) {
        if (!firstName.value.trim() || !lastName.value.trim() || !emailAddress.value.trim()) {
            error.addError({
                category: 'validation',
                message: 'First Name, Last Name, and Email are required for journalists'
            });
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
            error.addError({
                category: 'conflict',
                message: 'Unable to determine user type'
            });
            saving.value = false;
            return;
        }

        if (!result.isSuccess) {
            error.addError({
                category: 'server',
                message: result.responseMessage || 'Failed to update profile'
            });
            saving.value = false;
            return;
        }

        showUpdateSuccess('Profile updated successfully!');
        setTimeout(() => {
            router.push({ path: '/profile', query: { id: userId } });
        }, 1500);
    } catch (err) {
        error.addError({
            category: 'unknown',
            message: 'Error saving profile'
        });
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