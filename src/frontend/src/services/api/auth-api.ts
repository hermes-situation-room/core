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
}

export default new AuthApi();

