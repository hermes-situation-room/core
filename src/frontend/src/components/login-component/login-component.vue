<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink } from 'vue-router'
import { useUserType } from './use-user-type';
import { useLogin } from './use-login';

const { isJournalist, isActivist, selectUserType } = useUserType();
const { loginData, errorMessage, isLoading, handleLogin } = useLogin();

const loginTitle = computed(() => (isJournalist.value ? 'Journalist Login' : 'Activist Login'));
const loginDescription = computed(() =>
    isJournalist.value
        ? 'Login with your email and password'
        : 'Login with your username and password'
);
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
                                <button @click="selectUserType('journalist')" type="button"
                                    :class="['btn', isJournalist ? 'btn-dark' : 'btn-outline-dark']">
                                    Journalist
                                </button>
                                <button @click="selectUserType('activist')" type="button"
                                    :class="['btn', isActivist ? 'btn-dark' : 'btn-outline-dark']">
                                    Activist
                                </button>
                            </div>
                        </div>
                    </div>

                    <div class="card">
                        <div class="card-body">
                            <div class="text-center mb-4">
                                <h3 class="h5 mb-2">
                                    {{ loginTitle }}
                                </h3>
                                <p class="text-muted small">
                                    {{ loginDescription }}
                                </p>
                            </div>

                            <form @submit.prevent="handleLogin(isActivist)">
                                <div v-if="errorMessage" class="alert alert-danger" role="alert">
                                    {{ errorMessage }}
                                </div>

                                <div v-if="isActivist" class="mb-3">
                                    <label class="form-label">Username</label>
                                    <input v-model="loginData.username" type="text" required class="form-control"
                                        placeholder="Enter your username" :disabled="isLoading" />
                                </div>

                                <div v-if="isJournalist" class="mb-3">
                                    <label class="form-label">Email Address</label>
                                    <input v-model="loginData.email" type="email" required class="form-control"
                                        placeholder="Enter your email address" :disabled="isLoading" />
                                </div>

                                <div class="mb-4">
                                    <label class="form-label">Password</label>
                                    <input v-model="loginData.password" type="password" required class="form-control"
                                        placeholder="Enter your password" :disabled="isLoading" />
                                </div>

                                <button type="submit" class="btn btn-dark w-100 mb-3" :disabled="isLoading">
                                    <span v-if="isLoading">
                                        <span class="spinner-border spinner-border-sm me-2" role="status"
                                            aria-hidden="true"></span>
                                        Logging in...
                                    </span>
                                    <span v-else>
                                        {{ isJournalist ? 'Login as Journalist' : 'Login as Activist' }}
                                    </span>
                                </button>
                            </form>

                            <div class="text-center">
                                <p class="text-muted small mb-2">
                                    Don't have an account?
                                    <br />
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