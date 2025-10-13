<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import faviconUrl from '../assets/logo_situation_room_URL.png'
import logoUrl from '../assets/logo_situation_room.png'
import { RouterLink, useRoute } from 'vue-router'

const route = useRoute()
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
                        <RouterLink 
                            to="/login" 
                            class="btn btn-outline-secondary btn-sm d-flex align-items-center"
                        >
                            <i class="fas fa-users"></i>
                            <span class="d-none d-xl-inline ms-1">Search Users</span>
                        </RouterLink>
                        <RouterLink 
                            to="/chats" 
                            class="btn btn-outline-secondary btn-sm d-flex align-items-center"
                        >
                            <i class="fas fa-comments"></i>
                            <span class="d-none d-xl-inline ms-1">Messages</span>
                        </RouterLink>
                        <RouterLink 
                            to="/login" 
                            class="btn btn-outline-secondary btn-sm d-flex align-items-center"
                        >
                            <i class="fas fa-user-circle"></i>
                            <span class="d-none d-xl-inline ms-1">Profile</span>
                        </RouterLink>
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
                    <RouterLink 
                        to="/login" 
                        class="list-group-item list-group-item-action d-flex align-items-center gap-3"
                        @click="closeMobileMenu"
                    >
                        <i class="fas fa-users fs-5"></i>
                        <span>Search Users</span>
                    </RouterLink>
                    <RouterLink 
                        to="/chats" 
                        class="list-group-item list-group-item-action d-flex align-items-center gap-3"
                        @click="closeMobileMenu"
                    >
                        <i class="fas fa-comments fs-5"></i>
                        <span>Messages</span>
                    </RouterLink>
                    <RouterLink 
                        to="/login" 
                        class="list-group-item list-group-item-action d-flex align-items-center gap-3"
                        @click="closeMobileMenu"
                    >
                        <i class="fas fa-user-circle fs-5"></i>
                        <span>Profile</span>
                    </RouterLink>
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
