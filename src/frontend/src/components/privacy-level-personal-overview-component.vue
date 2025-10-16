<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { services } from '../services/api';
import { useAuthStore } from '../stores/auth-store';
import { useNotification } from '../composables/use-notification';
import type { UserProfileBo } from '../types/user';
import type { PrivacyLevel } from '../types/privacy-level';
import type { CreatePrivacyLevelPersonalDto, PrivacyLevelPersonalBo, UpdatePrivacyLevelPersonalDto } from '../types/privacy-level-personal';

interface Props {
    userProfile: UserProfileBo
}

const props = defineProps<Props>()
const authStore = useAuthStore();
const notification = useNotification();
const loadingPrivacyLevels = ref(false)
const editingPrivacy = ref(false)
const updatingPrivacy = ref(false)
const existingPersonalPrivacyLevel = ref(false)

const ownerId = computed(() => authStore.userId.value || '');
const consumerId = props.userProfile.uid;
const defaultPrivacyLevel = ref<PrivacyLevel>({
    isFirstNameVisible: false,
    isLastNameVisible: false,
    isEmailVisible: false
});
const personalPrivacyLevel = ref<PrivacyLevelPersonalBo>({
    uid: "",
    isFirstNameVisible: undefined,
    isLastNameVisible: undefined,
    isEmailVisible: undefined,
    ownerUid: "",
    consumerUid: ""
});

const loadPrivacyLevels = async () => {
    loadingPrivacyLevels.value = true;

    const defaultPrivacyResult = await loadDefaultPrivacyLevel();
    const personalPrivacyResult = await loadPrivacyLevelPersonal();

    defaultPrivacyLevel.value = defaultPrivacyResult || defaultPrivacyLevel.value;
    personalPrivacyLevel.value = personalPrivacyResult || personalPrivacyLevel.value;

    if (defaultPrivacyLevel) {
        existingPersonalPrivacyLevel.value = true
    }

    loadingPrivacyLevels.value = false
}

const loadPrivacyLevelPersonal = async () => {
    try {
    const result = await services.privacyLevelPersonal.getPrivacyLevelPersonal(ownerId.value, consumerId);
    if (result.isSuccess && result.data) {
        return result.data;
    } else if (result.responseCode == 404) {
        return null
    } else {
        notification.error(result.responseMessage || 'Failed to load personal privacy levels');
        return null;
    }
    } catch (err) {
        notification.error('Error loading personal privacy levels');
        return null;
    }
};

const loadDefaultPrivacyLevel = async () => {
  try {
    const result = await services.users.getActivistPrivacy(ownerId.value);
    if (result.isSuccess && result.data) {
      return result.data;
    } else {
      notification.error(result.responseMessage || 'Failed to load default privacy levels');
      return null;
    }
  } catch (err) {
    notification.error('Error loading default privacy levels');
    return null;
  }
};

const togglePrivacyEdit = () => {
    editingPrivacy.value = !editingPrivacy.value;
}

const updatePrivacy = async () => {
    try {
        let result

        if (existingPersonalPrivacyLevel && personalPrivacyLevel.value.isEmailVisible || personalPrivacyLevel.value.isFirstNameVisible || personalPrivacyLevel.value.isLastNameVisible){
            const updatePrivacyLevelPersonalData: UpdatePrivacyLevelPersonalDto = {
                isFirstNameVisible: personalPrivacyLevel.value.isFirstNameVisible,
                isLastNameVisible: personalPrivacyLevel.value.isLastNameVisible,
                isEmailVisible: personalPrivacyLevel.value.isEmailVisible
            }
            result = await services.privacyLevelPersonal.updatePrivacyLevelPersonal(personalPrivacyLevel.value.uid, updatePrivacyLevelPersonalData);
        }
        if (existingPersonalPrivacyLevel && !personalPrivacyLevel.value.isEmailVisible && !personalPrivacyLevel.value.isFirstNameVisible && !personalPrivacyLevel.value.isLastNameVisible){
            result = await services.privacyLevelPersonal.deletePrivacyLevelPersonal(personalPrivacyLevel.value.uid);
        }else{
            const createPrivacyLevelPersonalData: CreatePrivacyLevelPersonalDto = {
                isFirstNameVisible: personalPrivacyLevel.value.isFirstNameVisible,
                isLastNameVisible: personalPrivacyLevel.value.isLastNameVisible,
                isEmailVisible: personalPrivacyLevel.value.isEmailVisible,
                ownerUid: ownerId.value,
                consumerUid: consumerId
            }
            result = await services.privacyLevelPersonal.createPrivacyLevelPersonal(createPrivacyLevelPersonalData);
        }

        if (!result.isSuccess) {
            notification.error(result.responseMessage || 'Failed to update personal privacy settings');
            return;
        }

        notification.updated('Personal privacy settings updated successfully!');
    } catch (err) {
        notification.error('Error saving personal privacy settings');
    } finally {
        updatingPrivacy.value = false;
        editingPrivacy.value = false;
    }
}

