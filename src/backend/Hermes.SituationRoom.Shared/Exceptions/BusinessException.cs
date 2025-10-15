namespace Hermes.SituationRoom.Shared.Exceptions;

/// <summary>
/// Base class for all custom business exceptions in the application.
/// </summary>
public abstract class BusinessException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessException"/> class.
    /// </summary>
    protected BusinessException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    protected BusinessException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference
    /// (Nothing in Visual Basic) if no inner exception is specified.</param>
    protected BusinessException(string message, Exception innerException) : base(message, innerException) { }
}