import type {BaseResultBo} from "../../models/bo/base-result-bo";
import {useAuthStore} from "../../../stores/auth-store";
import router from "../../../router";
import { parseErrorResponse, parseNetworkError } from "../../../utils/error-parser";

export default class ApiBaseClient {
    apiBaseUrl: string = import.meta.env.VITE_APP_BACKEND;

    /**
     * Handle 401 Unauthorized responses by redirecting to login
     * Clears auth state locally without calling logout API (session already expired)
     * Uses dynamic imports to avoid circular dependency
     */
    private async handleUnauthorized(status: number) {
        if (status === 401) {
            const authStore = useAuthStore();
            authStore.clearAuthState();

            if (router.currentRoute.value.path !== '/login') {
                router.push('/login');
            }
        }
    }

    /**
     * GET functionality which handels errors, no try-catch needed for the caller
     * @param route Route which should be called by the cliend - Route-Parameters must be included into the route by the caller
     * @param params Query-params which will be included if provided
     * @returns Allways returns a Promise<BaseResultBo<T>>
     */
    async get<T>(route: string, params?: Record<string, string>): Promise<BaseResultBo<T>> {
        const query = new URLSearchParams(params).toString();
        const url = `${this.apiBaseUrl}${route}${query ? "?" + query : ""}`;

        try {
            const response = await fetch(url, {
                method: "GET",
                credentials: "include",
                headers: {
                    "Accept": "application/json, text/plain, */*",
                },
            });

            void this.handleUnauthorized(response.status);

            if (!response.ok) {
                const error = await parseErrorResponse(response);
                return {
                    data: undefined,
                    responseCode: response.status,
                    responseMessage: error.message,
                    isSuccess: false,
                    error
                };
            }

            const contentType = response.headers.get("content-type");
            let data: any;

            if (contentType?.includes("application/json")) {
                data = await response.json();
            } else if (contentType?.includes("text/")) {
                data = await response.text();
            } else {
                data = await response.blob();
            }

            return {
                data: data as T,
                responseCode: response.status,
                responseMessage: response.statusText,
                isSuccess: response.ok,
            };
        } catch (e) {
            const error = parseNetworkError(e);
            return {
                data: undefined,
                responseCode: undefined,
                responseMessage: error.message,
                isSuccess: false,
                error
            };
        }
    }

    /**
     * POST functionality which handels errors, no try-catch needed for the caller
     * @param route Route which should be called by the client - Route-Parameters must be included into the route by the caller
     * @param input Object which is sent to the API. Object will not be validated, must be done by the caller
     * @returns Always returns a BaseResultBo of type string which contains the uid of the created object
     */
    async post<T>(route: string, input: T): Promise<BaseResultBo<string>> {
        const url = `${this.apiBaseUrl}${route}`;
        try {
            const response = await fetch(url, {
                method: 'POST',
                credentials: "include",
                headers: {
                    "Content-Type": "application/json",
                    "Accept": "application/json, text/plain, */*",
                },
                body: JSON.stringify(input)
            });

            void this.handleUnauthorized(response.status);

            if (!response.ok) {
                const error = await parseErrorResponse(response);
                return {
                    data: undefined,
                    responseCode: response.status,
                    responseMessage: error.message,
                    isSuccess: false,
                    error
                };
            }

            const uid = (await response.text()).replace(/"/g, '');

            return {
                data: uid,
                responseCode: response.status,
                responseMessage: response.statusText,
                isSuccess: response.ok,
            };
        } catch (e) {
            const error = parseNetworkError(e);
            return {
                data: undefined,
                responseCode: undefined,
                responseMessage: error.message,
                isSuccess: false,
                error
            };
        }
    }

    /**
     * PUT functionality which handels errors, no try-catch needed for the caller
     * @param route Route which should be called by the cliend - Route-Parameters must be included into the route by the caller
     * @param input Object which is sent to the API. Object will not be validated, must be done by the caller
     * @returns Always returns a BaseResultBo of type string which contains the uid of the created object
     */
    async put<T>(route: string, input: T): Promise<BaseResultBo<string>> {
        const url = `${this.apiBaseUrl}${route}`;
        try {
            const response = await fetch(url, {
                method: 'PUT',
                credentials: "include",
                headers: {
                    "Content-Type": "application/json",
                    "Accept": "application/json, text/plain, */*",
                },
                body: JSON.stringify(input)
            });

            void this.handleUnauthorized(response.status);

            if (!response.ok) {
                const error = await parseErrorResponse(response);
                return {
                    data: undefined,
                    responseCode: response.status,
                    responseMessage: error.message,
                    isSuccess: false,
                    error
                };
            }

            const uid = await response.text();

            return {
                data: uid,
                responseCode: response.status,
                responseMessage: response.statusText,
                isSuccess: response.ok,
            };
        } catch (e) {
            const error = parseNetworkError(e);
            return {
                data: undefined,
                responseCode: undefined,
                responseMessage: error.message,
                isSuccess: false,
                error
            };
        }
    }

    /**
     * DELETE functionality which handels errors, no try-catch needed for the caller
     * @param route The route called, the id of the deletion-item must be included in the url by the caller
     * @returns Always returns a BaseResultBo of type undefined which only contains the response message and code
     */
    async delete(route: string): Promise<BaseResultBo<undefined>> {
        const url = `${this.apiBaseUrl}${route}`;
        try {
            const response = await fetch(url, {
                method: 'DELETE',
                credentials: "include",
            });

            void this.handleUnauthorized(response.status);

            if (!response.ok) {
                const error = await parseErrorResponse(response);
                return {
                    data: undefined,
                    responseCode: response.status,
                    responseMessage: error.message,
                    isSuccess: false,
                    error
                };
            }

            return {
                data: undefined,
                responseCode: response.status,
                responseMessage: response.statusText,
                isSuccess: response.ok
            };
        } catch (e) {
            const error = parseNetworkError(e);
            return {
                data: undefined,
                responseCode: undefined,
                responseMessage: error.message,
                isSuccess: false,
                error
            };
        }
    }
}
