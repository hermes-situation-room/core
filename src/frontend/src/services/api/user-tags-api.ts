import type { BaseResultBo } from "../models/bo/base-result-bo";
import ApiBaseClient from "./base/api-base-client";

export interface UserTagDto {
    uid: string;
    ownerUid: string;
    taggedUserUid: string;
    tagName: string;
    notes: string;
    createdAt: string;
}

export interface CreateUserTagDto {
    taggedUserUid: string;
    tagName: string;
    notes: string;
}

export interface UpdateUserTagDto {
    tagName: string;
    notes: string;
}

export default function userTagsApi(apiBaseClient: ApiBaseClient) {
    return {
        /**
         * Get all private tags created by the authenticated user
         */
        async getMyTags(): Promise<BaseResultBo<UserTagDto[]>> {
            return await apiBaseClient.get<UserTagDto[]>(`services/api/internal/user-tag`);
        },

        /**
         * Get the authenticated user's private tags for a specific tagged user
         */
        async getMyTagsForUser(taggedUserId: string): Promise<BaseResultBo<UserTagDto[]>> {
            return await apiBaseClient.get<UserTagDto[]>(`services/api/internal/user-tag/user/${taggedUserId}`);
        },

        /**
         * Search the authenticated user's private tags by tag name
         */
        async searchMyTags(tagName: string): Promise<BaseResultBo<UserTagDto[]>> {
            return await apiBaseClient.get<UserTagDto[]>(`services/api/internal/user-tag/search`, { tagName });
        },

        /**
         * Create a new private tag. The owner is always the authenticated user (server-enforced).
         */
        async createTag(tag: CreateUserTagDto): Promise<BaseResultBo<string>> {
            return await apiBaseClient.post<CreateUserTagDto>(`services/api/internal/user-tag`, tag);
        },

        /**
         * Update one of the authenticated user's private tags
         */
        async updateTag(uid: string, tag: UpdateUserTagDto): Promise<BaseResultBo<string>> {
            return await apiBaseClient.put<UpdateUserTagDto>(`services/api/internal/user-tag/${uid}`, tag);
        },

        /**
         * Delete one of the authenticated user's private tags
         */
        async deleteTag(uid: string): Promise<BaseResultBo<undefined>> {
            return await apiBaseClient.delete(`services/api/internal/user-tag/${uid}`);
        },
    };
}
