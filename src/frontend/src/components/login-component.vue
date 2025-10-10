<script setup lang="ts">
import { computed, ref } from 'vue'
import { RouterLink } from 'vue-router'
import type { UserType } from '../types/user'

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
    <div class="container-fluid d-flex justify-content-center p-4">
        <div class="col-12 col-md-8 col-lg-6 col-xl-5 mx-auto">
            <div class="card shadow-lg mb-4">
                <div class="card-body p-4">
                    <h2 class="text-center mb-4 font-weight-bold">Choose Your Role</h2>

                    <div class="btn-group w-100" role="group">
                        <button @click="selectUserType('journalist')" type="button" :class="[
                            'btn btn-outline-primary',
                            isJournalist ? 'active' : ''
                        ]">
                            Journalist
                        </button>
                        <button @click="selectUserType('activist')" type="button" :class="[
                            'btn btn-outline-success',
                            isActivist ? 'active' : ''
                        ]">
                            Activist
                        </button>
                    </div>
                </div>
            </div>

            <div class="card shadow-lg">
                <div class="card-body p-4">
                    <div class="text-center mb-4">
                        <h3 class="font-weight-bold mb-0">
                            {{ isJournalist ? 'Journalist Login' : 'Activist Login' }}
                        </h3>
                        <p class="text-muted mt-2">
                            {{
                                isJournalist
                                    ? 'Login with your email and password'
                                    : 'Login with your username and password'
                            }}
                        </p>
                    </div>

                    <form @submit.prevent="handleLogin">
                        <div v-if="isActivist" class="form-group mb-3">
                            <label class="form-label font-weight-bold">Username *</label>
                            <input v-model="loginData.username" type="text" required
                                class="form-control form-control-lg" placeholder="Enter your username" />
                        </div>

                        <div v-if="isJournalist" class="form-group mb-3">
                            <label class="form-label font-weight-bold">Email Address *</label>
                            <input v-model="loginData.email" type="email" required class="form-control form-control-lg"
                                placeholder="Enter your email address" />
                        </div>

                        <div class="form-group mb-4">
                            <label class="form-label font-weight-bold">Password *</label>
                            <input v-model="loginData.password" type="password" required
                                class="form-control form-control-lg" placeholder="Enter your password" />
                        </div>

                        <button type="submit" :class="[
                            'btn btn-lg btn-block font-weight-bold',
                            isJournalist ? 'btn-primary' : 'btn-success'
                        ]">
                            {{ isJournalist ? 'Login as Journalist' : 'Login as Activist' }}
                        </button>
                    </form>

                    <div class="text-center mt-4">
                        <p class="mb-2">
                            Don't have an account?
                            <RouterLink to="/register" class="text-primary text-decoration-none font-weight-bold">
                                Register here
                            </RouterLink>
                        </p>
                        <RouterLink to="/" class="text-muted text-decoration-none">
                            ← Back to Dashboard
                        </RouterLink>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>