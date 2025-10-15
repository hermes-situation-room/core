<script setup lang="ts">
import { computed, watch, onMounted, ref } from "vue";
import { useAuthStore } from "../stores/auth-store";
import NavbarComponent from "./navbar-component.vue";
import { services, sockets } from "../services/api";
import ToastContainer from "./toast-container.vue";

const authStore = useAuthStore();

const totalUnreadMessages = ref(0);
const isSocketConnected = ref(false);
const isAuthenticated = computed(() => authStore.isAuthenticated.value && !!authStore.userId.value);

const initSocket = async () => {
  if (isSocketConnected.value) return;

  try {
    await sockets.hub.initialize();

    await sockets.hub.registerToEvent("NewTotalUnreadChatMessage", (newTotal: number) => {
      totalUnreadMessages.value = newTotal;
    });

    await sockets.hub.joinMessaging();
    isSocketConnected.value = true;

    const userId = authStore.userId;
    if (userId?.value) await checkUnreadMessages(userId.value);
  } catch (error) {
    console.warn("Failed to connect to real-time messaging:", error);
    isSocketConnected.value = false;
  }
};

const checkUnreadMessages = async (userId: string) => {
  try {
    const res = await services.userChatMessageStatus.getTotalUnreadMessages(userId);
    if (res.isSuccess && res.data) {
      totalUnreadMessages.value = res.data.countUnreadMessages;
    }
  } catch (error) {
    console.warn("Failed to fetch unread message count:", error);
  }
};

const cleanupSocket = async () => {
  if (!isSocketConnected.value) return;
  try {
    await sockets.hub.leaveMessaging();
  } catch (e) {
    console.warn("Error leaving messaging:", e);
  } finally {
    sockets.hub.flush();
    isSocketConnected.value = false;
  }
};

watch(
  isAuthenticated,
  async (newVal, oldVal) => {
    if (newVal && !oldVal) {
      await initSocket();
    } else if (!newVal && oldVal) {
      await cleanupSocket();
    }
  },
  { immediate: false }
);

onMounted(async () => {
  if (isAuthenticated.value) {
    await initSocket();
  }
});

window.addEventListener("beforeunload", cleanupSocket);
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
