export interface CommentBo {
    commentUid: string;
    timestamp: Date;
    creatorUid: string;
    postUid: string;
    content: string;
    displayName: string;
}

export interface CreateCommentDto {
    postUid: string;
    creatorUid: string;
    content: string;
}

export interface UpdateCommentDto {
    content: string;
}
