namespace Hermes.SituationRoom.Shared.Exceptions;

/// <summary>
/// Exception thrown when a requested resource cannot be found.
/// Corresponds to HTTP 404 Not Found.
/// </summary>
public class ResourceNotFoundException : BusinessException
{
    /// <summary>
    /// Gets the type of the resource that was not found.
    /// </summary>
    public string ResourceType { get; }

    /// <summary>
    /// Gets the identifier of the resource that was not found.
    /// </summary>
    public string ResourceIdentifier { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceNotFoundException"/> class.
    /// </summary>
    public ResourceNotFoundException() : base("The requested resource was not found.")
    {
        ResourceType = "Resource";
        ResourceIdentifier = "Unknown";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceNotFoundException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ResourceNotFoundException(string message) : base(message)
    {
        ResourceType = "Resource";
        ResourceIdentifier = "Unknown";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceNotFoundException"/> class with the resource type and identifier.
    /// </summary>
    /// <param name="resourceType">The type of the resource (e.g., "User", "Post").</param>
    /// <param name="resourceIdentifier">The identifier of the resource (e.g., "123e4567-e89b-12d3-a456-426614174000").</param>
    public ResourceNotFoundException(string resourceType, string resourceIdentifier)
        : base($"The {resourceType} with identifier '{resourceIdentifier}' was not found.")
    {
        ResourceType = resourceType;
        ResourceIdentifier = resourceIdentifier;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceNotFoundException"/> class with the resource type, identifier, and a custom message.
    /// </summary>
    /// <param name="resourceType">The type of the resource (e.g., "User", "Post").</param>
    /// <param name="resourceIdentifier">The identifier of the resource (e.g., "123e4567-e89b-12d3-a456-426614174000").</param>
    /// <param name="message">The message that describes the error.</param>
    public ResourceNotFoundException(string resourceType, string resourceIdentifier, string message)
        : base(message)
    {
        ResourceType = resourceType;
        ResourceIdentifier = resourceIdentifier;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceNotFoundException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ResourceNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
        ResourceType = "Resource";
        ResourceIdentifier = "Unknown";
    }
}