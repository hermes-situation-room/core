export interface BaseResultBo<T> {
    data?: T
    responseCode?: number,
    responseMessage: string,
    isSuccess: boolean
}