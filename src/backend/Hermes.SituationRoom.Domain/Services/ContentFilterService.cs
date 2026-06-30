#nullable enable
namespace Hermes.SituationRoom.Domain.Services;

using System.Text.RegularExpressions;
using AutoMapper;
using Data.Interface;
using Interfaces;
using Shared.BusinessObjects;
using Shared.DataTransferObjects;
using Shared.Exceptions;

public sealed class ContentFilterService(
    IContentFilterRuleRepository contentFilterRuleRepository,
    IMapper mapper) : IContentFilterService
{
    private const string ActionRedact = "Redact";
    private const string ActionBlock = "Block";

    public async Task<ContentFilterRuleDto> CreateRuleAsync(string pattern, string action)
    {
        var normalizedPattern = NormalizePattern(pattern);
        var normalizedAction = NormalizeAction(action);

        if (await contentFilterRuleRepository.ExistsByPatternAsync(normalizedPattern))
        {
            throw new ValidationBusinessException(nameof(pattern), $"A rule for pattern '{normalizedPattern}' already exists.");
        }

        var rule = new ContentFilterRuleBo(
            Guid.Empty,
            normalizedPattern,
            normalizedAction,
            true,
            DateTime.UtcNow
        );

        var uid = await contentFilterRuleRepository.AddAsync(rule);
        var created = await contentFilterRuleRepository.GetByIdAsync(uid);

        return mapper.Map<ContentFilterRuleDto>(created);
    }

    public async Task<IReadOnlyList<ContentFilterRuleDto>> GetAllRulesAsync()
    {
        var rules = await contentFilterRuleRepository.GetAllAsync();
        return mapper.Map<IReadOnlyList<ContentFilterRuleDto>>(rules);
    }

    public async Task<ContentFilterRuleDto> UpdateRuleAsync(Guid uid, string pattern, string action, bool enabled)
    {
        var existing = await contentFilterRuleRepository.GetByIdAsync(uid);

        if (existing == null)
        {
            throw new KeyNotFoundException($"ContentFilterRule with UID {uid} not found.");
        }

        var normalizedPattern = NormalizePattern(pattern);
        var normalizedAction = NormalizeAction(action);

        if (!string.Equals(normalizedPattern, existing.Pattern, StringComparison.OrdinalIgnoreCase)
            && await contentFilterRuleRepository.ExistsByPatternAsync(normalizedPattern))
        {
            throw new ValidationBusinessException(nameof(pattern), $"A rule for pattern '{normalizedPattern}' already exists.");
        }

        var updated = existing with
        {
            Pattern = normalizedPattern,
            Action = normalizedAction,
            Enabled = enabled
        };

        var result = await contentFilterRuleRepository.UpdateAsync(updated);
        return mapper.Map<ContentFilterRuleDto>(result);
    }

    public async Task DeleteRuleAsync(Guid uid)
    {
        await contentFilterRuleRepository.DeleteAsync(uid);
    }

    public async Task<(string FilteredText, bool Blocked)> ApplyAsync(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return (text, false);
        }

        var rules = await contentFilterRuleRepository.GetEnabledAsync();

        if (rules.Count == 0)
        {
            return (text, false);
        }

        var blocked = rules
            .Where(rule => rule.Action == ActionBlock)
            .Any(rule => MatchesWholeWord(text, rule.Pattern));

        if (blocked)
        {
            return (text, true);
        }

        var filteredText = rules
            .Where(rule => rule.Action == ActionRedact)
            .Aggregate(text, (current, rule) => RedactWholeWord(current, rule.Pattern));

        return (filteredText, false);
    }

    private static bool MatchesWholeWord(string text, string pattern) =>
        Regex.IsMatch(text, BuildWholeWordPattern(pattern), RegexOptions.IgnoreCase);

    private static string RedactWholeWord(string text, string pattern) =>
        Regex.Replace(
            text,
            BuildWholeWordPattern(pattern),
            match => new string('*', match.Value.Length),
            RegexOptions.IgnoreCase);

    private static string BuildWholeWordPattern(string pattern) =>
        $@"\b{Regex.Escape(pattern)}\b";

    private static string NormalizePattern(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            throw new ValidationBusinessException(nameof(pattern), "Pattern must not be empty.");
        }

        return pattern.Trim();
    }

    private static string NormalizeAction(string action)
    {
        var normalized = string.IsNullOrWhiteSpace(action) ? string.Empty : action.Trim();

        if (string.Equals(normalized, ActionRedact, StringComparison.OrdinalIgnoreCase))
        {
            return ActionRedact;
        }

        if (string.Equals(normalized, ActionBlock, StringComparison.OrdinalIgnoreCase))
        {
            return ActionBlock;
        }

        throw new ValidationBusinessException(nameof(action), $"Action must be '{ActionRedact}' or '{ActionBlock}'.");
    }
}
