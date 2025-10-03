<script setup lang="ts">
import { onMounted } from 'vue'
import logoUrl from '../assets/logo_situation_room.png'
import { RouterLink } from 'vue-router'

onMounted(() => {
    const existing = document.querySelector<HTMLLinkElement>('link[rel="icon"]')
    const link = existing ?? document.createElement('link')
    link.rel = 'icon'
    link.type = 'image/png'
    link.href = logoUrl
    if (!existing) document.head.appendChild(link)
})
</script>

<template>
    <nav class="navbar bg-light py-3">
        <div class="container-fluid">
            <RouterLink to="/" class="navbar-brand d-flex align-items-center fs-2">
                <img :src="logoUrl" alt="Situation Room logo" height="56" class="me-2" />
                <span>Situation Room</span>
            </RouterLink>
        </div>
    </nav>
    <hr class="m-0" />
</template>