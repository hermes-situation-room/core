import aboutApi from "./about-api";
import postsApi from "./posts-api";
import tagsApi from "./tags-api";
import ApiBaseClient from "./base/api-base-client";
import SocketBaseClient from "./base/socket-base-client";

const apiBaseClient = new ApiBaseClient();

export const services = {
    about: aboutApi(apiBaseClient),
    posts: postsApi(apiBaseClient),
    tags: tagsApi(apiBaseClient)
}

const socketBaseClient = new SocketBaseClient();
/**
 * Indicated the initialization-state of the socket. If promise is resolved, the socket is fully initialized
 */
const initPromise = socketBaseClient.initialize();

export const sockets = {
    hub: {
        /**
         * Ensures the socket for hub is fully initialized. Always call ensure before registering or sending over the socket 
         */
        ensureSocketInitialization: async() => {
            await initPromise;
            console.log("Hub-Socket initialized");
        },
        /**
         * Registers a new event over the socket
         * @param event Sting with the event-name which is triggered over the socket
         * @param action Function which is executed when the event is triggered over the socket
         */
        registerToEvent: async(event: string, action: Function) => {
            socketBaseClient.registerToEvent(event, action);
        },
        /**
         * Send a JoinChat event over the socket to the backend
         * @param userId Guid (string) with the userId
         * @param chatId Guid (string) with the chatId
         */
        joinChat: (userId: string, chatId: string) => {
            socketBaseClient.sendEvent("JoinChat", [userId, chatId]);
        },
        /**
         * Send a LeaveChat event over the socket to the backend to unsubscribe to chat messages
         * @param chatId Guid (string) with the chatId to be left
         */
        leaveChat: (chatId: string) => {
            socketBaseClient.sendEvent("LeaveChat", [chatId]);
        }
    }
}