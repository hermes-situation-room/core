import type { BaseResultBo } from "../models/bo/base-result-bo";
import type { CreatePostDto, PostBo, PostFilter, UpdatePostDto } from "../../types/post";
import ApiBaseClient from "./base/api-base-client";

export default function postsApi(apiBaseClient: ApiBaseClient) {
    return {
        /**
         * Get all activist posts
         */
        async getActivistPosts(limit?: number, offset?: number, query?: string, sortBy?: string): Promise<BaseResultBo<PostBo[]>> {
            const params: Record<string, string> = {};
            if (limit !== undefined) params.limit = limit.toString();
            if (offset !== undefined) params.offset = offset.toString();
            if (query) params.query = query;
            if (sortBy) params.sortBy = sortBy;
            return await apiBaseClient.get<PostBo[]>('services/api/internal/post/activist', params);
        },

        /**
         * Get all journalist posts
         */
        async getJournalistPosts(limit?: number, offset?: number, query?: string, sortBy?: string): Promise<BaseResultBo<PostBo[]>> {
            const params: Record<string, string> = {};
            if (limit !== undefined) params.limit = limit.toString();
            if (offset !== undefined) params.offset = offset.toString();
            if (query) params.query = query;
            if (sortBy) params.sortBy = sortBy;
            return await apiBaseClient.get<PostBo[]>('services/api/internal/post/journalist', params);
        },

        /**
         * Get activist posts by tags
         */
        async getActivistPostsByTags(tags: string[], limit?: number, offset?: number, query?: string, sortBy?: string): Promise<BaseResultBo<PostBo[]>> {
            const params: Record<string, string> = {};
            if (tags.length > 0) {
                params.tags = tags.map(tag => tag.toUpperCase()).join(',');
            }
            if (limit !== undefined) params.limit = limit.toString();
            if (offset !== undefined) params.offset = offset.toString();
            if (query) params.query = query;
            if (sortBy) params.sortBy = sortBy;
            return await apiBaseClient.get<PostBo[]>('services/api/internal/post/activist/by-tags', params);
        },

        /**
         * Get journalist posts by tags
         */
        async getJournalistPostsByTags(tags: string[], limit?: number, offset?: number, query?: string, sortBy?: string): Promise<BaseResultBo<PostBo[]>> {
            const params: Record<string, string> = {};
            if (tags.length > 0) {
                params.tags = tags.map(tag => tag.toUpperCase()).join(',');
            }
            if (limit !== undefined) params.limit = limit.toString();
            if (offset !== undefined) params.offset = offset.toString();
            if (query) params.query = query;
            if (sortBy) params.sortBy = sortBy;
            return await apiBaseClient.get<PostBo[]>('services/api/internal/post/journalist/by-tags', params);
        },

        /**
         * Get posts by user UID
         */
        async getPostsByUser(userUid: string, limit?: number, offset?: number, query?: string, sortBy?: string): Promise<BaseResultBo<PostBo[]>> {
            const params: Record<string, string> = {};
            if (limit !== undefined) params.limit = limit.toString();
            if (offset !== undefined) params.offset = offset.toString();
            if (query) params.query = query;
            if (sortBy) params.sortBy = sortBy;
            return await apiBaseClient.get<PostBo[]>(`services/api/internal/post/user/${userUid}`, params);
        },

        /**
         * Get a single post by UID
         */
        async getPostById(uid: string): Promise<BaseResultBo<PostBo>> {
            return await apiBaseClient.get<PostBo>(`services/api/internal/post/${uid}`);
        },

        /**
         * Get all post privacy levels
         */
        async getPostPrivacyLevels(): Promise<BaseResultBo<string[]>> {
            return await apiBaseClient.get<string[]>(`services/api/internal/post/privacies`);
        },

        /**
         * Create a new post
         */
        async createPost(postData: CreatePostDto): Promise<BaseResultBo<string>> {
            return await apiBaseClient.post<CreatePostDto>('services/api/internal/post', postData);
        },

        /**
         * Update an existing post
         */
        async updatePost(uid: string, postData: UpdatePostDto): Promise<BaseResultBo<string>> {
            return await apiBaseClient.put<PostBo>(`services/api/internal/post/${uid}`, postData);
        },

        /**
         * Delete a post
         */
        async deletePost(uid: string): Promise<BaseResultBo<undefined>> {
            return await apiBaseClient.delete(`services/api/internal/post/${uid}`);
        },

        /**
         * Get posts with filtering based on category and tags
         */
        async getPostsWithFilter(filter?: PostFilter): Promise<BaseResultBo<PostBo[]>> {
            const limit = filter?.limit;
            const offset = filter?.offset;
            const query = filter?.query;
            const sortBy = filter?.sortBy;

            if (!filter) {
                return await this.getActivistPosts(limit, offset, query, sortBy);
            }

            if (filter.category === 'activist') {
                if (filter.tags && filter.tags.length > 0) {
                    return await this.getActivistPostsByTags(filter.tags, limit, offset, query, sortBy);
                }
                return await this.getActivistPosts(limit, offset, query, sortBy);
            } else if (filter.category === 'journalist') {
                if (filter.tags && filter.tags.length > 0) {
                    return await this.getJournalistPostsByTags(filter.tags, limit, offset, query, sortBy);
                }
                return await this.getJournalistPosts(limit, offset, query, sortBy);
            }

            if (filter.userUid) {
                return await this.getPostsByUser(filter.userUid, limit, offset, query, sortBy);
            }

            return await this.getActivistPosts(limit, offset, query, sortBy);
        }
    };
}
