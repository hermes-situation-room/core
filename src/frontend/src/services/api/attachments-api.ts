import type { BaseResultBo } from "../models/bo/base-result-bo";
import ApiBaseClient from "./base/api-base-client";

/** Public-facing attachment metadata (mirrors backend AttachmentDto; never exposes key material). */
export interface AttachmentDto {
    uid: string;
    ownerUid: string;
    fileName: string;
    contentType: string;
    sizeBytes: number;
    postUid?: string | null;
    messageUid?: string | null;
    createdAt: string;
}

const backendBaseUrl: string = import.meta.env.VITE_APP_BACKEND;

export default function attachmentsApi(_apiBaseClient: ApiBaseClient) {
    return {
        /**
         * Upload a single file via multipart/form-data. The file should already be redacted
         * client-side (the original never leaves the device). Optionally links it to an existing
         * post/message; usually called WITHOUT an id and the returned uid is passed as
         * `attachmentIds` on post/message creation so the backend links it.
         */
        async upload(
            file: File | Blob,
            options?: { fileName?: string; postId?: string; messageId?: string }
        ): Promise<BaseResultBo<AttachmentDto>> {
            const formData = new FormData();
            const fileName = options?.fileName
                ?? (file instanceof File ? file.name : 'file');
            formData.append('file', file, fileName);
            if (options?.postId) {
                formData.append('postId', options.postId);
            }
            if (options?.messageId) {
                formData.append('messageId', options.messageId);
            }

            try {
                const response = await fetch(`${backendBaseUrl}services/api/internal/attachment`, {
                    method: 'POST',
                    credentials: 'include',
                    body: formData,
                });

                const contentType = response.headers.get('content-type');
                const data = contentType?.includes('application/json')
                    ? await response.json()
                    : await response.text();

                return {
                    data: response.ok ? (data as AttachmentDto) : undefined,
                    responseCode: response.status,
                    responseMessage: response.ok
                        ? 'Success'
                        : (typeof data === 'object' && data?.detail) || (typeof data === 'object' && data?.title) || response.statusText || 'Upload failed',
                    isSuccess: response.ok,
                };
            } catch (e) {
                return {
                    data: undefined,
                    responseCode: undefined,
                    responseMessage: e instanceof Error ? e.message : String(e),
                    isSuccess: false,
                };
            }
        },

        /**
         * Absolute URL of the authorized download endpoint for an attachment. Suitable as an
         * <img>/<video> src or anchor href; the browser sends the auth cookie (credentials).
         */
        getDownloadUrl(uid: string): string {
            return `${backendBaseUrl}services/api/internal/attachment/${uid}`;
        },

        /**
         * Fetch the decrypted bytes of an attachment as a Blob via the authorized endpoint.
         * Use when an object URL is needed (e.g. forcing download with the real file name).
         */
        async download(uid: string): Promise<BaseResultBo<Blob>> {
            try {
                const response = await fetch(this.getDownloadUrl(uid), {
                    method: 'GET',
                    credentials: 'include',
                });
                return {
                    data: response.ok ? await response.blob() : undefined,
                    responseCode: response.status,
                    responseMessage: response.ok ? 'Success' : response.statusText,
                    isSuccess: response.ok,
                };
            } catch (e) {
                return {
                    data: undefined,
                    responseCode: undefined,
                    responseMessage: e instanceof Error ? e.message : String(e),
                    isSuccess: false,
                };
            }
        },

        /** Metadata of the attachments linked to a post the requester may see. */
        async getByPost(postId: string): Promise<BaseResultBo<AttachmentDto[]>> {
            return await _apiBaseClient.get<AttachmentDto[]>(`services/api/internal/attachment/by-post/${postId}`);
        },

        /** Metadata of the attachments linked to a message in a chat the requester belongs to. */
        async getByMessage(messageId: string): Promise<BaseResultBo<AttachmentDto[]>> {
            return await _apiBaseClient.get<AttachmentDto[]>(`services/api/internal/attachment/by-message/${messageId}`);
        },

        /** Delete an attachment (owner only). */
        async delete(uid: string): Promise<BaseResultBo<undefined>> {
            return await _apiBaseClient.delete(`services/api/internal/attachment/${uid}`);
        },
    };
}
