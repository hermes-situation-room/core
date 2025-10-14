<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import faviconUrl from '../assets/logo_situation_room_URL.png'
import logoUrl from '../assets/logo_situation_room.png'
import { RouterLink, useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth-store'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const pageTitle = ref('Posts')

watch(() => route.name, (newRouteName) => {
    pageTitle.value = newRouteName as string || 'Posts'
}, { immediate: true })

onMounted(() => {
    const existing = document.querySelector<HTMLLinkElement>('link[rel="icon"]')
    const link = existing ?? document.createElement('link')
    link.rel = 'icon'
    link.type = 'image/png'
    link.href = faviconUrl
    if (!existing) document.head.appendChild(link)
})

const handleLogout = async () => {
    await authStore.logout()
    await router.push('/login')
}
</script>

<template>
    <div class="sticky-top" style="z-index: 1030;">
        <nav class="navbar" style="padding: 0.5rem 1rem;">
            <div class="container-fluid d-none d-md-flex">
                <div class="d-flex align-items-center justify-content-between w-100">
                    <RouterLink to="/" class="navbar-brand d-flex align-items-center me-3">
                        <img
                            :src="logoUrl"
                            alt="Situation Room logo"
                            class="logo"
                            style="height: 2.5rem; width: auto;"
                        />
                    </RouterLink>

                    <div class="flex-grow-1 text-center d-none d-md-block fw-bold fs-6 fs-lg-5">
                        {{ pageTitle }}
                    </div>
                    <div class="d-md-none fw-bold">
                        {{ pageTitle }}
                    </div>

                    <div class="d-none d-md-flex align-items-center gap-1 gap-lg-2">
                        <template v-if="authStore.isAuthenticated.value">
                            <RouterLink 
                                to="/chats" 
                                class="btn btn-outline-secondary btn-sm d-flex align-items-center"
                            >
                                <i class="fas fa-comments"></i>
                                <span class="d-none d-xl-inline ms-1">Messages</span>
                            </RouterLink>
                            <span class="badge bg-dark text-white px-2 py-1 d-none d-lg-inline">
                                {{ authStore.userType.value === 'journalist' ? 'Journalist' : 'Activist' }}
                            </span>
                            <button 
                                @click="handleLogout"
                                class="btn btn-outline-danger btn-sm d-flex align-items-center"
                            >
                                <i class="fas fa-sign-out-alt"></i>
                                <span class="d-none d-xl-inline ms-1">Logout</span>
                            </button>
                        </template>
                        <template v-else>
                            <RouterLink 
                                to="/chats" 
                                class="btn btn-outline-secondary btn-sm d-flex align-items-center"
                            >
                                <i class="fas fa-sign-in-alt"></i>
                                <span class="d-none d-xl-inline ms-1">Login</span>
                            </RouterLink>
                            <RouterLink 
                                to="/register" 
                                class="btn btn-dark btn-sm d-flex align-items-center"
                            >
                                <i class="fas fa-user-plus"></i>
                                <span class="d-none d-xl-inline ms-1">Register</span>
                            </RouterLink>
                        </template>
                    </div>

                </div>
            </div>
        </nav>
    </div>
</template>
