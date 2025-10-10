import type { BaseResultBo } from "../models/bo/base-result-bo";
import ApiBaseClient from "./base/api-base-client";

export default function tagsApi(apiBaseClient: ApiBaseClient) {
    return {
        /**
         * Get all available tags
         */
        async getAllTags(): Promise<BaseResultBo<string[]>> {
            return await apiBaseClient.get<string[]>('services/api/internal/tags');
        }
    };
}

