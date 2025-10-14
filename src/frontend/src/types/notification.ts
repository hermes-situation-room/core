/**
 * Success notification categories
 */
export type SuccessCategory = 
    | 'registration'
    | 'login'
    | 'update'
    | 'delete'
    | 'create'
    | 'general';

/**
 * Success notification message
 */
export interface SuccessNotification {
    category: SuccessCategory;
    message: string;
    title?: string;
}

