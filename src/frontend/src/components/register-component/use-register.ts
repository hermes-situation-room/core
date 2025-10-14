import { ref } from "vue";
import { useRouter } from "vue-router";
import { services } from "../../services/api";
import { useAuthStore } from "../../stores/auth-store";

export function useRegister(formData: any, isActivist: any) {
    const router = useRouter();
    const authStore = useAuthStore();
    const errorMessage = ref("");
    const isLoading = ref(false);

    async function handleRegister() {
        if (isLoading.value) return;

        errorMessage.value = "";
        isLoading.value = true;

        try {
            let result;

            if (isActivist.value) {
                result = await services.auth.registerActivist({
                    userName: formData.value.userName,
                    password: formData.value.password,
                    firstName: formData.value.firstName || undefined,
                    lastName: formData.value.lastName || undefined,
                    emailAddress: formData.value.emailAddress || undefined,
                    isFirstNameVisible: formData.value.isFirstNameVisible ?? true,
                    isLastNameVisible: formData.value.isLastNameVisible ?? true,
                    isEmailVisible: formData.value.isEmailVisible ?? true,
                });
            } else {
                if (
                    !formData.value.firstName ||
                    !formData.value.lastName ||
                    !formData.value.emailAddress ||
                    !formData.value.employer
                ) {
                    errorMessage.value = "All fields are required for journalist registration.";
                    isLoading.value = false;
                    return;
                }

                result = await services.auth.registerJournalist({
                    firstName: formData.value.firstName,
                    lastName: formData.value.lastName,
                    emailAddress: formData.value.emailAddress,
                    password: formData.value.password,
                    employer: formData.value.employer,
                });
            }

            if (result.isSuccess && result.data) {
                authStore.login();
                await router.push("/");
            } else {
                errorMessage.value = result.responseMessage || "Registration failed. Please try again.";
            }
        } catch (error) {
            console.error("Registration error:", error);
            errorMessage.value = "An error occurred during registration. Please try again.";
        } finally {
            isLoading.value = false;
        }
    }

    return {
        errorMessage,
        isLoading,
        handleRegister,
    };
}
