namespace Hermes.SituationRoom.Api.Controllers;

using Base;
using Configurations;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DataTransferObjects;
using Swashbuckle.AspNetCore.Annotations;

[Authorize(Roles = "Admin")]
public class ContentFilterController(IControllerInfrastructure infra, IContentFilterService contentFilterService)
    : SituationRoomControllerBase(infra)
{
    [HttpGet("internal/content-filter")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_CONTENT_FILTER])]
    [ProducesResponseType(typeof(IReadOnlyList<ContentFilterRuleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ContentFilterRuleDto>>> GetAllRules()
    {
        var rules = await contentFilterService.GetAllRulesAsync();
        return Ok(rules);
    }

    [HttpPost("internal/content-filter")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_CONTENT_FILTER])]
    [ProducesResponseType(typeof(ContentFilterRuleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ContentFilterRuleDto>> CreateRule([FromBody] CreateContentFilterRuleDto createContentFilterRuleDto)
    {
        var rule = await contentFilterService.CreateRuleAsync(createContentFilterRuleDto.Pattern, createContentFilterRuleDto.Action);
        return CreatedAtAction(nameof(GetAllRules), new { }, rule);
    }

    [HttpPut("internal/content-filter/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_CONTENT_FILTER])]
    [ProducesResponseType(typeof(ContentFilterRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContentFilterRuleDto>> UpdateRule(Guid uid, [FromBody] UpdateContentFilterRuleDto updateContentFilterRuleDto)
    {
        try
        {
            var rule = await contentFilterService.UpdateRuleAsync(
                uid,
                updateContentFilterRuleDto.Pattern,
                updateContentFilterRuleDto.Action,
                updateContentFilterRuleDto.Enabled);
            return Ok(rule);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("internal/content-filter/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_CONTENT_FILTER])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteRule(Guid uid)
    {
        await contentFilterService.DeleteRuleAsync(uid);
        return NoContent();
    }
}
