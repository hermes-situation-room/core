import type {BaseResultBo} from "../../models/bo/base-result-bo";
import {useAuthStore} from "../../../stores/auth-store";

export default class ApiBaseClient {
    apiBaseUrl: string = import.meta.env.VITE_APP_BACKEND;

    /**
     * Handle 401 Unauthorized responses without forcing navigation
     * Clears auth state locally; routing guards will handle protected routes
     */
    private async handleUnauthorized(status: number) {
        if (status === 401) {
            const authStore = useAuthStore();
            authStore.clearAuthState();
        }
    }

    /**
     * Extract error message from ProblemDetails response or fallback to statusText
     * @param response The fetch response
     * @param data The parsed response data
     * @returns A user-friendly error message
     */
    private getErrorMessage(response: Response, data: any): string {
        if (data && typeof data === 'object') {
            if (data.detail) {
                return data.detail;
            }
            
            if (data.title) {
                return data.title;
            }
            
            if (data.errors && typeof data.errors === 'object') {
                const errorMessages: string[] = [];
                for (const field in data.errors) {
                    const fieldErrors = data.errors[field];
                    if (Array.isArray(fieldErrors)) {
                        errorMessages.push(...fieldErrors);
                    }
                }
                if (errorMessages.length > 0) {
                    return errorMessages.join(', ');
                }
            }
        }
        
        const statusMessages: Record<number, string> = {
            400: 'Invalid request. Please check your input.',
            401: 'Unauthorized. Please log in.',
            403: 'Access denied.',
            404: 'Resource not found.',
            409: 'This resource already exists.',
            500: 'Server error. Please try again later.',
            503: 'Service unavailable. Please try again later.'
        };
        
        return statusMessages[response.status] || response.statusText || 'An error occurred';
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
                data: response.ok ? (data as T) : undefined,
                responseCode: response.status,
                responseMessage: this.getErrorMessage(response, data),
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

            const contentType = response.headers.get("content-type");
            let responseData: any;

            if (contentType?.includes("application/json")) {
                responseData = await response.json();
            } else {
                responseData = await response.text();
            }

            const uid = response.ok && typeof responseData === 'string' 
                ? responseData.replace(/"/g, '') 
                : undefined;

            return {
                data: uid,
                responseCode: response.status,
                responseMessage: response.ok ? 'Success' : this.getErrorMessage(response, responseData),
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

            const contentType = response.headers.get("content-type");
            let responseData: any;

            if (contentType?.includes("application/json")) {
                responseData = await response.json();
            } else {
                responseData = await response.text();
            }

            const uid = response.ok && typeof responseData === 'string' 
                ? responseData.replace(/"/g, '') 
                : undefined;

            return {
                data: uid,
                responseCode: response.status,
                responseMessage: response.ok ? 'Success' : this.getErrorMessage(response, responseData),
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
                headers: {
                    "Accept": "application/json, text/plain, */*",
                }
            });

            void this.handleUnauthorized(response.status);

            let responseData: any;
            const contentType = response.headers.get("content-type");
            
            if (!response.ok && contentType?.includes("application/json")) {
                try {
                    responseData = await response.json();
                } catch {
                    responseData = null;
                }
            }

            return {
                data: undefined,
                responseCode: response.status,
                responseMessage: response.ok ? 'Success' : this.getErrorMessage(response, responseData),
                isSuccess: response.ok
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
}
