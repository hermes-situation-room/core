<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import faviconUrl from '../assets/logo_situation_room_URL.png'
import logoUrl from '../assets/logo_situation_room.png'
import { RouterLink, useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth-store'

const props = defineProps({
    totalUnreadMessages: {
        type: Number,
        required: false
    },
});

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const pageTitle = ref('Posts')
const showMobileMenu = ref(false)

watch(() => route.name, (newRouteName) => {
    pageTitle.value = newRouteName as string || 'Posts'
}, { immediate: true })

watch(() => route.path, () => {
    showMobileMenu.value = false
})

onMounted(() => {
    const existing = document.querySelector<HTMLLinkElement>('link[rel="icon"]')
    const link = existing ?? document.createElement('link')
    link.rel = 'icon'
    link.type = 'image/png'
    link.href = faviconUrl
    if (!existing) document.head.appendChild(link)
})

const toggleMobileMenu = () => {
    showMobileMenu.value = !showMobileMenu.value
}

const closeMobileMenu = () => {
    showMobileMenu.value = false
}

const viewUser = () => {
    const uid = authStore.userId.value || '';
    router.push({ 
        name: 'Profile', 
        query: {
            id: uid,
        }
    });
};

const goToPosts = () => {
    router.push('/');
};

const handleLogout = async () => {
    await authStore.logout()
    await router.push('/login')
}
</script>

<template>
    <div class="sticky-top" style="background-color: #F5F5F5; z-index: 1030;">
        <nav class="navbar py-2 px-3" style="background-color: #F5F5F5;">
            <div class="container-fluid">
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
                            <button 
                                @click="goToPosts"
                                class="btn btn-outline-secondary btn-sm d-flex align-items-center"
                            >
                                <i class="fas fa-newspaper"></i>
                                <span class="d-none d-xl-inline ms-1">Posts</span>
                            </button>
                            <div class="messages">
                                <RouterLink 
                                    to="/chats" 
                                    class="btn btn-outline-secondary btn-sm d-flex align-items-center"
                                >
                                    <i class="fas fa-comments"></i>
                                    <span class="d-none d-xl-inline ms-1">Messages</span>
                                </RouterLink>
                                <span class="unreadMessages" :class="['badge', 'bg-dark', 'text-white', 'px-2', 'py-1', { 'd-none': !props.totalUnreadMessages }]">
                                    {{ props.totalUnreadMessages }}
                                </span>
                            </div>
                            <button 
                                @click="viewUser"
                                class="btn btn-outline-secondary btn-sm d-flex align-items-center"
                            >
                                <i class="fas fa-user"></i>
                                <span class="d-none d-xl-inline ms-1">Profile</span>
                            </button>
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

                    <button 
                        class="btn btn-link text-dark d-md-none p-0"
                        @click="toggleMobileMenu"
                        type="button"
                    >
                        <i class="fas fa-bars fs-4"></i>
                    </button>
                </div>
            </div>
        </nav>
        <hr style="border: none; height: 1px; background-color: #000; margin: 0;" />

        <div 
            :class="['offcanvas', 'offcanvas-end', { 'show': showMobileMenu }]"
            tabindex="-1"
            :style="{ visibility: showMobileMenu ? 'visible' : 'hidden' }"
        >
            <div class="offcanvas-header">
                <h5 class="offcanvas-title">Menu</h5>
                <button 
                    type="button" 
                    class="btn-close"
                    @click="closeMobileMenu"
                ></button>
            </div>
            <div class="offcanvas-body">
                <div class="list-group list-group-flush">
                    <template v-if="authStore.isAuthenticated.value">
                        <div class="list-group-item bg-light">
                            <div class="d-flex align-items-center justify-content-between">
                                <span class="fw-bold">Logged in as:</span>
                                <span class="badge bg-dark">
                                    {{ authStore.userType.value === 'journalist' ? 'Journalist' : 'Activist' }}
                                </span>
                            </div>
                        </div>
                        <button 
                            @click="goToPosts(); closeMobileMenu()"
                            class="list-group-item list-group-item-action d-flex align-items-center gap-3"
                        >
                            <i class="fas fa-newspaper fs-5"></i>
                            <span>Posts</span>
                        </button>
                        <RouterLink 
                            to="/chats" 
                            class="list-group-item list-group-item-action d-flex align-items-center gap-3"
                            @click="closeMobileMenu"
                        >
                            <i class="fas fa-comments fs-5"></i>
                            <span>Messages</span>
                        </RouterLink>
                        <button 
                            @click="viewUser(); closeMobileMenu()"
                            class="list-group-item list-group-item-action d-flex align-items-center gap-3"
                        >
                            <i class="fas fa-user fs-5"></i>
                            <span>Profile</span>
                        </button>
                        <button 
                            @click="handleLogout"
                            class="list-group-item list-group-item-action d-flex align-items-center gap-3 text-danger"
                        >
                            <i class="fas fa-sign-out-alt fs-5"></i>
                            <span>Logout</span>
                        </button>
                    </template>
                    <template v-else>
                        <RouterLink 
                            to="/chats" 
                            class="list-group-item list-group-item-action d-flex align-items-center gap-3"
                            @click="closeMobileMenu"
                        >
                            <i class="fas fa-sign-in-alt fs-5"></i>
                            <span>Login</span>
                        </RouterLink>
                        <RouterLink 
                            to="/register" 
                            class="list-group-item list-group-item-action d-flex align-items-center gap-3"
                            @click="closeMobileMenu"
                        >
                            <i class="fas fa-user-plus fs-5"></i>
                            <span>Register</span>
                        </RouterLink>
                    </template>
                </div>
            </div>
        </div>

        <div 
            v-if="showMobileMenu"
            class="offcanvas-backdrop fade show"
            @click="closeMobileMenu"
        ></div>
    </div>
</template>

<style scoped lang="scss">
.messages {
    position: relative;
    .unreadMessages {
        position: absolute;
        top: -10px;
        right: -10px;
        border-radius: 50px;
        text-decoration: none;
    }
}
</style>