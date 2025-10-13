import { reactive, computed } from 'vue';
import type { UserType, ActivistBo, JournalistBo } from '../types/user';

interface AuthState {
    isAuthenticated: boolean;
    userType: UserType | null;
    userId: string | null;
    userData: ActivistBo | JournalistBo | null;
}

const state = reactive<AuthState>({
    isAuthenticated: false,
    userType: null,
    userId: null,
    userData: null
});

const initAuth = () => {
    const storedAuth = localStorage.getItem('auth');
    if (storedAuth) {
        try {
            const parsedAuth = JSON.parse(storedAuth);
            state.isAuthenticated = parsedAuth.isAuthenticated;
            state.userType = parsedAuth.userType;
            state.userId = parsedAuth.userId;
            state.userData = parsedAuth.userData;
        } catch (e) {
            console.error('Failed to parse stored auth', e);
            localStorage.removeItem('auth');
        }
    }
};

const saveAuthToStorage = () => {
    localStorage.setItem('auth', JSON.stringify({
        isAuthenticated: state.isAuthenticated,
        userType: state.userType,
        userId: state.userId,
        userData: state.userData
    }));
};

export const useAuthStore = () => {
    const isAuthenticated = computed(() => state.isAuthenticated);
    const userType = computed(() => state.userType);
    const userId = computed(() => state.userId);
    const userData = computed(() => state.userData);
    const isActivist = computed(() => state.userType === 'activist');
    const isJournalist = computed(() => state.userType === 'journalist');

    const login = (type: UserType, uid: string, data?: ActivistBo | JournalistBo) => {
        state.isAuthenticated = true;
        state.userType = type;
        state.userId = uid;
        state.userData = data || null;
        saveAuthToStorage();
    };

    const logout = () => {
        state.isAuthenticated = false;
        state.userType = null;
        state.userId = null;
        state.userData = null;
        localStorage.removeItem('auth');
    };

    const updateUserData = (data: ActivistBo | JournalistBo) => {
        state.userData = data;
        saveAuthToStorage();
    };

    return {
        isAuthenticated,
        userType,
        userId,
        userData,
        isActivist,
        isJournalist,
        login,
        logout,
        updateUserData,
        initAuth
    };
};

