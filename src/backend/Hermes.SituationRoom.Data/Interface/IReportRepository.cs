namespace Hermes.SituationRoom.Data.Interface;

using Hermes.SituationRoom.Shared.BusinessObjects;

public interface IReportRepository
{
    Task<ReportBo?> GetByIdAsync(Guid uid);
    Task<IEnumerable<ReportBo>> GetAllAsync();
    Task<IEnumerable<ReportBo>> GetByStatusAsync(string status);
    Task<IEnumerable<ReportBo>> GetByReporterUidAsync(Guid reporterUid);
    Task<IEnumerable<ReportBo>> GetByTargetAsync(string targetType, Guid targetUid);
    Task<ReportBo> CreateAsync(ReportBo report);
    Task<ReportBo> UpdateAsync(ReportBo report);
    Task DeleteAsync(Guid uid);
}

