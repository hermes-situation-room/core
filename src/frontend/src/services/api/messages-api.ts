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
            const url = `${apiBaseClient.apiBaseUrl}services/api/internal/message`;
            try {
                const response = await fetch(url, {
                    method: 'POST',
                    headers: {
                        "Content-Type": "application/json",
                        "Accept": "application/json, text/plain, */*"
                    },
                    body: JSON.stringify(messageData)
                });

                const data = await response.json();

                return {
                    data: data as string,
                    responseCode: response.status,
                    responseMessage: response.statusText,
                    isSuccess: response.ok,
                };
            } catch (e) {
                return {
                    data: undefined,
                    responseCode: undefined,
                    responseMessage: e instanceof Error ? e.message : String(e),
                    isSuccess: false,
                };
            }
        },

        /**
         * Update an existing message
         */
        async updateMessage(messageId: string, messageData: UpdateMessageRequest): Promise<BaseResultBo<undefined>> {
            const url = `${apiBaseClient.apiBaseUrl}services/api/internal/message/${messageId}`;
            try {
                const response = await fetch(url, {
                    method: 'PUT',
                    headers: {
                        "Content-Type": "application/json",
                        "Accept": "application/json, text/plain, */*"
                    },
                    body: JSON.stringify(messageData)
                });

                return {
                    data: undefined,
                    responseCode: response.status,
                    responseMessage: response.statusText,
                    isSuccess: response.ok,
                };
            } catch (e) {
                return {
                    data: undefined,
                    responseCode: undefined,
                    responseMessage: e instanceof Error ? e.message : String(e),
                    isSuccess: false,
                };
            }
        },

        /**
         * Delete a message
         */
        async deleteMessage(messageId: string): Promise<BaseResultBo<undefined>> {
            return await apiBaseClient.delete(`services/api/internal/message/${messageId}`);
        }
    };
}

