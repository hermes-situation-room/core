namespace Hermes.SituationRoom.Data.Interface;

using Shared.BusinessObjects;

public interface IContentFilterRuleRepository
{
    Task<Guid> AddAsync(ContentFilterRuleBo contentFilterRuleBo);
    Task<ContentFilterRuleBo?> GetByIdAsync(Guid uid);
    Task<IReadOnlyList<ContentFilterRuleBo>> GetAllAsync();
    Task<IReadOnlyList<ContentFilterRuleBo>> GetEnabledAsync();
    Task<bool> ExistsByPatternAsync(string pattern);
    Task<ContentFilterRuleBo> UpdateAsync(ContentFilterRuleBo contentFilterRuleBo);
    Task DeleteAsync(Guid uid);
}
