import type ApiBaseClient from "./base/api-base-client"

export default (baseClient: ApiBaseClient) => {
    return {
        getAboutString: async () => {
            const response = await baseClient.get<String>("services/api/external/about");
            return response;
        }
    }
}