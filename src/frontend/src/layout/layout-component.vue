<script setup lang="ts">
import { onUnmounted, ref, watch } from "vue";
import { useAuthStore } from "../stores/auth-store";
import NavbarComponent from "./navbar-component.vue";
import { services, sockets } from "../services/api";
import ToastContainer from "./toast-container.vue";

const authStore = useAuthStore();

const totalUnreadMessages = ref(0);
const isSocketConnected = ref(false);

watch(authStore.isAuthenticated, async (_) => {
    if (authStore.isAuthenticated) {
        try {
            await sockets.hub.initialize();
            sockets.hub.registerToEvent("NewTotalUnreadChatMessage", 
            (newTotal: number) => {
                totalUnreadMessages.value = newTotal;
            });
            sockets.hub.joinMessaging();
            isSocketConnected.value = true;
        } catch (error) {
            console.warn('Failed to connect to real-time messaging:', error);
            isSocketConnected.value = false;
        }
        
        const userId = authStore.userId;
        await checkUnreadMessages(userId.value!);
    }
}, {immediate: true});

const checkUnreadMessages = async (userId: string) => {
    try {
        const res = await services.userChatMessageStatus.getTotalUnreadMessages(userId);
        if (res.isSuccess && res.data) {
            totalUnreadMessages.value = res.data.countUnreadMessages;
        }
    } catch (error) {
        console.warn('Failed to fetch unread message count:', error);
    }
}


window.addEventListener('beforeunload', () => {
    if (isSocketConnected.value) {
        sockets.hub.leaveMessaging();
    }
})

onUnmounted(() => {
    if (isSocketConnected.value) {
        sockets.hub.leaveMessaging();
    }
})
</script>

<template>
    <div id="app">
        <navbarComponent :total-unread-messages="totalUnreadMessages" />
        <ToastContainer/>
        <main>
            <router-view />
        </main>
    </div>
</template>
