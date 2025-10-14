import ApiBaseClient from "./base/api-base-client";
import type {BaseResultBo} from "../models/bo/base-result-bo";

export interface LoginActivistDto {
    userName: string;
    password: string;
}

export interface LoginJournalistDto {
    emailAddress: string;
    password: string;
}

export interface RegisterActivistDto {
    userName: string;
    password: string;
    firstName?: string;
    lastName?: string;
    emailAddress?: string;
    isFirstNameVisible: boolean;
    isLastNameVisible: boolean;
    isEmailVisible: boolean;
}

export interface RegisterJournalistDto {
    firstName: string;
    lastName: string;
    emailAddress: string;
    password: string;
    employer: string;
}

export interface CurrentUserResponse {
    userId: string;
    userType: string;
}

export default function authApi(apiBaseClient: ApiBaseClient) {
    return {
        /**
         * Login as activist
         * @param request Login credentials for activist
         * @returns User UID on success
         */
        async loginActivist(request: LoginActivistDto): Promise<BaseResultBo<string>> {
            return await apiBaseClient.post<LoginActivistDto>(
                "services/api/internal/authorization/login/activist",
                request
            );
        },

        /**
         * Login as journalist
         * @param request Login credentials for journalist
         * @returns User UID on success
         */
        async loginJournalist(request: LoginJournalistDto): Promise<BaseResultBo<string>> {
            return await apiBaseClient.post<LoginJournalistDto>(
                "services/api/internal/authorization/login/journalist",
                request
            );
        },

        /**
         * Register new activist
         * @param request Activist registration data
         * @returns User UID on success
         */
        async registerActivist(request: RegisterActivistDto): Promise<BaseResultBo<string>> {
            return await apiBaseClient.post<RegisterActivistDto>(
                "services/api/internal/activist",
                request
            );
        },

        /**
         * Register new journalist
         * @param request Journalist registration data
         * @returns User UID on success
         */
        async registerJournalist(request: RegisterJournalistDto): Promise<BaseResultBo<string>> {
            return await apiBaseClient.post<RegisterJournalistDto>(
                "services/api/internal/journalist",
                request
            );
        },

        /**
         * Logout current user
         * Clears authentication cookie
         */
        async logout() {
            await apiBaseClient.post<undefined>(`services/api/internal/authorization/logout`, undefined);
        },

        /**
         * Get current authenticated user information
         * @returns Current user data from authentication cookie
         */
        async getCurrentUser(): Promise<BaseResultBo<CurrentUserResponse>> {
            return await apiBaseClient.get<CurrentUserResponse>(
                "services/api/internal/authorization/me"
            );
        }
    }
}
