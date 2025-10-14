export interface ChatBo {
    uid: string;
    user1Uid: string;
    user2Uid: string;
}

export interface CreateChatDto {
    uid?: string;
    user1Uid: string;
    user2Uid: string;
}
