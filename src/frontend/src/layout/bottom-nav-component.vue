<script setup lang="ts">
import { useRoute, RouterLink } from 'vue-router'
import { useAuthStore } from '../stores/auth-store'

const route = useRoute()
const authStore = useAuthStore()

const isActive = (path: string) => {
  return route.path === path || route.path.startsWith(path + '/')
}
</script>

<template>
  <nav class="bottom-nav">
    <RouterLink 
      to="/" 
      :class="['nav-item', { active: isActive('/') && !isActive('/chats') }]"
    >
      <i class="fas fa-home"></i>
      <span>Home</span>
    </RouterLink>
    
    <RouterLink 
      to="/journalist" 
      :class="['nav-item', { active: isActive('/journalist') }]"
    >
      <i class="fas fa-newspaper"></i>
      <span>Journalist</span>
    </RouterLink>
    
    <RouterLink 
      to="/activist" 
      :class="['nav-item', { active: isActive('/activist') }]"
    >
      <i class="fas fa-bullhorn"></i>
      <span>Activist</span>
    </RouterLink>
    
    <RouterLink 
      to="/chats" 
      :class="['nav-item', { active: isActive('/chats') || isActive('/chat') }]"
    >
      <i class="fas fa-comments"></i>
      <span>Chats</span>
    </RouterLink>
    
    <RouterLink 
      v-if="authStore.isAuthenticated.value"
      to="/profile" 
      :class="['nav-item', { active: isActive('/profile') }]"
    >
      <i class="fas fa-user-circle"></i>
      <span>Profile</span>
    </RouterLink>
    
    <RouterLink 
      v-else
      to="/login" 
      :class="['nav-item', { active: isActive('/login') || isActive('/register') }]"
    >
      <i class="fas fa-sign-in-alt"></i>
      <span>Login</span>
    </RouterLink>
  </nav>
</template>

