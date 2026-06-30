namespace Hermes.SituationRoom.Data.Repositories;

using Entities;
using Interface;
using Microsoft.EntityFrameworkCore;
using Shared.BusinessObjects;

public sealed class ContentFilterRuleRepository(IHermessituationRoomContext context) : IContentFilterRuleRepository
{
    public async Task<Guid> AddAsync(ContentFilterRuleBo contentFilterRuleBo)
    {
        ArgumentNullException.ThrowIfNull(contentFilterRuleBo);

        var newRule = new ContentFilterRule
        {
            Uid = Guid.NewGuid(),
            Pattern = contentFilterRuleBo.Pattern,
            Action = contentFilterRuleBo.Action,
            Enabled = contentFilterRuleBo.Enabled,
            CreatedAt = DateTime.UtcNow
        };

        context.ContentFilterRules.Add(newRule);
        await context.SaveChangesAsync();

        return newRule.Uid;
    }

    public async Task<ContentFilterRuleBo?> GetByIdAsync(Guid uid)
    {
        var rule = await context.ContentFilterRules
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Uid == uid);

        return rule == null ? null : MapToBo(rule);
    }

    public async Task<IReadOnlyList<ContentFilterRuleBo>> GetAllAsync()
    {
        return await context.ContentFilterRules
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => MapToBo(r))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ContentFilterRuleBo>> GetEnabledAsync()
    {
        return await context.ContentFilterRules
            .AsNoTracking()
            .Where(r => r.Enabled)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => MapToBo(r))
            .ToListAsync();
    }

    public async Task<bool> ExistsByPatternAsync(string pattern)
    {
        return await context.ContentFilterRules
            .AnyAsync(r => r.Pattern == pattern);
    }

    public async Task<ContentFilterRuleBo> UpdateAsync(ContentFilterRuleBo contentFilterRuleBo)
    {
        var rule = await context.ContentFilterRules
            .FirstOrDefaultAsync(r => r.Uid == contentFilterRuleBo.Uid);

        if (rule == null)
        {
            throw new KeyNotFoundException($"ContentFilterRule with UID {contentFilterRuleBo.Uid} not found.");
        }

        rule.Pattern = contentFilterRuleBo.Pattern;
        rule.Action = contentFilterRuleBo.Action;
        rule.Enabled = contentFilterRuleBo.Enabled;

        await context.SaveChangesAsync();

        return MapToBo(rule);
    }

    public async Task DeleteAsync(Guid uid)
    {
        var rule = await context.ContentFilterRules
            .FirstOrDefaultAsync(r => r.Uid == uid);

        if (rule != null)
        {
            context.ContentFilterRules.Remove(rule);
            await context.SaveChangesAsync();
        }
    }

    private static ContentFilterRuleBo MapToBo(ContentFilterRule rule) => new(
        rule.Uid,
        rule.Pattern,
        rule.Action,
        rule.Enabled,
        rule.CreatedAt
    );
}
