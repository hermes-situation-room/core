export interface MessageBo {
    uid: string;
    chatUid: string;
    senderUid: string;
    content: string;
    timestamp: string;
}

export interface CreateMessageDto {
    content: string;
    senderUid: string;
    chatUid: string;
}

export interface UpdateMessageDto {
    content: string;
}
