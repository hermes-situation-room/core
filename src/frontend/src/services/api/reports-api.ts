import type { BaseResultBo } from "../models/bo/base-result-bo";
import ApiBaseClient from "./base/api-base-client";

/**
 * Mirrors the backend ReportBo / ReportDto. Target type is a string such as
 * "Post", "Comment" or "User"; the target uid is a guid string.
 */
export interface ReportDto {
    uid: string;
    reporterUid: string;
    targetType: string;
    targetUid: string;
    reason: string;
    status: string;
    timestamp: string;
    reviewedByUid: string | null;
    reviewedAt: string | null;
    adminNotes: string;
}

/**
 * Body for POST internal/report. The backend overrides reporterUid with the
 * authenticated user, but the DTO still requires the field, so we send the
 * current user's uid for completeness.
 */
export interface CreateReportDto {
    reporterUid: string;
    targetType: string;
    targetUid: string;
    reason: string;
}

/**
 * Body for PUT internal/report/{uid}/review.
 */
export interface ReviewReportDto {
    reviewedByUid: string;
    status: string;
    adminNotes: string;
}

export default function reportsApi(apiBaseClient: ApiBaseClient) {
    return {
        /**
         * Create a report against a post, comment or user. Any authenticated user may call this.
         */
        async createReport(report: CreateReportDto): Promise<BaseResultBo<string>> {
            return await apiBaseClient.post<CreateReportDto>(`services/api/internal/report`, report);
        },

        /**
         * Get the authenticated user's own submitted reports.
         */
        async getMyReports(): Promise<BaseResultBo<ReportDto[]>> {
            return await apiBaseClient.get<ReportDto[]>(`services/api/internal/report/my-reports`);
        },

        /**
         * Admin-only: list all reports.
         */
        async getAllReports(): Promise<BaseResultBo<ReportDto[]>> {
            return await apiBaseClient.get<ReportDto[]>(`services/api/internal/report`);
        },

        /**
         * Admin-only: list reports filtered by status (e.g. "Pending", "Reviewed", "Resolved", "Rejected").
         */
        async getByStatus(status: string): Promise<BaseResultBo<ReportDto[]>> {
            return await apiBaseClient.get<ReportDto[]>(`services/api/internal/report/status/${encodeURIComponent(status)}`);
        },

        /**
         * Admin-only: list reports for a given target type and target uid.
         */
        async getByTarget(targetType: string, targetUid: string): Promise<BaseResultBo<ReportDto[]>> {
            return await apiBaseClient.get<ReportDto[]>(`services/api/internal/report/target/${encodeURIComponent(targetType)}/${targetUid}`);
        },

        /**
         * Admin-only: fetch a single report by uid.
         */
        async getById(uid: string): Promise<BaseResultBo<ReportDto>> {
            return await apiBaseClient.get<ReportDto>(`services/api/internal/report/${uid}`);
        },

        /**
         * Admin-only: review a report, setting its status and optional admin notes.
         */
        async reviewReport(uid: string, review: ReviewReportDto): Promise<BaseResultBo<string>> {
            return await apiBaseClient.put<ReviewReportDto>(`services/api/internal/report/${uid}/review`, review);
        },

        /**
         * Admin-only: delete a report.
         */
        async deleteReport(uid: string): Promise<BaseResultBo<undefined>> {
            return await apiBaseClient.delete(`services/api/internal/report/${uid}`);
        },
    };
}
