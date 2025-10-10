<script setup lang="ts">
import NavbarComponent from "./navbar-component.vue";
</script>

<template>
    <div id="app">
        <navbarComponent />
        <main>
            <router-view />
        </main>
    </div>
</template>
