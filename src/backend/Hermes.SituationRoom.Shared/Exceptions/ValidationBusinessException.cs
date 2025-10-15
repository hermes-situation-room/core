using System.Collections.Generic;
using System.Linq;

namespace Hermes.SituationRoom.Shared.Exceptions;

/// <summary>
/// Exception thrown when one or more validation errors occur in the business logic.
/// Corresponds to HTTP 400 Bad Request.
/// </summary>
public class ValidationBusinessException : BusinessException
{
    /// <summary>
    /// Gets a dictionary of validation errors, where the key is the field name and the value is an array of error messages.
    /// </summary>
    public IDictionary<string, string[]> ValidationErrors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationBusinessException"/> class.
    /// </summary>
    public ValidationBusinessException() : base("One or more validation errors occurred.")
    {
        ValidationErrors = new Dictionary<string, string[]>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationBusinessException"/> class with a single validation error.
    /// </summary>
    /// <param name="field">The name of the field that failed validation.</param>
    /// <param name="message">The validation error message for the field.</param>
    public ValidationBusinessException(string field, string message)
        : base($"Validation failed for field '{field}': {message}")
    {
        ValidationErrors = new Dictionary<string, string[]> { { field, new[] { message } } };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationBusinessException"/> class with multiple validation errors.
    /// </summary>
    /// <param name="validationErrors">A dictionary where keys are field names and values are arrays of error messages.</param>
    public ValidationBusinessException(IDictionary<string, string[]> validationErrors)
        : base("One or more validation errors occurred.")
    {
        ValidationErrors = validationErrors;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationBusinessException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ValidationBusinessException(string message, Exception innerException) : base(message, innerException)
    {
        ValidationErrors = new Dictionary<string, string[]>();
    }

    /// <summary>
    /// Overrides the Message property to provide a more detailed error message
    /// if multiple validation errors are present.
    /// </summary>
    public override string Message
    {
        get
        {
            if (ValidationErrors.Any())
            {
                var errors = string.Join("; ", ValidationErrors.SelectMany(kv => kv.Value.Select(v => $"{kv.Key}: {v}")));
                return $"One or more validation errors occurred: {errors}";
            }
            return base.Message;
        }
    }
}