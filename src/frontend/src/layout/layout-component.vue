<script setup lang="ts">
import NavbarComponent from "./navbar-component.vue";
import ErrorToast from "../components/shared/error-toast.vue";
import SuccessToast from "../components/shared/success-toast.vue";
</script>

<template>
    <div id="app">
        <navbarComponent />
        <error-toast />
        <success-toast />
        <main>
            <router-view />
        </main>
    </div>
</template>
