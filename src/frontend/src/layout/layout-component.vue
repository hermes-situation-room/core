<script setup lang="ts">
import { onBeforeUnmount, ref, watch } from "vue";
import { useAuthStore } from "../stores/auth-store";
import NavbarComponent from "./navbar-component.vue";
import { services, sockets } from "../services/api";

const authStore = useAuthStore();

const totalUnreadMessages = ref(0);

watch(authStore.isAuthenticated, async (_) => {
    if (authStore.isAuthenticated) {
        await sockets.hub.initialize();
        sockets.hub.joinMessaging();
        sockets.hub.registerToEvent("NewTotalUnreadChatMessage", 
        (newTotal: number) => {
            totalUnreadMessages.value = newTotal;
        });
        const userId = authStore.userId;
        await checkUnreadMessages(userId.value!);
    }
}, {immediate: true});

const checkUnreadMessages = async (userId: string) => {
    const res = await services.userChatMessageStatus.getTotalUnreadMessages(userId);
    if (res.isSuccess && res.data) {
        totalUnreadMessages.value = res.data.countUnreadMessages;
    }
}


window.addEventListener('beforeunload', () => {
    sockets.hub.leaveMessaging();
})

onBeforeUnmount(() => {
    sockets.hub.leaveMessaging();
})
</script>

<template>
    <div id="app">
        <navbarComponent :total-unread-messages="totalUnreadMessages" />
        <main>
            <router-view />
        </main>
    </div>
</template>
