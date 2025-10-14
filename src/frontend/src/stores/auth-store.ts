import {computed, reactive} from 'vue';
import type {UserType} from '../types/user';
import {services} from "../services/api";

interface AuthState {
    isAuthenticated: boolean;
    userType: UserType | null;
    userId: string | null;
    isLoading: boolean;
}

const state = reactive<AuthState>({
    isAuthenticated: false,
    userType: null,
    userId: null,
    isLoading: false
});

/**
 * Initialize authentication from server cookie
 * This is called on app startup to check if user is already authenticated
 */
const initAuth = async () => {
    state.isLoading = true;
    try {
        const response = await services.auth.getCurrentUser();

        if (response.isSuccess && response.data) {
            state.isAuthenticated = true;
            state.userType = response.data.userType as UserType;
            state.userId = response.data.userId;
        } else {
            clearAuthState();
        }
    } catch (error) {
        console.error('Failed to initialize auth', error);
        clearAuthState();
    } finally {
        state.isLoading = false;
    }
};

const clearAuthState = () => {
    state.isAuthenticated = false;
    state.userType = null;
    state.userId = null;
};

const refreshUserData = async () => {
    try {
        const response = await services.auth.getCurrentUser();

        if (response.isSuccess && response.data) {
            state.isAuthenticated = true;
            state.userType = response.data.userType as UserType;
            state.userId = response.data.userId;
            return true;
        } else {
            clearAuthState();
            return false;
        }
    } catch (error) {
        console.error('Failed to refresh user data', error);
        clearAuthState();
        return false;
    }
};

export const useAuthStore = () => {
    const isAuthenticated = computed(() => state.isAuthenticated);
    const userType = computed(() => state.userType);
    const userId = computed(() => state.userId);
    const isActivist = computed(() => state.userType === 'activist');
    const isJournalist = computed(() => state.userType === 'journalist');
    const isLoading = computed(() => state.isLoading);

    /**
     * Login user - sets authentication state after successful login
     * Note: The actual authentication cookie is set by the backend
     */
    const login = async () => {
        await refreshUserData();
    };

    /**
     * Logout user - clears cookie on server and local state
     */
    const logout = async () => {
        try {
            await services.auth.logout();
        } catch (error) {
            console.error('Logout error:', error);
        } finally {
            clearAuthState();
        }
    };

    /**
     * Update user data - fetches fresh data from server
     */
    const updateUserData = async () => {
        await refreshUserData();
    };

    return {
        isAuthenticated,
        userType,
        userId,
        isActivist,
        isJournalist,
        isLoading,
        login,
        logout,
        updateUserData,
        initAuth,
        clearAuthState
    };
};
