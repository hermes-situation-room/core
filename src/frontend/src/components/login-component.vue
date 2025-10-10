<script setup lang="ts">
import {computed, ref} from 'vue'
import {RouterLink} from 'vue-router'
import type {UserType} from '../types/user'

const selectedUserType = ref<UserType>('activist')
const loginData = ref({
    username: '',
    email: '',
    password: ''
})

const isJournalist = computed(() => selectedUserType.value === 'journalist')
const isActivist = computed(() => selectedUserType.value === 'activist')

function selectUserType(type: UserType) {
    selectedUserType.value = type

    loginData.value = {
        username: '',
        email: '',
        password: ''
    }
}

function handleLogin() {
    console.log('Login attempt:', {
        userType: selectedUserType.value,
        ...loginData.value
    })
    // TODO: Implement actual login logic
}
</script>

<template>
    <div class="bg-light d-flex align-items-center justify-content-center" style="min-height: calc(100vh - 80px);">
        <div class="container">
            <div class="row justify-content-center">
                <div class="col-12 col-md-8 col-lg-6 col-xl-5" style="padding: 2rem 0;">
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
                                    {{ isJournalist ? 'Journalist Login' : 'Activist Login' }}
                                </h3>
                                <p class="text-muted small">
                                    {{
                                        isJournalist ? 'Login with your email and password' : 'Login with your username and password'
                                    }}
                                </p>
                            </div>

                            <form @submit.prevent="handleLogin">
                                <div v-if="isActivist" class="mb-3">
                                    <label class="form-label">Username</label>
                                    <input
                                        v-model="loginData.username"
                                        type="text"
                                        required
                                        class="form-control"
                                        placeholder="Enter your username"
                                    />
                                </div>

                                <div v-if="isJournalist" class="mb-3">
                                    <label class="form-label">Email Address</label>
                                    <input
                                        v-model="loginData.email"
                                        type="email"
                                        required
                                        class="form-control"
                                        placeholder="Enter your email address"
                                    />
                                </div>

                                <div class="mb-4">
                                    <label class="form-label">Password</label>
                                    <input
                                        v-model="loginData.password"
                                        type="password"
                                        required
                                        class="form-control"
                                        placeholder="Enter your password"
                                    />
                                </div>

                                <button type="submit" class="btn btn-dark w-100 mb-3">
                                    {{ isJournalist ? 'Login as Journalist' : 'Login as Activist' }}
                                </button>
                            </form>

                            <div class="text-center">
                                <p class="text-muted small mb-2">
                                    Don't have an account?
                                    <br/>
                                    <RouterLink to="/register" class="text-primary">
                                        Register here
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