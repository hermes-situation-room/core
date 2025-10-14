import { ref } from "vue";
import { useRouter } from "vue-router";
import { services } from "../../services/api";
import { useAuthStore } from "../../stores/auth-store";

export function useLogin() {
    const router = useRouter();
    const authStore = useAuthStore();

    const loginData = ref({
        username: "",
        email: "",
        password: "",
    });
    const errorMessage = ref("");
    const isLoading = ref(false);

    async function handleLogin(isActivist: boolean) {
        if (isLoading.value) return;

        errorMessage.value = "";
        isLoading.value = true;

        try {
            let result;

            if (isActivist) {
                result = await services.auth.loginActivist({
                    userName: loginData.value.username,
                    password: loginData.value.password,
                });
            } else {
                result = await services.auth.loginJournalist({
                    emailAddress: loginData.value.email,
                    password: loginData.value.password,
                });
            }

            if (result.isSuccess) {
                await authStore.login();

                if (authStore.isAuthenticated.value) {
                    await router.push("/");
                } else {
                    errorMessage.value = "Failed to fetch user data. Please try again.";
                }
            } else {
                errorMessage.value = result.responseMessage || "Login failed. Please check your credentials.";
            }
        } catch (error) {
            console.error("Login error:", error);
            errorMessage.value = "An error occurred during login. Please try again.";
        } finally {
            isLoading.value = false;
        }
    }

    return {
        loginData,
        errorMessage,
        isLoading,
        handleLogin,
    };
}
