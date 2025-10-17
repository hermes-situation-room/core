export interface LoginActivistDto {
    userName: string;
    password: string;
}

export interface LoginJournalistDto {
    emailAddress: string;
    password: string;
}

export interface RegisterActivistDto {
    userName: string;
    password: string;
    firstName?: string;
    lastName?: string;
    emailAddress?: string;
    isFirstNameVisible: boolean;
    isLastNameVisible: boolean;
    isEmailVisible: boolean;
    profileIcon?: string;
    profileIconColor?: string;
}

export interface RegisterJournalistDto {
    firstName: string;
    lastName: string;
    emailAddress: string;
    password: string;
    employer: string;
    profileIcon?: string;
    profileIconColor?: string;
}

export interface CurrentUserResponse {
    userId: string;
    userType: string;
}
