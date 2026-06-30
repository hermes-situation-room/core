import type { BaseResultBo } from "../models/bo/base-result-bo";
import ApiBaseClient from "./base/api-base-client";

export interface PostLikeStatsDto {
    postUid: string;
    likeCount: number;
    userHasLiked: boolean;
}

export default function postLikesApi(apiBaseClient: ApiBaseClient) {
    return {
        /**
         * Get like statistics for a post
         */
        async getPostLikeStats(postId: string, userId?: string): Promise<BaseResultBo<PostLikeStatsDto>> {
            const params: Record<string, string> = {};
            if (userId) {
                params.userId = userId;
            }
            return await apiBaseClient.get<PostLikeStatsDto>(`services/api/internal/post/${postId}/likes/stats`, params);
        },
        
        /**
         * Toggle like on a post (like if not liked, unlike if liked)
         */
        async toggleLike(postId: string, userId: string): Promise<BaseResultBo<string>> {
            return await apiBaseClient.post<{}>(`services/api/internal/post/${postId}/likes/toggle?userId=${userId}`, {});
        },
    };
}

