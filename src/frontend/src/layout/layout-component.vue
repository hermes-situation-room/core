<script setup lang="ts">
import NavbarComponent from "./navbar-component.vue";
import BottomNavComponent from "./bottom-nav-component.vue";
</script>

<template>
    <div id="app">
        <NavbarComponent />
        <main>
            <router-view />
        </main>
        <BottomNavComponent />
    </div>
</template>
