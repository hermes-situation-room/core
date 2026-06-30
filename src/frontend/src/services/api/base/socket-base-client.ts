import * as signalR from '@microsoft/signalr';

const apiBaseUrl = import.meta.env.VITE_APP_BACKEND;
const apiHubPath = import.meta.env.VITE_APP_SOCKET_HUB_PATH;

const connection = new signalR.HubConnectionBuilder()
    .withUrl(`${apiBaseUrl}${apiHubPath}`, {
        withCredentials: true
    })
    .build();

export default class SocketBaseClient {

    /**
     * Per-event map from the caller-supplied action to the wrapped handler that was actually
     * registered on the shared connection. This lets us remove exactly the handler we added,
     * instead of dropping every handler bound to the event-name on the shared singleton.
     */
    private readonly handlers = new Map<string, Map<Function, (...args: any[]) => void>>();

    /**
     * Registers a new event to be triggered with a certain backend socket-message.
     * The caller-supplied action is wrapped and the wrapped handler is tracked per event so it
     * can later be removed individually via unregisterFromEvent(event, action).
     * Registering the same action for the same event twice is a no-op (avoids double-firing on remount).
     * @param event Event-name to be registered in the backend
     * @param action Function which is executed with the response-object from the socket-message
     */
    registerToEvent = (event: string, action: Function) => {
        let eventHandlers = this.handlers.get(event);
        if (!eventHandlers) {
            eventHandlers = new Map();
            this.handlers.set(event, eventHandlers);
        }

        if (eventHandlers.has(action)) {
            return;
        }

        const wrappedHandler = (...args: any[]) => action(...args);
        eventHandlers.set(action, wrappedHandler);
        connection.on(event, wrappedHandler);
    }

    /**
     * Removes a registered handler for a given event-name. When an action is supplied only the
     * handler that was registered for that exact action is removed, so other components keeping
     * a listener on the same event-name are unaffected. When no action is supplied, every handler
     * this client registered for the event is removed (used for full teardown).
     * @param event Event-name to unregister
     * @param action The same function reference that was passed to registerToEvent. Optional.
     */
    unregisterFromEvent = (event: string, action?: Function) => {
        const eventHandlers = this.handlers.get(event);
        if (!eventHandlers) {
            return;
        }

        if (action) {
            const wrappedHandler = eventHandlers.get(action);
            if (wrappedHandler) {
                connection.off(event, wrappedHandler);
                eventHandlers.delete(action);
            }
        } else {
            eventHandlers.forEach((wrappedHandler) => connection.off(event, wrappedHandler));
            eventHandlers.clear();
        }

        if (eventHandlers.size === 0) {
            this.handlers.delete(event);
        }
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

    flush = () => {
        this.handlers.clear();
        connection.stop();
    }
}
