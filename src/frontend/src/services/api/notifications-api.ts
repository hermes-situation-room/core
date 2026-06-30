import type {BaseResultBo} from "../models/bo/base-result-bo";
import ApiBaseClient from "./base/api-base-client";
import type {NotificationBo} from "../../types/notification";

export default function notificationsApi(apiBaseClient: ApiBaseClient) {
    return {
        /**
         * Get all notifications for a user
         */
        async getByUserUid(userUid: string): Promise<BaseResultBo<NotificationBo[]>> {
            return await apiBaseClient.get<NotificationBo[]>(`services/api/internal/notification/user/${userUid}`);
        },

        /**
         * Get unread notifications for a user
         */
        async getUnreadByUserUid(userUid: string): Promise<BaseResultBo<NotificationBo[]>> {
            return await apiBaseClient.get<NotificationBo[]>(`services/api/internal/notification/user/${userUid}/unread`);
        },

        /**
         * Get unread notification count for a user
         */
        async getUnreadCount(userUid: string): Promise<BaseResultBo<number>> {
            return await apiBaseClient.get<number>(`services/api/internal/notification/user/${userUid}/unread/count`);
        },

        /**
         * Get notification by ID
         */
        async getById(uid: string): Promise<BaseResultBo<NotificationBo>> {
            return await apiBaseClient.get<NotificationBo>(`services/api/internal/notification/${uid}`);
        },

        /**
         * Mark notification as read
         */
        async markAsRead(uid: string): Promise<BaseResultBo<string>> {
            return await apiBaseClient.put<undefined>(`services/api/internal/notification/${uid}/read`, undefined);
        },

        /**
         * Mark all notifications as read for a user
         */
        async markAllAsRead(userUid: string): Promise<BaseResultBo<string>> {
            return await apiBaseClient.put<undefined>(`services/api/internal/notification/user/${userUid}/read-all`, undefined);
        },

        /**
         * Delete a notification
         */
        async delete(uid: string): Promise<BaseResultBo<undefined>> {
            return await apiBaseClient.delete(`services/api/internal/notification/${uid}`);
        }
    };
}
