import type { BaseResultBo } from "../models/bo/base-result-bo";
import ApiBaseClient from "./base/api-base-client";

export interface ContentFilterRuleDto {
    uid: string;
    pattern: string;
    action: string;
    enabled: boolean;
    createdAt: string;
}

export interface CreateContentFilterRuleDto {
    pattern: string;
    action: string;
}

export interface UpdateContentFilterRuleDto {
    pattern: string;
    action: string;
    enabled: boolean;
}

export default function contentFilterApi(apiBaseClient: ApiBaseClient) {
    return {
        /**
         * Admin-only: list all content filter rules
         */
        async getAllRules(): Promise<BaseResultBo<ContentFilterRuleDto[]>> {
            return await apiBaseClient.get<ContentFilterRuleDto[]>(`services/api/internal/content-filter`);
        },

        /**
         * Admin-only: create a new content filter rule
         */
        async createRule(rule: CreateContentFilterRuleDto): Promise<BaseResultBo<string>> {
            return await apiBaseClient.post<CreateContentFilterRuleDto>(`services/api/internal/content-filter`, rule);
        },

        /**
         * Admin-only: update an existing content filter rule (pattern, action, enabled)
         */
        async updateRule(uid: string, rule: UpdateContentFilterRuleDto): Promise<BaseResultBo<string>> {
            return await apiBaseClient.put<UpdateContentFilterRuleDto>(`services/api/internal/content-filter/${uid}`, rule);
        },

        /**
         * Admin-only: delete a content filter rule
         */
        async deleteRule(uid: string): Promise<BaseResultBo<undefined>> {
            return await apiBaseClient.delete(`services/api/internal/content-filter/${uid}`);
        },
    };
}
