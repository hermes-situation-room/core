import { ref, onMounted, onUnmounted } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useAuthStore } from "../../stores/auth-store";
import type { ChatBo } from "../../types/chat";
import type { MessageBo } from "../../types/message";
import { services, sockets } from "../../services/api";

export function useChat() {
    const route = useRoute();
    const router = useRouter();
    const authStore = useAuthStore();

    const chat = ref<ChatBo | null>(null);
    const messages = ref<MessageBo[]>([]);
    const loading = ref(false);
    const loadingMessages = ref(false);
    const errorMessage = ref("");
    const currentUserUid = ref(authStore.userId.value || "");

    const clearError = () => {
        errorMessage.value = "";
    };

    const showError = (message: string) => {
        errorMessage.value = message;
        setTimeout(clearError, 5000);
    };

    const loadChat = async () => {
        loading.value = true;
        try {
            const chatId = route.params.id as string;

            if (!currentUserUid.value) {
                showError("You must be logged in to view chats");
                router.push("/chats");
                return;
            }

            const result = await services.chats.getChatById(chatId);
            if (result.isSuccess && result.data) {
                chat.value = result.data;
                await loadMessages(chatId);
                await sockets.hub.ensureSocketInitialization();
                sockets.hub.joinChat(currentUserUid.value, chatId);
                sockets.hub.registerToEvent("ReceiveMessage", handleIncomingMessage);
                sockets.hub.registerToEvent("UpdateMessage", handleMessageUpdate);
                sockets.hub.registerToEvent("DeleteMessage", handleMessageDelete);
            } else if (result.responseCode === 404) {
                showError("Chat not found");
                router.push("/chats");
            }
        } catch (error) {
            showError("Error loading chat");
        } finally {
            loading.value = false;
        }
    };

    const loadMessages = async (chatId: string) => {
        loadingMessages.value = true;
        try {
            const result = await services.messages.getMessagesByChat(chatId);
            if (result.isSuccess && result.data) {
                messages.value = result.data.sort(
                    (a, b) => new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime()
                );
            }
        } catch (error) {
            showError("Error loading messages");
        } finally {
            loadingMessages.value = false;
        }
    };

    const handleIncomingMessage = (message: MessageBo) => {
        if (!chat.value || message.chatUid !== chat.value.uid) return;

        const existingIndex = messages.value.findIndex((m) => m.uid === message.uid);
        if (existingIndex !== -1) {
            messages.value[existingIndex] = message;
        } else {
            messages.value.push(message);
        }
    };

    const handleMessageUpdate = (message: MessageBo) => {
        const index = messages.value.findIndex((m) => m.uid === message.uid);
        if (index !== -1) {
            messages.value[index] = message;
        }
    };

    const handleMessageDelete = (messageId: string) => {
        messages.value = messages.value.filter((m) => m.uid !== messageId);
    };

    const getOtherUserUid = () => {
        if (!chat.value) return "";
        return chat.value.user1Uid === currentUserUid.value ? chat.value.user2Uid : chat.value.user1Uid;
    };

    onMounted(() => {
        loadChat();
    });

    onUnmounted(() => {
        if (chat.value) {
            sockets.hub.leaveChat(chat.value.uid);
        }
    });

    return {
        chat,
        messages,
        loading,
        loadingMessages,
        errorMessage,
        loadChat,
        loadMessages,
        handleIncomingMessage,
        handleMessageUpdate,
        handleMessageDelete,
        showError,
        clearError,
        getOtherUserUid,
    };
}
