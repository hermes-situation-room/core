<script setup lang="ts">
import {computed, ref} from 'vue'
import {RouterLink, useRouter} from 'vue-router'
import type {LoginFormData, UserType} from '../types/user.ts'
import {useAuthStore} from '../stores/auth-store'
import {services} from "../services/api";
import {useNotification} from '../composables/use-notification.ts';
import ProfileIconSelector from './profile-icon-selector.vue';
import ProfileIconDisplay from './profile-icon-display.vue';
import {ProfileIcon, DEFAULT_COLOR} from '../types/profile-icon.ts';

const router = useRouter()
const authStore = useAuthStore()
const notification = useNotification()

const selectedUserType = ref<UserType>('activist')
const formData = ref<LoginFormData>({
    userType: 'activist',
    userName: '',
    password: '',
    firstName: '',
    lastName: '',
    emailAddress: '',
    employer: '',
    isFirstNameVisible: true,
    isLastNameVisible: true,
    isEmailVisible: true,
    profileIcon: ProfileIcon.User,
    profileIconColor: DEFAULT_COLOR
})
const isLoading = ref(false)
const iconSelection = ref({
    icon: ProfileIcon.User,
    color: DEFAULT_COLOR
})
const profileIconModalRef = ref<HTMLDivElement | null>(null)

const isJournalist = computed(() => selectedUserType.value === 'journalist')
const isActivist = computed(() => selectedUserType.value === 'activist')

const showPrivacySettings = computed(() => {
    return isActivist.value && (
        formData.value.firstName ||
        formData.value.lastName ||
        formData.value.emailAddress
    )
})

function selectUserType(type: UserType) {
    selectedUserType.value = type
    formData.value.userType = type

    formData.value = {
        userType: type,
        userName: '',
        password: '',
        firstName: '',
        lastName: '',
        emailAddress: '',
        employer: '',
        isFirstNameVisible: true,
        isLastNameVisible: true,
        isEmailVisible: true,
        profileIcon: ProfileIcon.User,
        profileIconColor: DEFAULT_COLOR
    }
    
    iconSelection.value = {
        icon: ProfileIcon.User,
        color: DEFAULT_COLOR
    }
}

function updateIconSelection() {
    formData.value.profileIcon = iconSelection.value.icon
    formData.value.profileIconColor = iconSelection.value.color
    
    const closeButton = profileIconModalRef.value?.querySelector('[data-bs-dismiss="modal"]') as HTMLButtonElement
    if (closeButton) {
        closeButton.click()
    }
}

async function handleRegister() {
    if (isLoading.value) return

    isLoading.value = true

    try {
        let result;

        if (isActivist.value) {
            result = await services.auth.registerActivist({
                userName: formData.value.userName,
                password: formData.value.password,
                firstName: formData.value.firstName || undefined,
                lastName: formData.value.lastName || undefined,
                emailAddress: formData.value.emailAddress || undefined,
                isFirstNameVisible: formData.value.isFirstNameVisible ?? true,
                isLastNameVisible: formData.value.isLastNameVisible ?? true,
                isEmailVisible: formData.value.isEmailVisible ?? true,
                profileIcon: formData.value.profileIcon || ProfileIcon.User,
                profileIconColor: formData.value.profileIconColor || DEFAULT_COLOR
            })
        } else {
            if (!formData.value.firstName || !formData.value.lastName ||
                !formData.value.emailAddress || !formData.value.employer) {
                notification.warning('All fields are required for journalist registration.')
                isLoading.value = false
                return
            }

            result = await services.auth.registerJournalist({
                firstName: formData.value.firstName,
                lastName: formData.value.lastName,
                emailAddress: formData.value.emailAddress,
                password: formData.value.password,
                employer: formData.value.employer,
                profileIcon: formData.value.profileIcon || ProfileIcon.User,
                profileIconColor: formData.value.profileIconColor || DEFAULT_COLOR
            })
        }

        if (result.isSuccess && result.data) {
            authStore.login()
            notification.success('Registration successful!');
            if (isJournalist.value) {
                await router.push('/login?type=journalist')
            } else {
                await router.push('/login?type=activist')
            }
        } else {
            notification.error(result.responseMessage || 'Registration failed. Please try again.')
        }
    } catch (error) {
        console.error('Registration error:', error)
        notification.error('An error occurred during registration. Please try again.')
    } finally {
        isLoading.value = false
    }
}
</script>

