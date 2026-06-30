export interface NotificationBo {
    uid: string;
    userUid: string;
    title: string;
    message: string;
    type: string;
    isRead: boolean;
    createdAt: string;
    relatedEntityUid?: string;
    relatedEntityType?: string;
}

export interface CreateNotificationDto {
    userUid: string;
    title: string;
    message: string;
    type: string;
    relatedEntityUid?: string;
    relatedEntityType?: string;
}

export interface NotificationStats {
    unreadCount: number;
    hasUnread: boolean;
}
