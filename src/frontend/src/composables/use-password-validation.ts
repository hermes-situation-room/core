import { computed, ref } from 'vue';

export interface PasswordValidation {
    isValid: boolean;
    errors: string[];
    checks: {
        minLength: boolean;
        hasUppercase: boolean;
        hasLowercase: boolean;
        hasNumber: boolean;
        hasSpecialChar: boolean;
    };
}

export function usePasswordValidation() {
    const MIN_PASSWORD_LENGTH = 12;
    
    const validatePassword = (password: string): PasswordValidation => {
        const checks = {
            minLength: password.length >= MIN_PASSWORD_LENGTH,
            hasUppercase: /[A-Z]/.test(password),
            hasLowercase: /[a-z]/.test(password),
            hasNumber: /[0-9]/.test(password),
            hasSpecialChar: /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(password),
        };

        const errors: string[] = [];
        
        if (!checks.minLength) {
            errors.push(`Password must be at least ${MIN_PASSWORD_LENGTH} characters long`);
        }
        if (!checks.hasUppercase) {
            errors.push('Password must contain at least one uppercase letter');
        }
        if (!checks.hasLowercase) {
            errors.push('Password must contain at least one lowercase letter');
        }
        if (!checks.hasNumber) {
            errors.push('Password must contain at least one number');
        }
        if (!checks.hasSpecialChar) {
            errors.push('Password must contain at least one special character');
        }

        return {
            isValid: Object.values(checks).every(check => check),
            errors,
            checks,
        };
    };

    return {
        validatePassword,
        MIN_PASSWORD_LENGTH,
    };
}

// Reactive password validation for forms
export function useReactivePasswordValidation(password: ReturnType<typeof ref<string>>) {
    const { validatePassword, MIN_PASSWORD_LENGTH } = usePasswordValidation();
    
    const validation = computed(() => validatePassword(password.value));
    
    const isValid = computed(() => validation.value.isValid);
    const errors = computed(() => validation.value.errors);
    const checks = computed(() => validation.value.checks);
    
    return {
        validation,
        isValid,
        errors,
        checks,
        MIN_PASSWORD_LENGTH,
    };
}

