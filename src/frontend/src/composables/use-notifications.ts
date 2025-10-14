import { useNotificationStore } from '../stores/notification-store'

/**
 * Composable for showing success notifications
 */
export function useNotifications() {
    const notificationStore = useNotificationStore()

    /**
     * Show a registration success message
     */
    function showRegistrationSuccess(userType: 'activist' | 'journalist'): string {
        return notificationStore.addSuccess({
            category: 'registration',
            title: 'Registration Successful!',
            message: userType === 'activist'
                ? 'Your activist account has been created. Please log in to continue.'
                : 'Your journalist account has been created. Please log in to continue.'
        })
    }

    /**
     * Show a login success message
     */
    function showLoginSuccess(): string {
        return notificationStore.addSuccess({
            category: 'login',
            title: 'Welcome back!',
            message: 'You have successfully logged in.'
        })
    }

    /**
     * Show a create success message
     */
    function showCreateSuccess(resourceName: string): string {
        return notificationStore.addSuccess({
            category: 'create',
            title: 'Created!',
            message: `${resourceName} has been created successfully.`
        })
    }

    /**
     * Show an update success message
     */
    function showUpdateSuccess(resourceName: string): string {
        return notificationStore.addSuccess({
            category: 'update',
            title: 'Updated!',
            message: `${resourceName} has been updated successfully.`
        })
    }

    /**
     * Show a delete success message
     */
    function showDeleteSuccess(resourceName: string): string {
        return notificationStore.addSuccess({
            category: 'delete',
            title: 'Deleted!',
            message: `${resourceName} has been deleted successfully.`
        })
    }

    return {
        showRegistrationSuccess,
        showLoginSuccess,
        showCreateSuccess,
        showUpdateSuccess,
        showDeleteSuccess
    }
}

