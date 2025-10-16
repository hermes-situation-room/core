import type { BaseResultBo } from "../models/bo/base-result-bo";
import type {CreatePrivacyLevelPersonalDto, PrivacyLevelPersonalBo, UpdatePrivacyLevelPersonalDto} from "../../types/privacy-level-personal";
import ApiBaseClient from "./base/api-base-client";

export default function privacyLevelPersonalApi(apiBaseClient: ApiBaseClient) {
    return {
        /**
         * Get personal privacy level of an owner and consumer
         */
        async getPrivacyLevelPersonal(ownerUid: string, consumerUid: string): Promise<BaseResultBo<PrivacyLevelPersonalBo>> {
            const params: Record<string, string> = {};
            params.ownerUid = ownerUid;
            params.consumerUid = consumerUid;
            return await apiBaseClient.get<PrivacyLevelPersonalBo>('services/api/internal/privacylevelpersonal', params);
        },

        /**
         * Create new personal privacy level between an owner and consumer
         */
        async createPrivacyLevelPersonal(privacyLevelPersonalData: CreatePrivacyLevelPersonalDto): Promise<BaseResultBo<string>> {
            return await apiBaseClient.post<CreatePrivacyLevelPersonalDto>(`services/api/internal/privacylevelpersonal`, privacyLevelPersonalData);
        },
        
        /**
         * Update personal privacy level between an owner and consumer
         */
        async updatePrivacyLevelPersonal(privacyLevelPersonalUid: string, privacylevelpersonalData: UpdatePrivacyLevelPersonalDto): Promise<BaseResultBo<string>> {
            return await apiBaseClient.put<UpdatePrivacyLevelPersonalDto>(`services/api/internal/privacylevelpersonal/${privacyLevelPersonalUid}`, privacylevelpersonalData);
        },

        /**
         * Delete a comment
         */
        async deletePrivacyLevelPersonal(privacyLevelPersonalUid: string): Promise<BaseResultBo<undefined>> {
            return await apiBaseClient.delete(`services/api/internal/privacylevelpersonal/${privacyLevelPersonalUid}`);
        }
    };
}
