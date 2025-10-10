import aboutApi from "./about-api";
import postsApi from "./posts-api";
import tagsApi from "./tags-api";
import ApiBaseClient from "./base/api-base-client";

const apiBaseClient = new ApiBaseClient();

export const services = {
    about: aboutApi(apiBaseClient),
    posts: postsApi(apiBaseClient),
    tags: tagsApi(apiBaseClient)
}