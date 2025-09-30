<script setup lang="ts">
import { onMounted } from 'vue'
import logoUrl from './assets/logo_situation_room.png'

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
      <a class="navbar-brand d-flex align-items-center fs-2" href="#">
        <img :src="logoUrl" alt="Situation Room logo" height="56" class="me-2" />
        <span>Situation Room</span>
      </a>
    </div>
  </nav>
  <hr class="m-0" />
</template>
