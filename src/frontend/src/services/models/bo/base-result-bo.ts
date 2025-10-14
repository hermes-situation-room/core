import type { AppError } from '../../../types/error'

export interface BaseResultBo<T> {
    data?: T
    responseCode?: number,
    responseMessage: string,
    isSuccess: boolean,
    error?: AppError
}