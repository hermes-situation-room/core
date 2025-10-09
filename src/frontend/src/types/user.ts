export interface ActivistBo {
  uid: string;
  password: string;
  firstName?: string;
  lastName?: string;
  emailAddress?: string;
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
  employer: string;
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
}
