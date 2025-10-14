namespace Hermes.SituationRoom.Shared.Exceptions;

/// <summary>
/// Exception thrown when authentication credentials are invalid
/// </summary>
public class InvalidCredentialsException()
    : Exception("Invalid username or password.");
