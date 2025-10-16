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
        }
    }
}