<script setup lang="ts">
import {computed, ref} from 'vue'
import {RouterLink} from 'vue-router'
import type {LoginFormData, UserType} from '../types/user'

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
    <div class="container-fluid d-flex justify-content-center p-4">
        <div class="col-12 col-md-8 col-lg-6 col-xl-5 mx-auto">
            <div class="card shadow-lg mb-4">
                <div class="card-body p-4">
                    <h2 class="text-center mb-4 font-weight-bold">Choose Your Role</h2>

                    <div class="btn-group w-100" role="group">
                        <button
                            @click="selectUserType('journalist')"
                            type="button"
                            :class="[
                                'btn btn-outline-primary',
                                isJournalist ? 'active' : ''
                            ]"
                        >
                            Journalist
                        </button>
                        <button
                            @click="selectUserType('activist')"
                            type="button"
                            :class="[
                                'btn btn-outline-success',
                                isActivist ? 'active' : ''
                            ]"
                        >
                            Activist
                        </button>
                    </div>
                </div>
            </div>

            <div class="card shadow-lg">
                <div class="card-body p-4">
                    <div class="text-center mb-4">
                        <h3 class="font-weight-bold mb-0">
                            {{ isJournalist ? 'Journalist Registration' : 'Activist Registration' }}
                        </h3>
                        <p class="text-muted mt-2">
                            Create your {{ isJournalist ? 'journalist' : 'activist' }} account
                        </p>
                    </div>

                    <form @submit.prevent="handleRegister">
                        <!-- Username -->
                        <div class="form-group mb-3">
                            <label class="form-label font-weight-bold">Username *</label>
                            <input
                                v-model="formData.userName"
                                type="text"
                                required
                                class="form-control form-control-lg"
                                placeholder="Choose a username"
                            />
                        </div>

                        <div class="form-group mb-3">
                            <label class="form-label font-weight-bold">Password *</label>
                            <input
                                v-model="formData.password"
                                type="password"
                                required
                                class="form-control form-control-lg"
                                placeholder="Choose a password"
                            />
                        </div>

                        <template v-if="isJournalist">
                            <div class="form-group mb-3">
                                <label class="form-label font-weight-bold">First Name *</label>
                                <input
                                    v-model="formData.firstName"
                                    type="text"
                                    required
                                    class="form-control form-control-lg"
                                    placeholder="Enter your first name"
                                />
                            </div>

                            <div class="form-group mb-3">
                                <label class="form-label font-weight-bold">Last Name *</label>
                                <input
                                    v-model="formData.lastName"
                                    type="text"
                                    required
                                    class="form-control form-control-lg"
                                    placeholder="Enter your last name"
                                />
                            </div>

                            <div class="form-group mb-3">
                                <label class="form-label font-weight-bold">Email Address *</label>
                                <input
                                    v-model="formData.emailAddress"
                                    type="email"
                                    required
                                    class="form-control form-control-lg"
                                    placeholder="Enter your email"
                                />
                            </div>

                            <div class="form-group mb-4">
                                <label class="form-label font-weight-bold">Employer *</label>
                                <input
                                    v-model="formData.employer"
                                    type="text"
                                    required
                                    class="form-control form-control-lg"
                                    placeholder="Enter your employer"
                                />
                            </div>
                        </template>

                        <template v-if="isActivist">
                            <div class="row mb-3">
                                <div class="col-md-6">
                                    <div class="form-group">
                                        <label class="form-label font-weight-bold">First Name</label>
                                        <input
                                            v-model="formData.firstName"
                                            type="text"
                                            class="form-control form-control-lg"
                                            placeholder="Optional"
                                        />
                                    </div>
                                </div>
                                <div class="col-md-6">
                                    <div class="form-group">
                                        <label class="form-label font-weight-bold">Last Name</label>
                                        <input
                                            v-model="formData.lastName"
                                            type="text"
                                            class="form-control form-control-lg"
                                            placeholder="Optional"
                                        />
                                    </div>
                                </div>
                            </div>

                            <div class="form-group mb-4">
                                <label class="form-label font-weight-bold">Email Address</label>
                                <input
                                    v-model="formData.emailAddress"
                                    type="email"
                                    class="form-control form-control-lg"
                                    placeholder="Optional"
                                />
                            </div>

                            <div v-if="showPrivacySettings" class="bg-light rounded p-3 mb-4">
                                <h5 class="font-weight-bold mb-3">Privacy Settings</h5>
                                <div v-if="formData.firstName" class="form-check mb-2">
                                    <input
                                        v-model="formData.isFirstNameVisible"
                                        type="checkbox"
                                        class="form-check-input"
                                        id="firstNameVisible"
                                    />
                                    <label class="form-check-label" for="firstNameVisible">
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
                                    <label class="form-check-label" for="lastNameVisible">
                                        Make last name visible
                                    </label>
                                </div>
                                <div v-if="formData.emailAddress" class="form-check mb-3">
                                    <input
                                        v-model="formData.isEmailVisible"
                                        type="checkbox"
                                        class="form-check-input"
                                        id="emailVisible"
                                    />
                                    <label class="form-check-label" for="emailVisible">
                                        Make email visible
                                    </label>
                                </div>
                            </div>
                        </template>

                        <button
                            type="submit"
                            :class="[
                                'btn btn-lg btn-block font-weight-bold',
                                isJournalist ? 'btn-primary' : 'btn-success'
                            ]"
                        >
                            {{ isJournalist ? 'Register as Journalist' : 'Register as Activist' }}
                        </button>
                    </form>

                    <div class="text-center mt-4">
                        <p class="mb-2">
                            Already have an account?
                            <RouterLink
                                to="/login"
                                class="text-primary text-decoration-none font-weight-bold"
                            >
                                Login here
                            </RouterLink>
                        </p>
                        <RouterLink
                            to="/"
                            class="text-muted text-decoration-none"
                        >
                            ← Back to Dashboard
                        </RouterLink>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
