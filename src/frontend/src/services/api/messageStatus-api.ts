import type { UserChatMessageStatus } from "../../types/user-chat-message-status";
import type { BaseResultBo } from "../models/bo/base-result-bo";
import type ApiBaseClient from "./base/api-base-client";

export default function messageStatusApi(apiBaseClient: ApiBaseClient) {
    return {
        async getTotalUnreadMessages(userId: string): Promise<BaseResultBo<UserChatMessageStatus>> {
            return await apiBaseClient.get<UserChatMessageStatus>("services/api/internal/userChatReadStatus/total", {userId: userId});
        },
        async getUnreadMessageCountPerChat(): Promise<BaseResultBo<UserChatMessageStatus[]>> {
            return await apiBaseClient.get<UserChatMessageStatus[]>("services/api/internal/userChatReadStatus/byChat");
        }
    }
}