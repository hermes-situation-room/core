import type {BaseResultBo} from "../models/bo/base-result-bo";
import type {MessageBo, CreateMessageRequest, UpdateMessageRequest} from "../../types/chat";
import ApiBaseClient from "./base/api-base-client";

export default function messagesApi(apiBaseClient: ApiBaseClient) {
    return {
        /**
         * Get all messages for a specific chat
         */
        async getMessagesByChat(chatId: string): Promise<BaseResultBo<MessageBo[]>> {
            return await apiBaseClient.get<MessageBo[]>(`services/api/internal/message/get-messages-by-chat/${chatId}`);
        },

        /**
         * Get a specific message by ID
         */
        async getMessageById(messageId: string): Promise<BaseResultBo<MessageBo>> {
            return await apiBaseClient.get<MessageBo>(`services/api/internal/message/${messageId}`);
        },

        /**
         * Create a new message
         */
        async createMessage(messageData: CreateMessageRequest): Promise<BaseResultBo<string>> {
            return await apiBaseClient.post<CreateMessageRequest>(`services/api/internal/message`, messageData);
        },

        /**
         * Update an existing message
         */
        async updateMessage(messageId: string, messageData: UpdateMessageRequest): Promise<BaseResultBo<string>> {
            return await apiBaseClient.put<UpdateMessageRequest>(`services/api/internal/message/${messageId}`, messageData);
        },

        /**
         * Delete a message
         */
        async deleteMessage(messageId: string): Promise<BaseResultBo<undefined>> {
            return await apiBaseClient.delete(`services/api/internal/message/${messageId}`);
        }
    };
}

