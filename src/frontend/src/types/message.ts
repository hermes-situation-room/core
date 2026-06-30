export interface MessageBo {
    uid: string;
    chatUid: string;
    senderUid: string;
    content: string;
    timestamp: string;
    expiresAt?: string | null;
    isViewOnce?: boolean;
    // True once a view-once message has been consumed for the current user (content is blanked).
    isConsumed?: boolean;
}

export interface CreateMessageDto {
    content: string;
    senderUid: string;
    chatUid: string;
    // Optional disappear timer in seconds; when set the backend computes ExpiresAt = now + this.
    disappearAfterSeconds?: number | null;
    isViewOnce?: boolean;
    // Ids of attachments already uploaded by the sender, to link to the new message.
    attachmentIds?: string[];
}

export interface UpdateMessageDto {
    content: string;
}
