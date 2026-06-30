import aboutApi from "./about-api";
import postsApi from "./posts-api";
import tagsApi from "./tags-api";
import chatsApi from "./chats-api";
import messagesApi from "./messages-api";
import ApiBaseClient from "./base/api-base-client";
import SocketBaseClient from "./base/socket-base-client";
import authApi from "./auth-api.ts";
import usersApi from "./users-api";
import messageStatusApi from "./messageStatus-api.ts";
import commentsApi from "./comments-api.ts";
import privacyLevelPersonalApi from "./privacy-level-personal-api.ts";
import postLikesApi from "./post-likes-api.ts";
import notificationsApi from "./notifications-api.ts";
import userTagsApi from "./user-tags-api.ts";
import tagRequestsApi from "./tag-requests-api.ts";
import contentFilterApi from "./content-filter-api.ts";
import reportsApi from "./reports-api.ts";
import attachmentsApi from "./attachments-api.ts";

const apiBaseClient = new ApiBaseClient();

export const services = {
    comments: commentsApi(apiBaseClient),
    about: aboutApi(apiBaseClient),
    posts: postsApi(apiBaseClient),
    tags: tagsApi(apiBaseClient),
    chats: chatsApi(apiBaseClient),
    messages: messagesApi(apiBaseClient),
    auth: authApi(apiBaseClient),
    users: usersApi(apiBaseClient),
    userChatMessageStatus: messageStatusApi(apiBaseClient),
    privacyLevelPersonal: privacyLevelPersonalApi(apiBaseClient),
    postLikes: postLikesApi(apiBaseClient),
    notifications: notificationsApi(apiBaseClient),
    userTags: userTagsApi(apiBaseClient),
    tagRequests: tagRequestsApi(apiBaseClient),
    contentFilter: contentFilterApi(apiBaseClient),
    reports: reportsApi(apiBaseClient),
    attachments: attachmentsApi(apiBaseClient),
}

const socketBaseClient = new SocketBaseClient();
/**
 * Indicated the initialization-state of the socket. If promise is resolved, the socket is fully initialized
 */
let initPromise: Promise<void> | undefined = undefined;

export const sockets = {
    hub: {
        /**
         * Initializes the socket connection and ensures, the connection is established. Calling it again tells the caller that the connection is fully established. 
         */
        initialize: async(): Promise<void> => {
            if (!initPromise) {
                initPromise = socketBaseClient.initialize();
            }
            return initPromise;
        },
        flush: () => {
            initPromise = undefined;
            socketBaseClient.flush();
        },
        /**
         * Registers a new event over the socket
         * @param event Sting with the event-name which is triggered over the socket
         * @param action Function which is executed when the event is triggered over the socket
         */
        registerToEvent: async(event: string, action: Function) => {
            await initPromise;
            socketBaseClient.registerToEvent(event, action);
        },
        /**
         * Removes a handler registered for the given event-name. Pass the same action reference
         * used in registerToEvent to remove only that handler, leaving other components' listeners
         * on the same event intact. Omit the action to remove all handlers this client registered
         * for the event. Call on unmount to avoid leaks.
         * @param event Event-name to unregister
         * @param action Optional: the exact handler reference passed to registerToEvent
         */
        unregisterFromEvent: async(event: string, action?: Function) => {
            await initPromise;
            socketBaseClient.unregisterFromEvent(event, action);
        },
        /**
         * Send a JoinChat event over the socket to the backend
         * @param chatId Guid (string) with the chatId
         */
        joinChat: async (chatId: string) => {
            await initPromise;
            socketBaseClient.sendEvent("JoinChat", [chatId]);
        },
        /**
         * Send a LeaveChat event over the socket to the backend to unsubscribe to chat messages
         * @param chatId Guid (string) with the chatId to be left
         */
        leaveChat: async (chatId: string) => {
            await initPromise;
            socketBaseClient.sendEvent("LeaveChat", [chatId]);
        },
        joinMessaging: async () => {
            await initPromise;
            socketBaseClient.sendEvent("JoinMessaging", []);
        },
        leaveMessaging: async () => {
            await initPromise;
            socketBaseClient.sendEvent("LeaveMessaging", []);
        },
        /**
         * Send an UpdateReadChat event over the socket to mark a chat as read
         * @param chatId Guid (string) with the chatId to be marked as read
         */
        updateReadChat: async (chatId: string) => {
            await initPromise;
            socketBaseClient.sendEvent("UpdateReadChat", [chatId]);
        },
        /**
         * Join the shared "posts" feed group to receive live post create/update/delete events
         */
        joinPosts: async () => {
            await initPromise;
            socketBaseClient.sendEvent("JoinPosts", []);
        },
        /**
         * Leave the shared "posts" feed group
         */
        leavePosts: async () => {
            await initPromise;
            socketBaseClient.sendEvent("LeavePosts", []);
        },
        /**
         * Join the per-post comment group to receive live comment events for a post
         * @param postId Guid (string) with the postId
         */
        joinPostComments: async (postId: string) => {
            await initPromise;
            socketBaseClient.sendEvent("JoinPostComments", [postId]);
        },
        /**
         * Leave the per-post comment group
         * @param postId Guid (string) with the postId to be left
         */
        leavePostComments: async (postId: string) => {
            await initPromise;
            socketBaseClient.sendEvent("LeavePostComments", [postId]);
        }
    }
}