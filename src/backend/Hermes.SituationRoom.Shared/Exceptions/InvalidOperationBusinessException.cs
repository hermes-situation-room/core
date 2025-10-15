namespace Hermes.SituationRoom.Shared.Exceptions;

/// <summary>
/// Exception thrown when an operation is attempted that is invalid given the current state of the system or business rules.
/// Corresponds to HTTP 400 Bad Request.
/// </summary>
public class InvalidOperationBusinessException : BusinessException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidOperationBusinessException"/> class.
    /// </summary>
    public InvalidOperationBusinessException() : base("The requested operation is invalid.") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidOperationBusinessException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public InvalidOperationBusinessException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidOperationBusinessException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public InvalidOperationBusinessException(string message, Exception innerException) : base(message, innerException) { }
}