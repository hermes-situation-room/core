namespace Hermes.SituationRoom.Domain.Services;

using Hermes.SituationRoom.Data.Interface;
using Hermes.SituationRoom.Domain.Interfaces;
using Hermes.SituationRoom.Shared.BusinessObjects;

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;

    public ReportService(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<ReportBo?> GetByIdAsync(Guid uid)
    {
        return await _reportRepository.GetByIdAsync(uid);
    }

    public async Task<IEnumerable<ReportBo>> GetAllAsync()
    {
        return await _reportRepository.GetAllAsync();
    }

    public async Task<IEnumerable<ReportBo>> GetByStatusAsync(string status)
    {
        return await _reportRepository.GetByStatusAsync(status);
    }

    public async Task<IEnumerable<ReportBo>> GetByReporterUidAsync(Guid reporterUid)
    {
        return await _reportRepository.GetByReporterUidAsync(reporterUid);
    }

    public async Task<IEnumerable<ReportBo>> GetByTargetAsync(string targetType, Guid targetUid)
    {
        return await _reportRepository.GetByTargetAsync(targetType, targetUid);
    }

    public async Task<ReportBo> CreateReportAsync(Guid reporterUid, string targetType, Guid targetUid, string reason)
    {
        var report = new ReportBo(
            Guid.Empty, // Will be generated in repository
            reporterUid,
            targetType,
            targetUid,
            reason,
            "Pending",
            DateTime.UtcNow,
            null,
            null,
            null
        );

        return await _reportRepository.CreateAsync(report);
    }

    public async Task<ReportBo> ReviewReportAsync(Guid reportUid, Guid reviewerUid, string status, string adminNotes)
    {
        var existingReport = await _reportRepository.GetByIdAsync(reportUid);
        if (existingReport == null)
        {
            throw new KeyNotFoundException($"Report with UID {reportUid} not found.");
        }

        var updatedReport = existingReport with
        {
            Status = status,
            ReviewedByUid = reviewerUid,
            ReviewedAt = DateTime.UtcNow,
            AdminNotes = adminNotes
        };

        return await _reportRepository.UpdateAsync(updatedReport);
    }

    public async Task DeleteAsync(Guid uid)
    {
        await _reportRepository.DeleteAsync(uid);
    }
}

