export interface PrivacyLevelPersonalBo {
    uid: string;
    isFirstNameVisible: boolean;
    isLastNameVisible: boolean;
    isEmailVisible: boolean;
    ownerUid: string;
    consumerUid: string;
}

export interface CreatePrivacyLevelDto {
    isFirstNameVisible: boolean;
    isLastNameVisible: boolean;
    isEmailVisible: boolean;
    ownerUid: string;
    consumerUid: string;
}

export interface UpdatePrivacyLevelDto {
    isFirstNameVisible: boolean;
    isLastNameVisible: boolean;
    isEmailVisible: boolean;
}