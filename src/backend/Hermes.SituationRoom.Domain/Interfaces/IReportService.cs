namespace Hermes.SituationRoom.Domain.Interfaces;

using Hermes.SituationRoom.Shared.BusinessObjects;

public interface IReportService
{
    Task<ReportBo?> GetByIdAsync(Guid uid);
    Task<IEnumerable<ReportBo>> GetAllAsync();
    Task<IEnumerable<ReportBo>> GetByStatusAsync(string status);
    Task<IEnumerable<ReportBo>> GetByReporterUidAsync(Guid reporterUid);
    Task<IEnumerable<ReportBo>> GetByTargetAsync(string targetType, Guid targetUid);
    Task<ReportBo> CreateReportAsync(Guid reporterUid, string targetType, Guid targetUid, string reason);
    Task<ReportBo> ReviewReportAsync(Guid reportUid, Guid reviewerUid, string status, string adminNotes);
    Task DeleteAsync(Guid uid);
}

