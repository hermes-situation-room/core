namespace Hermes.SituationRoom.Shared.Exceptions;

/// <summary>
/// Exception thrown when a requested resource is not found
/// </summary>
public class ResourceNotFoundException(string resourceType, Guid resourceId)
    : Exception($"{resourceType} with identifier '{resourceId}' was not found.")
{
    public string ResourceType { get; } = resourceType;

    public string ResourceId { get; } = resourceId.ToString();
}
