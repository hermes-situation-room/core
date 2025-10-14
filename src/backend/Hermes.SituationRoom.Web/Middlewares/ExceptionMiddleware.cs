namespace Hermes.SituationRoom.Api.Middlewares;

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Shared.Exceptions;

public class ExceptionMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<ExceptionMiddleware> _logger;
	private readonly IWebHostEnvironment _env;

	public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IWebHostEnvironment env)
	{
		_next = next;
		_logger = logger;
		_env = env;
	}

	public async Task InvokeAsync(HttpContext httpContext)
	{
		try
		{
			await _next(httpContext);
		}
		catch (Exception exception)
		{
			if (httpContext.Response.HasStarted)
			{
				_logger.LogError(exception, "Unhandled exception after response started. TraceId: {TraceId}", httpContext.TraceIdentifier);
				throw;
			}

			_logger.LogError(exception, "Unhandled exception occurred. TraceId: {TraceId}", httpContext.TraceIdentifier);
			await WriteProblemDetailsAsync(httpContext, exception);
		}
	}

	private (int StatusCode, string Title, string Type) MapExceptionToResponse(Exception exception)
	{
		return exception switch
		{
			InvalidCredentialsException => (
				StatusCodes.Status401Unauthorized, 
				"Authentication Failed", 
				"https://tools.ietf.org/html/rfc7235#section-3.1"
			),
			UnauthorizedAccessException => (
				StatusCodes.Status401Unauthorized, 
				"Unauthorized Access", 
				"https://tools.ietf.org/html/rfc7235#section-3.1"
			),
			ResourceNotFoundException => (
				StatusCodes.Status404NotFound, 
				"Resource Not Found", 
				"https://tools.ietf.org/html/rfc7231#section-6.5.4"
			),
			KeyNotFoundException => (
				StatusCodes.Status404NotFound, 
				"Resource Not Found", 
				"https://tools.ietf.org/html/rfc7231#section-6.5.4"
			),
			DuplicateResourceException => (
				StatusCodes.Status409Conflict, 
				"Resource Already Exists", 
				"https://tools.ietf.org/html/rfc7231#section-6.5.8"
			),
			ValidationException => (
				StatusCodes.Status400BadRequest, 
				"Validation Failed", 
				"https://tools.ietf.org/html/rfc7231#section-6.5.1"
			),
			ArgumentException or ArgumentNullException => (
				StatusCodes.Status400BadRequest, 
				"Invalid Request", 
				"https://tools.ietf.org/html/rfc7231#section-6.5.1"
			),
			NotSupportedException => (
				StatusCodes.Status405MethodNotAllowed, 
				"Operation Not Allowed", 
				"https://tools.ietf.org/html/rfc7231#section-6.5.5"
			),
			InvalidOperationException => (
				StatusCodes.Status400BadRequest, 
				"Invalid Operation", 
				"https://tools.ietf.org/html/rfc7231#section-6.5.1"
			),
			_ => (
				StatusCodes.Status500InternalServerError, 
				"Internal Server Error", 
				"https://tools.ietf.org/html/rfc7231#section-6.6.1"
			)
		};
	}

	private static IDictionary<string, string[]> ExtractValidationErrors(Exception exception)
	{
		if (exception is ValidationException validationException)
		{
			var memberName = validationException.ValidationResult?.MemberNames?.FirstOrDefault() ?? "Model";
			return new Dictionary<string, string[]>
			{
				{ memberName, new[] { validationException.ValidationResult?.ErrorMessage ?? validationException.Message } }
			};
		}

		return new Dictionary<string, string[]>();
	}

	private ProblemDetails BuildProblemDetails(HttpContext httpContext, Exception exception)
	{
		var (statusCode, title, type) = MapExceptionToResponse(exception);

		var problem = new ProblemDetails
		{
			Status = statusCode,
			Title = title,
			Type = type,
			Detail = GetDetailMessage(exception, statusCode),
			Instance = httpContext.Request.Path
		};

		problem.Extensions["traceId"] = httpContext.TraceIdentifier;
		problem.Extensions["timestamp"] = DateTime.UtcNow;

		var errors = ExtractValidationErrors(exception);
		if (errors.Count > 0)
		{
			problem.Extensions["errors"] = errors;
		}

		switch (exception)
		{
			case ResourceNotFoundException resourceNotFound:
				problem.Extensions["resourceType"] = resourceNotFound.ResourceType;
				problem.Extensions["resourceId"] = resourceNotFound.ResourceId;
				break;
			case DuplicateResourceException duplicateResource:
				problem.Extensions["resourceType"] = duplicateResource.ResourceType;
				problem.Extensions["conflictingField"] = duplicateResource.ConflictingField;
				break;
		}

		return problem;
	}

	private string GetDetailMessage(Exception exception, int statusCode)
	{
		if (statusCode == StatusCodes.Status500InternalServerError && !_env.IsDevelopment())
		{
			return "An unexpected error occurred. Please try again later or contact support if the problem persists.";
		}

		return exception.Message;
	}

	private async Task WriteProblemDetailsAsync(HttpContext httpContext, Exception exception)
	{
		var problemDetails = BuildProblemDetails(httpContext, exception);
		httpContext.Response.ContentType = "application/problem+json";
		httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

		await httpContext.Response.WriteAsJsonAsync(problemDetails);
	}
}