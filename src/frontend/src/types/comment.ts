export interface CommentBo {
    uid: string;
    timestamp: Date;
    creatorUid: string;
    postUid: string;
    content: string;
    displayName: string;
    inEdit: boolean;
}

export interface CreateCommentDto {
    postUid: string;
    creatorUid: string;
    content: string;
}

export interface UpdateCommentDto {
    content: string;
}
