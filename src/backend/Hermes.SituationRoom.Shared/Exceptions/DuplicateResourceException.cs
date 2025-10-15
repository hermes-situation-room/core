namespace Hermes.SituationRoom.Shared.Exceptions;

/// <summary>
/// Exception thrown when an attempt is made to create a resource that already exists,
/// or to update a resource with a value that conflicts with an existing resource.
/// Corresponds to HTTP 409 Conflict.
/// </summary>
public class DuplicateResourceException : BusinessException
{
    /// <summary>
    /// Gets the type of the resource that caused the conflict.
    /// </summary>
    public string ResourceType { get; }

    /// <summary>
    /// Gets the identifier or conflicting value of the resource.
    /// </summary>
    public string ResourceIdentifier { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateResourceException"/> class.
    /// </summary>
    public DuplicateResourceException() : base("A resource with the specified identifier already exists.")
    {
        ResourceType = "Resource";
        ResourceIdentifier = "Unknown";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateResourceException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public DuplicateResourceException(string message) : base(message)
    {
        ResourceType = "Resource";
        ResourceIdentifier = "Unknown";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateResourceException"/> class with the resource type and identifier.
    /// </summary>
    /// <param name="resourceType">The type of the resource (e.g., "User", "Post").</param>
    /// <param name="resourceIdentifier">The identifier or conflicting value (e.g., "john.doe@example.com", "my-unique-username").</param>
    public DuplicateResourceException(string resourceType, string resourceIdentifier)
        : base($"A {resourceType} with identifier '{resourceIdentifier}' already exists.")
    {
        ResourceType = resourceType;
        ResourceIdentifier = resourceIdentifier;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateResourceException"/> class with the resource type, identifier, and a custom message.
    /// </summary>
    /// <param name="resourceType">The type of the resource (e.g., "User", "Post").</param>
    /// <param name="resourceIdentifier">The identifier or conflicting value (e.g., "john.doe@example.com", "my-unique-username").</param>
    /// <param name="message">The message that describes the error.</param>
    public DuplicateResourceException(string resourceType, string resourceIdentifier, string message)
        : base(message)
    {
        ResourceType = resourceType;
        ResourceIdentifier = resourceIdentifier;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateResourceException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public DuplicateResourceException(string message, Exception innerException) : base(message, innerException)
    {
        ResourceType = "Resource";
        ResourceIdentifier = "Unknown";
    }
}