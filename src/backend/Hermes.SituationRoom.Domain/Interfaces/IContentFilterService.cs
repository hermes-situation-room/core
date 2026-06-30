namespace Hermes.SituationRoom.Domain.Interfaces;

using Shared.DataTransferObjects;

public interface IContentFilterService
{
    Task<ContentFilterRuleDto> CreateRuleAsync(string pattern, string action);

    Task<IReadOnlyList<ContentFilterRuleDto>> GetAllRulesAsync();

    Task<ContentFilterRuleDto> UpdateRuleAsync(Guid uid, string pattern, string action, bool enabled);

    Task DeleteRuleAsync(Guid uid);

    /// <summary>
    /// Applies all enabled content filter rules to the supplied text.
    /// Redact rules replace whole-word, case-insensitive matches with asterisks.
    /// Block rules cause the returned <c>blocked</c> flag to be set to true.
    /// Intended for public posts and comments only — never private messages.
    /// </summary>
    Task<(string FilteredText, bool Blocked)> ApplyAsync(string text);
}
