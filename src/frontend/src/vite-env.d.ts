/// <reference types="vite/client" />

declare module "virtual:pwa-register" {
    export function registerSW(options?: {
        immediate?: boolean;
        onNeedRefresh?: () => void;
        onOfflineReady?: () => void;
    }): void;
}

declare module "virtual:pwa-register/vue" {
    import type { Ref } from "vue";
    export function useRegisterSW(options?: {
        immediate?: boolean;
        onNeedRefresh?: () => void;
        onOfflineReady?: () => void;
    }): {
        offlineReady: Ref<boolean>;
        needRefresh: Ref<boolean>;
        updateServiceWorker: (reloadPage?: boolean) => Promise<void>;
    };
}
