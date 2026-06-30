import type { BaseResultBo } from "../models/bo/base-result-bo";
import ApiBaseClient from "./base/api-base-client";

export interface TagRequestDto {
    uid: string;
    requestedByUid: string;
    name: string;
    status: string;
    reviewedByUid: string | null;
    reviewedAt: string | null;
    createdAt: string;
}

export interface CreateTagRequestDto {
    name: string;
}

export default function tagRequestsApi(apiBaseClient: ApiBaseClient) {
    return {
        /**
         * Request a new post tag. The requester is always the authenticated user (server-enforced).
         */
        async createTagRequest(tagRequest: CreateTagRequestDto): Promise<BaseResultBo<string>> {
            return await apiBaseClient.post<CreateTagRequestDto>(`services/api/internal/tag-request`, tagRequest);
        },

        /**
         * Get the authenticated user's own tag requests
         */
        async getMyTagRequests(): Promise<BaseResultBo<TagRequestDto[]>> {
            return await apiBaseClient.get<TagRequestDto[]>(`services/api/internal/tag-request/my-requests`);
        },

        /**
         * Admin-only: list all pending tag requests awaiting review
         */
        async getPendingTagRequests(): Promise<BaseResultBo<TagRequestDto[]>> {
            return await apiBaseClient.get<TagRequestDto[]>(`services/api/internal/tag-request/pending`);
        },

        /**
         * Admin-only: approve a pending tag request, making the tag usable for posts
         */
        async approveTagRequest(uid: string): Promise<BaseResultBo<string>> {
            return await apiBaseClient.put<undefined>(`services/api/internal/tag-request/${uid}/approve`, undefined);
        },

        /**
         * Admin-only: reject a pending tag request
         */
        async rejectTagRequest(uid: string): Promise<BaseResultBo<string>> {
            return await apiBaseClient.put<undefined>(`services/api/internal/tag-request/${uid}/reject`, undefined);
        },
    };
}
