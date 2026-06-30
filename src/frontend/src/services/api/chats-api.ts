import type {BaseResultBo} from "../models/bo/base-result-bo";
import type {ChatBo, CreateChatDto, CreateGroupChatDto} from "../../types/chat";
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
         * Get existing chat or create new one between two users
         * This method will never return 404 - it creates the chat if it doesn't exist
         */
        async getOrCreateChatByUserPair(user1Id: string, user2Id: string): Promise<BaseResultBo<ChatBo>> {
            const createChatDto = {
                user1Uid: user1Id,
                user2Uid: user2Id
            };

            const response = await apiBaseClient.post<CreateChatDto>('services/api/internal/chats', createChatDto);
            const chatId = response.data;

            return await apiBaseClient.get<ChatBo>(`services/api/internal/chats/${chatId}`);
        },

        /**
         * Create a new chat
         */
        async createChat(chatData: CreateChatDto): Promise<BaseResultBo<string>> {
            return await apiBaseClient.post<CreateChatDto>('services/api/internal/chats', chatData);
        },

        /**
         * Create a new group chat with an arbitrary number of members.
         * Returns the new chat, fetched after creation.
         */
        async createGroupChat(memberUids: string[], name?: string): Promise<BaseResultBo<ChatBo>> {
            const createGroupChatDto: CreateGroupChatDto = {
                memberUids: memberUids,
                name: name
            };

            const response = await apiBaseClient.post<CreateGroupChatDto>('services/api/internal/group-chats', createGroupChatDto);
            const chatId = response.data;

            return await apiBaseClient.get<ChatBo>(`services/api/internal/chats/${chatId}`);
        },

        /**
         * Delete a chat
         */
        async deleteChat(chatId: string): Promise<BaseResultBo<undefined>> {
            return await apiBaseClient.delete(`services/api/internal/chats/${chatId}`);
        }
    };
}

