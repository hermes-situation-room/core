import { ref } from 'vue';

export type Theme = 'light' | 'dark';

const THEME_STORAGE_KEY = 'situation-room-theme';

// Shared state across all instances
const currentTheme = ref<Theme>('light');
const isInitialized = ref(false);

export function useTheme() {
    const getPreferredTheme = (): Theme => {
        const stored = localStorage.getItem(THEME_STORAGE_KEY);
        if (stored === 'dark' || stored === 'light') {
            return stored;
        }
        
        // Check system preference
        if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
            return 'dark';
        }
        
        return 'light';
    };

    const applyTheme = (theme: Theme) => {
        const html = document.documentElement;
        html.setAttribute('data-theme', theme);
        
        // Also set a class for easier CSS targeting
        if (theme === 'dark') {
            html.classList.add('dark-theme');
            html.classList.remove('light-theme');
        } else {
            html.classList.add('light-theme');
            html.classList.remove('dark-theme');
        }
    };

    const setTheme = (theme: Theme) => {
        currentTheme.value = theme;
        localStorage.setItem(THEME_STORAGE_KEY, theme);
        applyTheme(theme);
    };

    const toggleTheme = () => {
        const newTheme = currentTheme.value === 'light' ? 'dark' : 'light';
        setTheme(newTheme);
    };

    // Initialize theme on mount
    if (!isInitialized.value) {
        const preferredTheme = getPreferredTheme();
        currentTheme.value = preferredTheme;
        applyTheme(preferredTheme);
        isInitialized.value = true;

        // Watch for system theme changes
        if (window.matchMedia) {
            const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
            mediaQuery.addEventListener('change', (e) => {
                // Only update if user hasn't set a preference
                const stored = localStorage.getItem(THEME_STORAGE_KEY);
                if (!stored) {
                    const newTheme = e.matches ? 'dark' : 'light';
                    currentTheme.value = newTheme;
                    applyTheme(newTheme);
                }
            });
        }
    }

    return {
        theme: currentTheme,
        setTheme,
        toggleTheme,
        isDark: () => currentTheme.value === 'dark',
        isLight: () => currentTheme.value === 'light',
    };
}

