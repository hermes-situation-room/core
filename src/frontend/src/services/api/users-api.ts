import type { BaseResultBo } from "../models/bo/base-result-bo";
import type {UserProfileBo, ActivistBo, JournalistBo} from "../../types/user";
import ApiBaseClient from "./base/api-base-client";

export default function usersApi(apiBaseClient: ApiBaseClient) {
    return {
        /**
         * Get user profile
         */
        async getUserProfile(userUid: string, consumerUid: string): Promise<BaseResultBo<UserProfileBo>> {
            const params: Record<string, string> = {};
            params.uid = userUid;
            if (consumerUid.length > 0) { params.consumerUid = consumerUid; }
            return await apiBaseClient.get<UserProfileBo>('services/api/internal/user/profile', params);
        },
        /**
         * Get display name for a user (username for activists, firstname + lastname for journalists)
         */
        async getDisplayName(userUid: string): Promise<BaseResultBo<{ displayName: string }>> {
            return await apiBaseClient.get<{ displayName: string }>(`services/api/internal/user/display-name/${userUid}`);
        },
        /**
         * Get activist privacy
         */
        async getActivistPrivacy(uid: string): Promise<BaseResultBo<ActivistBo>> {
            return await apiBaseClient.get<ActivistBo>(`services/api/internal/activist/${uid}`);
        },
        /**
         * Update activist profile (full ActivistBo)
         */
        async updateActivist(uid: string, activistData: ActivistBo): Promise<BaseResultBo<string>> {
            return await apiBaseClient.put<ActivistBo>(`services/api/internal/activist/${uid}`, activistData);
        },
        /**
         * Update journalist profile
         */
        async updateJournalist(uid: string, journalistData: JournalistBo): Promise<BaseResultBo<string>> {
            return await apiBaseClient.put<JournalistBo>(`services/api/internal/journalist/${uid}`, journalistData);
        }
    };
}
