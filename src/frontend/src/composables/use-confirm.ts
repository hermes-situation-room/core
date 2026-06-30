import {readonly, ref} from 'vue';

export interface ConfirmOptions {
    title?: string;
    message: string;
    confirmLabel?: string;
    cancelLabel?: string;
    danger?: boolean;
}

export interface ConfirmState extends ConfirmOptions {
    id: string;
}

const current = ref<ConfirmState | null>(null);
let resolver: ((value: boolean) => void) | null = null;
let idCounter = 0;

const resolveCurrent = (value: boolean): void => {
    if (resolver) {
        const resolve = resolver;
        resolver = null;
        current.value = null;
        resolve(value);
    }
};

const confirm = (options: ConfirmOptions): Promise<boolean> => {
    // If a confirm is already open, resolve it as cancelled before opening a new one.
    resolveCurrent(false);

    return new Promise<boolean>((resolve) => {
        resolver = resolve;
        current.value = {
            id: `confirm-${Date.now()}-${idCounter++}`,
            title: options.title ?? 'Please confirm',
            message: options.message,
            confirmLabel: options.confirmLabel ?? 'Confirm',
            cancelLabel: options.cancelLabel ?? 'Cancel',
            danger: options.danger ?? false,
        };
    });
};

export const useConfirm = () => {
    return {
        current: readonly(current),
        confirm,
        accept: () => resolveCurrent(true),
        cancel: () => resolveCurrent(false),
    };
};
