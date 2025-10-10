export interface PostBo {
    uid: string;
    timestamp: string;
    title: string;
    description: string;
    content: string;
    creatorUid: string;
    tags: string[];
}

export interface CreatePostRequest {
    title: string;
    description: string;
    content: string;
    creatorUid: string;
    tags: string[];
}

export interface UpdatePostRequest {
    uid: string;
    timestamp: string;
    title: string;
    description: string;
    content: string;
    creatorUid: string;
    tags: string[];
}

export interface PostFilter {
    category?: 'activist' | 'journalist';
    tags?: string[];
    userUid?: string;
}

export interface ErrorResponse {
    type: string;
    title: string;
    status: number;
    detail: string;
    instance: string;
    errors?: Record<string, string[]>;
}
