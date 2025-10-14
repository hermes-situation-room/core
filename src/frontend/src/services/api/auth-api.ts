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
    userData: any;
}

class AuthApi extends ApiBaseClient {
    /**
     * Login as activist
     * @param request Login credentials for activist
     * @returns User UID on success
     */
    async loginActivist(request: LoginActivistDto): Promise<BaseResultBo<string>> {
        return await this.post<LoginActivistDto>(
            "services/api/internal/authorization/login/activist",
            request
        );
    }

    /**
     * Login as journalist
     * @param request Login credentials for journalist
     * @returns User UID on success
     */
    async loginJournalist(request: LoginJournalistDto): Promise<BaseResultBo<string>> {
        return await this.post<LoginJournalistDto>(
            "services/api/internal/authorization/login/journalist",
            request
        );
    }

    /**
     * Register new activist
     * @param request Activist registration data
     * @returns User UID on success
     */
    async registerActivist(request: RegisterActivistDto): Promise<BaseResultBo<string>> {
        return await this.post<RegisterActivistDto>(
            "services/api/internal/activist",
            request
        );
    }

    /**
     * Register new journalist
     * @param request Journalist registration data
     * @returns User UID on success
     */
    async registerJournalist(request: RegisterJournalistDto): Promise<BaseResultBo<string>> {
        return await this.post<RegisterJournalistDto>(
            "services/api/internal/journalist",
            request
        );
    }

    /**
     * Logout current user
     * Clears authentication cookie
     */
    async logout(): Promise<BaseResultBo<undefined>> {
        const url = `${this.apiBaseUrl}services/api/internal/authorization/logout`;
        try {
            const response = await fetch(url, {
                method: 'POST',
                credentials: 'include',
                headers: {
                    "Accept": "application/json, text/plain, */*",
                },
            });

            return {
                data: undefined,
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
    }

    /**
     * Get current authenticated user information
     * @returns Current user data from authentication cookie
     */
    async getCurrentUser(): Promise<BaseResultBo<CurrentUserResponse>> {
        return await this.get<CurrentUserResponse>(
            "services/api/internal/authorization/me"
        );
    }
}

export default new AuthApi();

