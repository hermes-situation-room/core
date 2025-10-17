export interface CommentBo {
    uid: string;
    timestamp: string;
    creatorUid: string;
    postUid: string;
    content: string;
    profileIconColor: string;
    profileIcon: string;
    displayName: string;
    inEdit: boolean;
    parentCommentUid?: string;
    replies?: CommentBo[];
    isReply?: boolean;
}

export interface CreateCommentDto {
    postUid: string;
    creatorUid: string;
    content: string;
    parentCommentUid?: string;
}

export interface UpdateCommentDto {
    content: string;
}
