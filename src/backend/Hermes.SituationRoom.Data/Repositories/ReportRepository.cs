namespace Hermes.SituationRoom.Data.Repositories;

using Entities;
using Interface;
using Microsoft.EntityFrameworkCore;
using Shared.BusinessObjects;

public class ReportRepository(IHermessituationRoomContext context) : IReportRepository
{
    private static ReportBo MapToBo(Report report)
    {
        return new ReportBo(
            report.Uid,
            report.ReporterUid,
            report.TargetType,
            report.TargetUid,
            report.Reason,
            report.Status,
            report.Timestamp,
            report.ReviewedByUid,
            report.ReviewedAt,
            report.AdminNotes
        );
    }

    public async Task<ReportBo?> GetByIdAsync(Guid uid)
    {
        var report = await context.Reports
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Uid == uid);
        return report == null ? null : MapToBo(report);
    }

    public async Task<IEnumerable<ReportBo>> GetAllAsync()
    {
        var reports = await context.Reports
            .AsNoTracking()
            .OrderByDescending(r => r.Timestamp)
            .ToListAsync();
        return reports.Select(MapToBo);
    }

    public async Task<IEnumerable<ReportBo>> GetByStatusAsync(string status)
    {
        var reports = await context.Reports
            .Where(r => r.Status == status)
            .AsNoTracking()
            .OrderByDescending(r => r.Timestamp)
            .ToListAsync();
        return reports.Select(MapToBo);
    }

    public async Task<IEnumerable<ReportBo>> GetByReporterUidAsync(Guid reporterUid)
    {
        var reports = await context.Reports
            .Where(r => r.ReporterUid == reporterUid)
            .AsNoTracking()
            .OrderByDescending(r => r.Timestamp)
            .ToListAsync();
        return reports.Select(MapToBo);
    }

    public async Task<IEnumerable<ReportBo>> GetByTargetAsync(string targetType, Guid targetUid)
    {
        var reports = await context.Reports
            .Where(r => r.TargetType == targetType && r.TargetUid == targetUid)
            .AsNoTracking()
            .OrderByDescending(r => r.Timestamp)
            .ToListAsync();
        return reports.Select(MapToBo);
    }

    public async Task<ReportBo> CreateAsync(ReportBo reportBo)
    {
        var report = new Report
        {
            Uid = Guid.NewGuid(),
            ReporterUid = reportBo.ReporterUid,
            TargetType = reportBo.TargetType,
            TargetUid = reportBo.TargetUid,
            Reason = reportBo.Reason,
            Status = "Pending",
            Timestamp = DateTime.UtcNow,
            ReviewedByUid = null,
            ReviewedAt = null,
            AdminNotes = null
        };
        
        context.Reports.Add(report);
        await context.SaveChangesAsync();
        
        return MapToBo(report);
    }

    public async Task<ReportBo> UpdateAsync(ReportBo reportBo)
    {
        var report = await context.Reports.FirstOrDefaultAsync(r => r.Uid == reportBo.Uid);
        if (report == null)
        {
            throw new KeyNotFoundException($"Report with UID {reportBo.Uid} not found.");
        }

        report.Status = reportBo.Status;
        report.ReviewedByUid = reportBo.ReviewedByUid;
        report.ReviewedAt = reportBo.ReviewedAt;
        report.AdminNotes = reportBo.AdminNotes;
        
        await context.SaveChangesAsync();
        
        return MapToBo(report);
    }

    public async Task DeleteAsync(Guid uid)
    {
        var report = await context.Reports.FirstOrDefaultAsync(r => r.Uid == uid);
        if (report != null)
        {
            context.Reports.Remove(report);
            await context.SaveChangesAsync();
        }
    }
}

