namespace Hermes.SituationRoom.Shared.Exceptions;

/// <summary>
/// Exception thrown when attempting to create a resource that already exists
/// </summary>
public class DuplicateResourceException(string resourceType, string conflictingField, string conflictingValue)
    : Exception($"{resourceType} with {conflictingField} '{conflictingValue}' already exists.")
{
    public string ResourceType { get; } = resourceType;

    public string ConflictingField { get; } = conflictingField;

    public string ConflictingValue { get; } = conflictingValue;
}
