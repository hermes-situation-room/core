<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import faviconUrl from '../assets/logo_situation_room_URL.png'
import logoUrl from '../assets/logo_situation_room.png'
import { RouterLink, useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth-store'
import rButton from '../components/controls/r-button.vue'

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
    <nav class="navbar desktop row">
        <div class="col-md-4 image-wrapper">
            <RouterLink to="/" class="logo-image">
                <img :src="logoUrl" alt="Situation Room logo" class="logo" style="height: 2.5rem; width: auto;" />
            </RouterLink>
        </div>

        <div class="col-md-4 page-title">
            {{ pageTitle }}
        </div>

        <div class="col-md-4 user-actions">
            <template v-if="authStore.isAuthenticated.value">
                <!-- <RouterLink to="/chats" class="btn plain-primary">
                    <i class="fas fa-comments"></i>
                    <span class="">Messages</span>
                </RouterLink> -->
                <r-button
                    label="Messages"
                    icon="comments"
                    link="/chats"
                    classes="primary"
                ></r-button>
                <span class="badge">
                    {{ authStore.userType.value === 'journalist' ? 'Journalist' : 'Activist' }}
                </span>
                <r-button
                    label="Logout"
                    icon="sign-out-alt"
                    @click="handleLogout"
                    class="plain-primary"
                ></r-button>
            </template>
            <template v-else>
                <RouterLink to="/chats" class="btn">
                    <i class="fas fa-sign-in-alt"></i>
                    <span class="">Login</span>
                </RouterLink>
                <RouterLink to="/register" class="btn ">
                    <i class="fas fa-user-plus"></i>
                    <span class="">Register</span>
                </RouterLink>
            </template>
        </div>
    </nav>
</template>
