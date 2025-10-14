import { ref, computed } from "vue";
import type { LoginFormData, UserType } from "../../types/user";

export function useUserType() {
    const selectedUserType = ref<UserType>("activist");
    const formData = ref<LoginFormData>({
        userType: "activist",
        userName: "",
        password: "",
        firstName: "",
        lastName: "",
        emailAddress: "",
        employer: "",
        isFirstNameVisible: true,
        isLastNameVisible: true,
        isEmailVisible: true,
    });

    const isJournalist = computed(() => selectedUserType.value === "journalist");
    const isActivist = computed(() => selectedUserType.value === "activist");

    function selectUserType(type: UserType) {
        selectedUserType.value = type;
        formData.value = {
            userType: type,
            userName: "",
            password: "",
            firstName: "",
            lastName: "",
            emailAddress: "",
            employer: "",
            isFirstNameVisible: true,
            isLastNameVisible: true,
            isEmailVisible: true,
        };
    }

    return {
        selectedUserType,
        formData,
        isJournalist,
        isActivist,
        selectUserType,
    };
}
