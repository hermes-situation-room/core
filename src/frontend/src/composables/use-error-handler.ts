import { useErrorStore } from '../stores/error-store'
import type { BaseResultBo } from '../services/models/bo/base-result-bo'

/**
 * Composable for handling API errors consistently across components
 */
export function useErrorHandler() {
    const errorStore = useErrorStore()

    /**
     * Handle a result from an API call
     * Returns true if successful, false if there was an error
     */
    function handleResult<T>(result: BaseResultBo<T>): boolean {
        if (!result.isSuccess && result.error) {
            errorStore.addError(result.error)
            return false
        }
        return result.isSuccess
    }

    /**
     * Execute an async operation and handle any errors
     * Returns the result if successful, undefined if there was an error
     */
    async function withErrorHandling<T>(
        operation: () => Promise<BaseResultBo<T>>,
        customErrorMessage?: string
    ): Promise<T | undefined> {
        try {
            const result = await operation()
            
            if (result.isSuccess) {
                return result.data
            } else if (result.error) {
                errorStore.addError(result.error)
            } else if (customErrorMessage) {
                errorStore.addError({
                    category: 'unknown',
                    message: customErrorMessage
                })
            }
            
            return undefined
        } catch (error) {
            console.error('Unexpected error:', error)
            errorStore.addError({
                category: 'unknown',
                message: customErrorMessage || 'An unexpected error occurred. Please try again.'
            })
            return undefined
        }
    }

    return {
        handleResult,
        withErrorHandling
    }
}

