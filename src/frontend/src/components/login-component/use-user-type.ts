import { ref, computed } from "vue";
import type { UserType } from "../../types/user";

export function useUserType() {
    const selectedUserType = ref<UserType>("activist");

    const isJournalist = computed(() => selectedUserType.value === "journalist");
    const isActivist = computed(() => selectedUserType.value === "activist");

    function selectUserType(type: UserType) {
        selectedUserType.value = type;
    }

    return {
        selectedUserType,
        isJournalist,
        isActivist,
        selectUserType,
    };
}
