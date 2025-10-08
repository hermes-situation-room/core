namespace Hermes.SituationRoom.Api.Middlewares;

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

public class ExceptionMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<ExceptionMiddleware> _logger;

	public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
	{
		_next = next;
		_logger = logger;
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

	private static (int StatusCode, string Title) MapExceptionToResponse(Exception exception)
	{
		return exception switch
		{
			KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
			UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
			ValidationException => (StatusCodes.Status400BadRequest, "Validation failed"),
			ArgumentException => (StatusCodes.Status400BadRequest, "Invalid argument"),
			NotSupportedException => (StatusCodes.Status405MethodNotAllowed, "Operation not allowed"),
			_ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred: " + exception.InnerException?.Message)
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

	private static ProblemDetails BuildProblemDetails(HttpContext httpContext, Exception exception)
	{
		var (statusCode, title) = MapExceptionToResponse(exception);

		var problem = new ProblemDetails
		{
			Status = statusCode,
			Title = title,
			Detail = exception.Message,
			Instance = httpContext.Request.Path
		};

		problem.Extensions["traceId"] = httpContext.TraceIdentifier;
		var errors = ExtractValidationErrors(exception);
		if (errors.Count > 0)
		{
			problem.Extensions["errors"] = errors;
		}

		return problem;
	}

	private static async Task WriteProblemDetailsAsync(HttpContext httpContext, Exception exception)
	{
		var problemDetails = BuildProblemDetails(httpContext, exception);
		httpContext.Response.ContentType = "application/problem+json";
		httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

		await httpContext.Response.WriteAsJsonAsync(problemDetails);
	}
}