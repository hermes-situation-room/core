import type {BaseResultBo} from "../models/bo/base-result-bo";
import type {CreatePostRequest, PostBo, PostFilter, UpdatePostRequest} from "../../types/post";
import ApiBaseClient from "./base/api-base-client";

export default function postsApi(apiBaseClient: ApiBaseClient) {
    return {
        /**
         * Get all activist posts
         */
        async getActivistPosts(): Promise<BaseResultBo<PostBo[]>> {
            return await apiBaseClient.get<PostBo[]>('services/api/internal/post/activist');
        },

        /**
         * Get all journalist posts
         */
        async getJournalistPosts(): Promise<BaseResultBo<PostBo[]>> {
            return await apiBaseClient.get<PostBo[]>('services/api/internal/post/journalist');
        },

        /**
         * Get activist posts by tags
         */
        async getActivistPostsByTags(tags: string[]): Promise<BaseResultBo<PostBo[]>> {
            const params: Record<string, string> = {};
            if (tags.length > 0) {
                params.tags = tags.map(tag => tag.toUpperCase()).join(',');
            }
            return await apiBaseClient.get<PostBo[]>('services/api/internal/post/activist/by-tags', params);
        },

        /**
         * Get journalist posts by tags
         */
        async getJournalistPostsByTags(tags: string[]): Promise<BaseResultBo<PostBo[]>> {
            const params: Record<string, string> = {};
            if (tags.length > 0) {
                params.tags = tags.map(tag => tag.toUpperCase()).join(',');
            }
            return await apiBaseClient.get<PostBo[]>('services/api/internal/post/journalist/by-tags', params);
        },

        /**
         * Get posts by user UID
         */
        async getPostsByUser(userUid: string): Promise<BaseResultBo<PostBo[]>> {
            return await apiBaseClient.get<PostBo[]>(`services/api/internal/post/user/${userUid}`);
        },

        /**
         * Get a single post by UID
         */
        async getPostById(uid: string): Promise<BaseResultBo<PostBo>> {
            return await apiBaseClient.get<PostBo>(`services/api/internal/post/${uid}`);
        },

        /**
         * Create a new post
         */
        async createPost(postData: CreatePostRequest): Promise<BaseResultBo<string>> {
            console.log(postData)
            return await apiBaseClient.post<CreatePostRequest>('services/api/internal/post', postData);
        },

        /**
         * Update an existing post
         */
        async updatePost(uid: string, postData: UpdatePostRequest): Promise<BaseResultBo<PostBo>> {
            const url = `${apiBaseClient.apiBaseUrl}services/api/internal/post/${uid}`;
            try {
                const response = await fetch(url, {
                    method: 'PUT',
                    headers: {
                        "Content-Type": "application/json",
                        "Accept": "application/json, text/plain, */*"
                    },
                    body: JSON.stringify(postData)
                });

                const data = await response.json();

                return {
                    data: data as PostBo,
                    responseCode: response.status,
                    responseMessage: response.statusText,
                    isSuccess: response.ok,
                };
            } catch (e) {
                return {
                    data: undefined,
                    responseCode: undefined,
                    responseMessage: e instanceof Error ? e.message : String(e),
                    isSuccess: false,
                };
            }
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
            if (!filter) {
                return await this.getActivistPosts();
            }

            if (filter.category === 'activist') {
                if (filter.tags && filter.tags.length > 0) {
                    return await this.getActivistPostsByTags(filter.tags);
                }
                return await this.getActivistPosts();
            } else if (filter.category === 'journalist') {
                if (filter.tags && filter.tags.length > 0) {
                    return await this.getJournalistPostsByTags(filter.tags);
                }
                return await this.getJournalistPosts();
            }

            if (filter.userUid) {
                return await this.getPostsByUser(filter.userUid);
            }

            return await this.getActivistPosts();
        }
    };
}
