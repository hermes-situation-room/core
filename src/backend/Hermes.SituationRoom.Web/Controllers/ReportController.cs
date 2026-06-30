namespace Hermes.SituationRoom.Api.Controllers;

using Base;
using Configurations;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.BusinessObjects;
using Shared.DataTransferObjects;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

[Authorize]
public class ReportController(IControllerInfrastructure infra, IReportService reportService)
    : SituationRoomControllerBase(infra)
{
    private Guid GetUserIdFromContext()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found or invalid.");
        }
        return userId;
    }

    [HttpGet("internal/report")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_REPORT])]
    [ProducesResponseType(typeof(IEnumerable<ReportBo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ReportBo>>> GetAllReports()
    {
        var reports = await reportService.GetAllAsync();
        return Ok(reports);
    }

    [HttpGet("internal/report/{uid:guid}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_REPORT])]
    [ProducesResponseType(typeof(ReportBo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReportBo>> GetReportById(Guid uid)
    {
        var report = await reportService.GetByIdAsync(uid);
        if (report == null)
        {
            return NotFound();
        }
        return Ok(report);
    }

    [HttpGet("internal/report/status/{status}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_REPORT])]
    [ProducesResponseType(typeof(IEnumerable<ReportBo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ReportBo>>> GetReportsByStatus(string status)
    {
        var reports = await reportService.GetByStatusAsync(status);
        return Ok(reports);
    }

    [HttpGet("internal/report/target/{targetType}/{targetUid:guid}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_REPORT])]
    [ProducesResponseType(typeof(IEnumerable<ReportBo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ReportBo>>> GetReportsByTarget(string targetType, Guid targetUid)
    {
        var reports = await reportService.GetByTargetAsync(targetType, targetUid);
        return Ok(reports);
    }

    [HttpGet("internal/report/my-reports")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_REPORT])]
    [ProducesResponseType(typeof(IEnumerable<ReportBo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ReportBo>>> GetMyReports()
    {
        var userId = GetUserIdFromContext();
        var reports = await reportService.GetByReporterUidAsync(userId);
        return Ok(reports);
    }

    [HttpPost("internal/report")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_REPORT])]
    [ProducesResponseType(typeof(ReportBo), StatusCodes.Status201Created)]
    public async Task<ActionResult<ReportBo>> CreateReport([FromBody] CreateReportDto createReportDto)
    {
        var userId = GetUserIdFromContext();
        var report = await reportService.CreateReportAsync(
            userId,
            createReportDto.TargetType,
            createReportDto.TargetUid,
            createReportDto.Reason
        );
        return CreatedAtAction(nameof(GetReportById), new { uid = report.Uid }, report);
    }

    [HttpPut("internal/report/{uid:guid}/review")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_REPORT])]
    [ProducesResponseType(typeof(ReportBo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReportBo>> ReviewReport(Guid uid, [FromBody] ReviewReportDto reviewReportDto)
    {
        try
        {
            var report = await reportService.ReviewReportAsync(
                uid,
                reviewReportDto.ReviewedByUid,
                reviewReportDto.Status,
                reviewReportDto.AdminNotes
            );
            return Ok(report);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("internal/report/{uid:guid}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_REPORT])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReport(Guid uid)
    {
        await reportService.DeleteAsync(uid);
        return NoContent();
    }
}

