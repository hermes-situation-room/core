<script setup lang="ts">
import hermesNavbarComponent from './components/hermes-navbar-component.vue';
</script>

<template>
    <div id="app">
        <hermesNavbarComponent/>
        <main>
            <router-view/>
        </main>
    </div>
</template>
