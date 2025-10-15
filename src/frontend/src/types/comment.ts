export interface CommentBo {
    commentUid: string;
    postUid: string;
    creatorUid: string;
    content: string;
    timeStamp: Date;
}

export interface CreateCommentDto {
    postUid: string;
    creatorUid: string;
    content: string;
}

export interface UpdateCommentDto {
    content: string;
}
