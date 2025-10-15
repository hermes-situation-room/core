import type { ProblemDetails, AppError, ErrorCategory } from '../types/error'

/**
 * Categorizes an error based on status code and problem details
 */
function categorizeError(status?: number): ErrorCategory {
    if (!status) {
        return 'network'
    }

    switch (status) {
        case 401:
            return 'authentication'
        case 403:
            return 'permission'
        case 404:
            return 'not_found'
        case 409:
            return 'conflict'
        case 400:
        case 422:
            return 'validation'
        case 500:
        case 502:
        case 503:
        case 504:
            return 'server'
        default:
            return 'unknown'
    }
}

/**
 * Generates a user-friendly error message from problem details
 */
function generateUserMessage(category: ErrorCategory, details?: ProblemDetails): string {
    // Use the detail from problem details if available
    if (details?.detail) {
        return details.detail
    }

    // Use the title from problem details if available
    if (details?.title) {
        return details.title
    }

    // Fallback to category-based messages
    switch (category) {
        case 'authentication':
            return 'Authentication failed. Please check your credentials and try again.'
        case 'permission':
            return 'You do not have permission to perform this action.'
        case 'not_found':
            return details?.resourceType 
                ? `${details.resourceType} not found.`
                : 'The requested resource was not found.'
        case 'conflict':
            if (details?.detail && details.detail.includes('already in use')) {
                return details.detail;
            }
            return details?.conflictingField
                ? `A resource with this ${details.conflictingField} already exists.`
                : 'This resource already exists.'
        case 'validation':
            return 'Please check your input and try again.'
        case 'server':
            return 'A server error occurred. Please try again later.'
        case 'network':
            return 'Network error. Please check your connection and try again.'
        default:
            return 'An unexpected error occurred. Please try again.'
    }
}

/**
 * Parses an error response into an AppError
 */
export async function parseErrorResponse(response: Response): Promise<AppError> {
    let problemDetails: ProblemDetails | undefined

    try {
        const contentType = response.headers.get('content-type')
        if (contentType?.includes('application/problem+json') || contentType?.includes('application/json')) {
            const json = await response.json()
            problemDetails = {
                type: json.type,
                title: json.title,
                status: json.status,
                detail: json.detail,
                instance: json.instance,
                traceId: json.traceId,
                timestamp: json.timestamp,
                errors: json.errors,
                resourceType: json.resourceType,
                resourceId: json.resourceId,
                conflictingField: json.conflictingField
            }
        }
    } catch (e) {
        // If we can't parse the response, continue with undefined problemDetails
        console.error('Failed to parse error response:', e)
    }

    const category = categorizeError(response.status)
    const message = generateUserMessage(category, problemDetails)

    return {
        category,
        message,
        details: problemDetails,
        fieldErrors: problemDetails?.errors
    }
}

/**
 * Parses a network or unknown error into an AppError
 */
export function parseNetworkError(error: Error | unknown): AppError {
    const message = error instanceof Error ? error.message : String(error)
    
    return {
        category: 'network',
        message: `Network error: ${message}`,
        details: undefined,
        fieldErrors: undefined
    }
}

