export interface PrivacyLevelPersonalBo {
    uid: string;
    isFirstNameVisible?: boolean;
    isLastNameVisible?: boolean;
    isEmailVisible?: boolean;
    ownerUid: string;
    consumerUid: string;
}

export interface CreatePrivacyLevelPersonalDto {
    isFirstNameVisible?: boolean;
    isLastNameVisible?: boolean;
    isEmailVisible?: boolean;
    ownerUid: string;
    consumerUid: string;
}

export interface UpdatePrivacyLevelPersonalDto {
    isFirstNameVisible?: boolean;
    isLastNameVisible?: boolean;
    isEmailVisible?: boolean;
}