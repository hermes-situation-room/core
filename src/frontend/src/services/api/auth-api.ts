import ApiBaseClient from "./base/api-base-client";
import type { BaseResultBo } from "../models/bo/base-result-bo";

export interface LoginActivistRequest {
    userName: string;
    password: string;
}

export interface LoginJournalistRequest {
    emailAddress: string;
    password: string;
}

export interface RegisterActivistRequest {
    uid: string;
    userName: string;
    password: string;
    firstName?: string;
    lastName?: string;
    emailAddress?: string;
    isFirstNameVisible: boolean;
    isLastNameVisible: boolean;
    isEmailVisible: boolean;
}

export interface RegisterJournalistRequest {
    uid: string;
    firstName: string;
    lastName: string;
    emailAddress: string;
    password: string;
    employer: string;
}

class AuthApi extends ApiBaseClient {
    /**
     * Login as activist
     * @param request Login credentials for activist
     * @returns User UID on success
     */
    async loginActivist(request: LoginActivistRequest): Promise<BaseResultBo<string>> {
        return await this.post<LoginActivistRequest>(
            "services/api/internal/authorization/login/activist",
            request
        );
    }

    /**
     * Login as journalist
     * @param request Login credentials for journalist
     * @returns User UID on success
     */
    async loginJournalist(request: LoginJournalistRequest): Promise<BaseResultBo<string>> {
        return await this.post<LoginJournalistRequest>(
            "services/api/internal/authorization/login/journalist",
            request
        );
    }

    /**
     * Register new activist
     * @param request Activist registration data
     * @returns User UID on success
     */
    async registerActivist(request: RegisterActivistRequest): Promise<BaseResultBo<string>> {
        return await this.post<RegisterActivistRequest>(
            "services/api/internal/activist",
            request
        );
    }

    /**
     * Register new journalist
     * @param request Journalist registration data
     * @returns User UID on success
     */
    async registerJournalist(request: RegisterJournalistRequest): Promise<BaseResultBo<string>> {
        return await this.post<RegisterJournalistRequest>(
            "services/api/internal/journalist",
            request
        );
    }
}

export default new AuthApi();

