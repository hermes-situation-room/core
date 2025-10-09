import aboutApi from "./about-api";
import ApiBaseClient from "./base/api-base-client";

const apiBaseClient = new ApiBaseClient();

export const services = {
    about: aboutApi(apiBaseClient)
}