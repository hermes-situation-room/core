export interface ChatBo {
    uid: string;
    user1Uid: string;
    user2Uid: string;
    memberUids: string[];
    name?: string;
    isGroup: boolean;
}

export interface CreateChatDto {
    uid?: string;
    user1Uid: string;
    user2Uid: string;
}

export interface CreateGroupChatDto {
    memberUids: string[];
    name?: string;
}
