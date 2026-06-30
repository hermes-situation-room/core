export interface UserProfileBo {
    uid: string;
    firstName?: string;
    lastName?: string;
    emailAddress?: string;
    userName?: string;
    employer?: string;
    profileIcon?: string;
    profileIconColor?: string;
    bio?: string;
    publishedWorkLink?: string;
    linkedInUrl?: string;
    twitterUrl?: string;
    websiteUrl?: string;
    specializations?: string[];
}

export interface ActivistBo {
    uid: string;
    password: string;
    firstName?: string;
    lastName?: string;
    emailAddress?: string;
    profileIcon?: string;
    profileIconColor?: string;
    userName: string;
    isFirstNameVisible: boolean;
    isLastNameVisible: boolean;
    isEmailVisible: boolean;
}

export interface JournalistBo {
    uid: string;
    password: string;
    firstName: string;
    lastName: string;
    emailAddress: string;
    profileIcon?: string;
    profileIconColor?: string;
    employer: string;
    bio?: string;
    publishedWorkLink?: string;
    linkedInUrl?: string;
    twitterUrl?: string;
    websiteUrl?: string;
    specializations?: string[];
}

export type UserType = 'activist' | 'journalist';

export interface LoginFormData {
    userType: UserType;
    userName: string;
    password: string;
    firstName?: string;
    lastName?: string;
    emailAddress?: string;
    employer?: string;
    isFirstNameVisible?: boolean;
    isLastNameVisible?: boolean;
    isEmailVisible?: boolean;
    profileIcon?: string;
    profileIconColor?: string;
}