<template>
    <div class="bg-light d-flex align-items-center justify-content-center" style="min-height: calc(100vh - 80px);">
        <div class="container">
            <div class="row justify-content-center">
                <div class="col-12 col-md-10 col-lg-8 col-xl-6" style="padding: 2rem 0;">
                    <div class="card mb-4">
                        <div class="card-body text-center">
                            <h2 class="h4 mb-4">Choose Your Role</h2>
                            <div class="btn-group w-100" role="group">
                                <button
                                    @click="selectUserType('journalist')"
                                    type="button"
                                    :class="['btn', isJournalist ? 'btn-dark' : 'btn-outline-dark']"
                                >
                                    Journalist
                                </button>
                                <button
                                    @click="selectUserType('activist')"
                                    type="button"
                                    :class="['btn', isActivist ? 'btn-dark' : 'btn-outline-dark']"
                                >
                                    Activist
                                </button>
                            </div>
                        </div>
                    </div>

                    <div class="card">
                        <div class="card-body">
                            <div class="text-center mb-4">
                                <h3 class="h5 mb-2">
                                    {{ isJournalist ? 'Journalist Registration' : 'Activist Registration' }}
                                </h3>
                                <p class="text-muted small">
                                    Create your {{ isJournalist ? 'journalist' : 'activist' }} account
                                </p>
                            </div>

                            <form @submit.prevent="handleRegister">
                                <div class="mb-3">
                                    <label class="form-label">Profile Icon</label>
                                    <div class="d-flex align-items-center gap-3">
                                        <ProfileIconDisplay 
                                            :icon="formData.profileIcon" 
                                            :color="formData.profileIconColor" 
                                            size="md" 
                                        />
                                        <button 
                                            type="button" 
                                            class="btn btn-outline-secondary btn-sm"
                                            data-bs-toggle="modal"
                                            data-bs-target="#profileIconModal"
                                            :disabled="isLoading"
                                        >
                                            <i class="fas fa-edit me-1"></i>
                                            Edit Profile Icon
                                        </button>
                                    </div>
                                </div>

                                <div v-if="isActivist" class="mb-3">
                                    <label class="form-label">Username</label>
                                    <input
                                        v-model="formData.userName"
                                        type="text"
                                        required
                                        class="form-control"
                                        placeholder="Choose a username"
                                        :disabled="isLoading"
                                    />
                                </div>

                                <div v-if="isJournalist" class="mb-3">
                                    <label class="form-label">Email Address</label>
                                    <input
                                        v-model="formData.emailAddress"
                                        type="email"
                                        required
                                        class="form-control"
                                        placeholder="Enter your email"
                                        :disabled="isLoading"
                                    />
                                </div>

                                <div class="mb-3">
                                    <label class="form-label">Password</label>
                                    <input
                                        v-model="formData.password"
                                        type="password"
                                        required
                                        class="form-control"
                                        placeholder="Choose a password"
                                        :disabled="isLoading"
                                    />
                                </div>

                                <div v-if="!isJournalist" class="mb-3">
                                    <label class="form-label">Email Address</label>
                                    <input
                                        v-model="formData.emailAddress"
                                        type="email"
                                        class="form-control"
                                        placeholder="Optional"
                                        :disabled="isLoading"
                                    />
                                </div>

                                <div class="row">
                                    <div class="col-md-6">
                                        <div class="mb-3">
                                            <label class="form-label">First Name</label>
                                            <input
                                                v-model="formData.firstName"
                                                type="text"
                                                :required="isJournalist"
                                                class="form-control"
                                                :placeholder="isJournalist ? 'Enter your first name' : 'Optional'"
                                                :disabled="isLoading"
                                            />
                                        </div>
                                    </div>
                                    <div class="col-md-6">
                                        <div class="mb-3">
                                            <label class="form-label">Last Name</label>
                                            <input
                                                v-model="formData.lastName"
                                                type="text"
                                                :required="isJournalist"
                                                class="form-control"
                                                :placeholder="isJournalist ? 'Enter your last name' : 'Optional'"
                                                :disabled="isLoading"
                                            />
                                        </div>
                                    </div>
                                </div>

                                <div v-if="isJournalist" class="mb-3">
                                    <label class="form-label">Employer</label>
                                    <input
                                        v-model="formData.employer"
                                        type="text"
                                        required
                                        class="form-control"
                                        placeholder="Enter your employer"
                                        :disabled="isLoading"
                                    />
                                </div>

                                <div v-if="showPrivacySettings" class="card bg-light mb-4">
                                    <div class="card-body">
                                        <div class="text-center mb-3">
                                            <h6 class="card-title">Privacy Settings</h6>
                                        </div>

                                        <div v-if="formData.firstName" class="form-check mb-2">
                                            <input
                                                v-model="formData.isFirstNameVisible"
                                                type="checkbox"
                                                class="form-check-input"
                                                id="firstNameVisible"
                                            />
                                            <label class="form-check-label small" for="firstNameVisible">
                                                Make first name visible
                                            </label>
                                        </div>

                                        <div v-if="formData.lastName" class="form-check mb-2">
                                            <input
                                                v-model="formData.isLastNameVisible"
                                                type="checkbox"
                                                class="form-check-input"
                                                id="lastNameVisible"
                                            />
                                            <label class="form-check-label small" for="lastNameVisible">
                                                Make last name visible
                                            </label>
                                        </div>

                                        <div v-if="formData.emailAddress" class="form-check">
                                            <input
                                                v-model="formData.isEmailVisible"
                                                type="checkbox"
                                                class="form-check-input"
                                                id="emailVisible"
                                            />
                                            <label class="form-check-label small" for="emailVisible">
                                                Make email visible
                                            </label>
                                        </div>
                                    </div>
                                </div>

                                <button type="submit" class="btn btn-dark w-100 mb-3" :disabled="isLoading">
                                    <span v-if="isLoading">
                                        <span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                                        Registering...
                                    </span>
                                    <span v-else>
                                        Register as {{ isJournalist ? 'Journalist' : 'Activist' }}
                                    </span>
                                </button>
                            </form>

                            <div class="text-center">
                                <p class="text-muted small mb-2">
                                    Already have an account?
                                    <br/>
                                    <RouterLink to="/login" class="text-primary">
                                        Login here
                                    </RouterLink>
                                </p>
                                <RouterLink to="/" class="text-muted small">
                                    ← Back to Dashboard
                                </RouterLink>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div ref="profileIconModalRef" class="modal fade" id="profileIconModal" tabindex="-1" aria-labelledby="profileIconModalLabel" aria-hidden="true">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="profileIconModalLabel">Choose Your Profile Icon</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <ProfileIconSelector v-model="iconSelection" />
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                        <button type="button" class="btn btn-primary" @click="updateIconSelection">Select Icon</button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
