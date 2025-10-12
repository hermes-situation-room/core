export interface ChatBo {
    uid: string;
    user1Uid: string;
    user2Uid: string;
}

export interface CreateChatRequest {
    uid?: string;
    user1Uid: string;
    user2Uid: string;
}

export interface MessageBo {
    uid: string;
    chatUid: string;
    senderUid: string;
    content: string;
    timestamp: string;
}

export interface CreateMessageRequest {
    content: string;
    senderUid: string;
    chatUid: string;
}

export interface UpdateMessageRequest {
    content: string;
}

