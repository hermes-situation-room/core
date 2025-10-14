export interface ProblemDetails {
    /** A URI reference that identifies the problem type */
    type?: string;
    
    /** A short, human-readable summary of the problem type */
    title?: string;
    
    /** The HTTP status code */
    status?: number;
    
    /** A human-readable explanation specific to this occurrence of the problem */
    detail?: string;
    
    /** A URI reference that identifies the specific occurrence of the problem */
    instance?: string;
    
    /** The trace ID for debugging purposes */
    traceId?: string;
    
    /** Timestamp when the error occurred */
    timestamp?: string;
    
    /** Validation errors keyed by field name */
    errors?: Record<string, string[]>;
    
    /** Resource type (for ResourceNotFoundException) */
    resourceType?: string;
    
    /** Resource ID (for ResourceNotFoundException) */
    resourceId?: string;
    
    /** Conflicting field (for DuplicateResourceException) */
    conflictingField?: string;
}

/**
 * User-friendly error categories for display purposes
 */
export type ErrorCategory = 
    | 'authentication'
    | 'validation'
    | 'not_found'
    | 'conflict'
    | 'permission'
    | 'server'
    | 'network'
    | 'unknown';

/**
 * Application error with parsed ProblemDetails and user-friendly message
 */
export interface AppError {
    /** The error category for UI handling */
    category: ErrorCategory;
    
    /** User-friendly error message */
    message: string;
    
    /** Original problem details from API */
    details?: ProblemDetails;
    
    /** Field-specific validation errors */
    fieldErrors?: Record<string, string[]>;
}

