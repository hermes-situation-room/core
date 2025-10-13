import * as signalR from '@microsoft/signalr';

const apiBaseUrl = import.meta.env.VITE_APP_BACKEND;
const apiHubPath = import.meta.env.VITE_APP_SOCKET_HUB_PATH;

const connection = new signalR.HubConnectionBuilder()
    .withUrl(`${apiBaseUrl}${apiHubPath}`, {
        withCredentials: false
    })
    .build();

export default class SocketBaseClient {

    /**
     * Registers a new event to be triggered with a certain backend socket-message
     * @param event Event-name to be registered in the backend
     * @param action Function which is executed with the response-object from the socket-message
     */
    registerToEvent = (event: string, action: Function) => {
        connection.on(event, (response) => action(response));
    }

    /**
     * Send an event over the socket to the backend
     * @param event Event-name as string which addresses the backend-socket-endpoint
     * @param args Array of any-types which will be passed to the backend
     */
    sendEvent = (event: string, args: any[]) => {
        connection.send(event, ...args);
    }

    /**
     * Initializes the socket-connection and returns the state of the initialization as promise
     * @returns Promise which represents the initialization-state of the socket-connection 
     * -> promise.resolved == connection is fully initialized
     */
    initialize = (): Promise<void> => {
        const promise = connection.start()
            .catch((err) => console.log(err))
        return promise;
    }
}