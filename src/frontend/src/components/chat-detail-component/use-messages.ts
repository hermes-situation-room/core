import { ref } from "vue";
import type { CreateMessageDto, MessageBo } from "../../types/message";
import { services } from "../../services/api";

export function useMessages(messages: any, currentUserUid: any, chat: any) {
    const newMessage = ref("");
    const sending = ref(false);
    const editingMessageId = ref<string | null>(null);
    const editingContent = ref("");

    const sendMessage = async (event: SubmitEvent) => {
        event.preventDefault();
        if (!newMessage.value.trim()) return;

        sending.value = true;
        const messageContent = newMessage.value.trim();
        newMessage.value = "";

        try {
            const messageData: CreateMessageDto = {
                content: messageContent,
                senderUid: currentUserUid.value,
                chatUid: chat.value?.uid || "",
            };

            const result = await services.messages.createMessage(messageData);
            if (!result.isSuccess) {
                console.error("Failed to send message:", result.responseMessage);
            }
        } catch (error) {
            console.error("Failed to send message:", error);
        } finally {
            sending.value = false;
        }
    };

    const startEditMessage = (message: MessageBo) => {
        if (message.senderUid === currentUserUid.value) {
            editingMessageId.value = message.uid;
            editingContent.value = message.content;
        }
    };

    const saveEdit = async (messageId: string) => {
        if (!editingContent.value.trim()) return;

        try {
            const result = await services.messages.updateMessage(messageId, { content: editingContent.value.trim() });
            if (result.isSuccess) {
                const index = messages.value.findIndex((m: any) => m.uid === messageId);
                if (index !== -1) {
                    messages.value[index].content = editingContent.value.trim();
                }
                editingMessageId.value = null;
                editingContent.value = "";
            }
        } catch (error) {
            console.error("Failed to edit message:", error);
        }
    };

    const deleteMessage = async (messageId: string) => {
        try {
            const result = await services.messages.deleteMessage(messageId);
            if (result.isSuccess) {
                messages.value = messages.value.filter((m: any) => m.uid !== messageId);
            }
        } catch (error) {
            console.error("Failed to delete message:", error);
        }
    };

    const isMyMessage = (message: MessageBo) => {
        return message.senderUid === currentUserUid.value;
    };

    const shouldShowEditMode = (message: MessageBo) => {
        return editingMessageId.value === message.uid;
    };

    const cancelEdit = () => {
        editingMessageId.value = null;
        editingContent.value = "";
    };

    return {
        newMessage,
        sending,
        editingMessageId,
        editingContent,
        sendMessage,
        startEditMessage,
        saveEdit,
        deleteMessage,
        isMyMessage,
        shouldShowEditMode,
        cancelEdit,
    };
}
