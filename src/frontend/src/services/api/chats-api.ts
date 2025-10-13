import type {BaseResultBo} from "../models/bo/base-result-bo";
import type {ChatBo, CreateChatDto} from "../../types/chat";
import ApiBaseClient from "./base/api-base-client";

export default function chatsApi(apiBaseClient: ApiBaseClient) {
    return {
        /**
         * Get all chats for a specific user
         */
        async getChatsByUser(userId: string): Promise<BaseResultBo<ChatBo[]>> {
            return await apiBaseClient.get<ChatBo[]>(`services/api/internal/chats/by-user/${userId}`);
        },

        /**
         * Get a specific chat by chat ID
         */
        async getChatById(chatId: string): Promise<BaseResultBo<ChatBo>> {
            return await apiBaseClient.get<ChatBo>(`services/api/internal/chats/${chatId}`);
        },

        /**
         * Get chat between two specific users
         */
        async getChatByUserPair(user1Id: string, user2Id: string): Promise<BaseResultBo<ChatBo>> {
            const params = {
                user1Id: user1Id,
                user2Id: user2Id
            };
            return await apiBaseClient.get<ChatBo>('services/api/internal/chats/by-user-pair', params);
        },

        /**
         * Create a new chat
         */
        async createChat(chatData: CreateChatDto): Promise<BaseResultBo<string>> {
            return await apiBaseClient.post<CreateChatDto>('services/api/internal/chats', chatData);
        },

        /**
         * Delete a chat
         */
        async deleteChat(chatId: string): Promise<BaseResultBo<undefined>> {
            return await apiBaseClient.delete(`services/api/internal/chats/${chatId}`);
        }
    };
}