onMounted(() => {
    loadPrivacyLevels()    
});
</script>
<template>
    <div>
        <form @submit.prevent="updatePrivacy" class="mb-4">
            <h5 class="border-bottom pb-2 mb-3">Personal Privacy Settings</h5>
            <p class="text-muted small mb-3">
                <i class="fas fa-shield-alt me-1"></i>
                Control what information is visible to this User.
            </p>

            <div v-if="!defaultPrivacyLevel.isFirstNameVisible" class="mb-3">
                <div class="form-check form-switch">
                    <input 
                        :disabled="!editingPrivacy"
                        class="form-check-input" 
                        type="checkbox" 
                        id="firstNameVisible"
                        v-model="personalPrivacyLevel.isFirstNameVisible"
                    />
                    <label class="form-check-label" for="firstNameVisible">
                        Show First Name to this User
                    </label>
                </div>
            </div>

            <div v-if="!defaultPrivacyLevel.isLastNameVisible" class="mb-3">
                <div class="form-check form-switch">
                    <input 
                        :disabled="!editingPrivacy"
                        class="form-check-input" 
                        type="checkbox" 
                        id="lastNameVisible"
                        v-model="personalPrivacyLevel.isLastNameVisible"
                    />
                    <label class="form-check-label" for="lastNameVisible">
                        Show Last Name to this User
                    </label>
                </div>
            </div>

            <div v-if="!defaultPrivacyLevel.isEmailVisible" class="mb-3">
                <div class="form-check form-switch">
                    <input 
                        :disabled="!editingPrivacy"
                        class="form-check-input" 
                        type="checkbox" 
                        id="emailVisible"
                        v-model="personalPrivacyLevel.isEmailVisible"
                    />
                    <label class="form-check-label" for="emailVisible">
                        Show Email to this User
                    </label>
                </div>
            </div>

            <div 
            v-if="defaultPrivacyLevel.isEmailVisible || defaultPrivacyLevel.isFirstNameVisible || defaultPrivacyLevel.isLastNameVisible"
            class="alert alert-info"
            >
                <i class="fas fa-lightbulb me-2"></i>
                <small>Some of your Properties are set to public. This means anyone can see them. You can change this in your User Settings</small>
            </div>
            
            <div v-if="editingPrivacy" class="d-flex gap-2 justify-content-end">
                <button 
                    type="button" 
                    class="btn btn-secondary"
                    @click="togglePrivacyEdit"
                    :disabled="updatingPrivacy"
                >
                    <i class="fas fa-times me-1"></i>
                    Cancel
                </button>
                <button 
                    type="submit" 
                    class="btn btn-primary"
                    :disabled="updatingPrivacy"
                >
                    <span v-if="updatingPrivacy" class="spinner-border spinner-border-sm me-1"></span>
                    <i v-else class="fas fa-save me-1"></i>
                    Save Changes
                </button>
            </div>
            <div v-else class="d-flex gap-2 justify-content-end">
                <button type="button" class="btn btn-primary" @click="togglePrivacyEdit"><i class="fas fa-edit me-1"></i> Edit Privacy Settings for this User</button>
            </div>
        </form>
    </div>
</template>