import type {BaseResultBo} from "../models/bo/base-result-bo";
import ApiBaseClient from "./base/api-base-client";
import type {CreateCommentDto, CommentBo, UpdateCommentDto} from "../../types/comment.ts";

export default function commentsApi(apiBaseClient: ApiBaseClient) {
    return {
        /**
         * Get all comments for a specific post
         */
        async getCommentsByPost(postId: string): Promise<BaseResultBo<CommentBo[]>> {
            return await apiBaseClient.get<CommentBo[]>(`services/api/internal/post/${postId}/comment`);
        },

        /**
         * Create a new comment
         */
        async createComment(commentData: CreateCommentDto): Promise<BaseResultBo<string>> {
            return await apiBaseClient.post<CreateCommentDto>(`services/api/internal/comment`, commentData);
        },

        /**
         * Update an existing comment
         */
        async updateComment(commentId: string, commentData: UpdateCommentDto): Promise<BaseResultBo<string>> {
            return await apiBaseClient.put<UpdateCommentDto>(`services/api/internal/comment/${commentId}`, commentData);
        },

        /**
         * Delete a comment
         */
        async deleteComment(commentId: string): Promise<BaseResultBo<undefined>> {
            return await apiBaseClient.delete(`services/api/internal/comment/${commentId}`);
        }
    };
}

