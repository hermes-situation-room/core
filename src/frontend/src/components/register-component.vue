<script setup lang="ts">
import { computed, ref } from 'vue'
import { RouterLink } from 'vue-router'
import type { LoginFormData, UserType } from '../types/user'

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
    isEmailVisible: true
})

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
        isEmailVisible: true
    }
}

function handleRegister() {
    console.log('Register attempt:', formData.value)
    // TODO: Implement actual registration logic
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
                                    <label class="form-label">Username</label>
                                    <input
                                        v-model="formData.userName"
                                        type="text"
                                        required
                                        class="form-control"
                                        placeholder="Choose a username"
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
                                    />
                                </div>

                                <template v-if="isJournalist">
                                    <div class="mb-3">
                                        <label class="form-label">First Name</label>
                                        <input
                                            v-model="formData.firstName"
                                            type="text"
                                            required
                                            class="form-control"
                                            placeholder="Enter your first name"
                                        />
                                    </div>

                                    <div class="mb-3">
                                        <label class="form-label">Last Name</label>
                                        <input
                                            v-model="formData.lastName"
                                            type="text"
                                            required
                                            class="form-control"
                                            placeholder="Enter your last name"
                                        />
                                    </div>

                                    <div class="mb-3">
                                        <label class="form-label">Email Address</label>
                                        <input
                                            v-model="formData.emailAddress"
                                            type="email"
                                            required
                                            class="form-control"
                                            placeholder="Enter your email"
                                        />
                                    </div>

                                    <div class="mb-4">
                                        <label class="form-label">Employer</label>
                                        <input
                                            v-model="formData.employer"
                                            type="text"
                                            required
                                            class="form-control"
                                            placeholder="Enter your employer"
                                        />
                                    </div>
                                </template>

                                <template v-if="isActivist">
                                    <div class="row">
                                        <div class="col-md-6">
                                            <div class="mb-3">
                                                <label class="form-label">First Name</label>
                                                <input
                                                    v-model="formData.firstName"
                                                    type="text"
                                                    class="form-control"
                                                    placeholder="Optional"
                                                />
                                            </div>
                                        </div>
                                        <div class="col-md-6">
                                            <div class="mb-3">
                                                <label class="form-label">Last Name</label>
                                                <input
                                                    v-model="formData.lastName"
                                                    type="text"
                                                    class="form-control"
                                                    placeholder="Optional"
                                                />
                                            </div>
                                        </div>
                                    </div>

                                    <div class="mb-3">
                                        <label class="form-label">Email Address</label>
                                        <input
                                            v-model="formData.emailAddress"
                                            type="email"
                                            class="form-control"
                                            placeholder="Optional"
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
                                </template>

                                <button type="submit" class="btn btn-dark w-100 mb-3">
                                    {{ isJournalist ? 'Register as Journalist' : 'Register as Activist' }}
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
    </div>
</template>
