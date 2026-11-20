namespace AutoRent.Application.Common;

/// <summary>Category of an expected business failure; the API maps each kind to an HTTP status.</summary>
public enum ErrorKind
{
    Validation,
    Unauthorized,
    Forbidden,
    NotFound,
    Conflict,
    BusinessRule,
}

/// <summary>
/// An expected failure with a stable, machine-readable <see cref="Code"/> (e.g. "auth.invalid_credentials").
/// The message is safe to show to the client.
/// </summary>
public sealed class AppException(ErrorKind kind, string code, string message) : Exception(message)
{
    public ErrorKind Kind { get; } = kind;
    public string Code { get; } = code;

    public static AppException Validation(string code, string message) => new(ErrorKind.Validation, code, message);
    public static AppException Unauthorized(string code, string message) => new(ErrorKind.Unauthorized, code, message);
    public static AppException Forbidden(string code, string message) => new(ErrorKind.Forbidden, code, message);
    public static AppException NotFound(string code, string message) => new(ErrorKind.NotFound, code, message);
    public static AppException Conflict(string code, string message) => new(ErrorKind.Conflict, code, message);
    public static AppException BusinessRule(string code, string message) => new(ErrorKind.BusinessRule, code, message);
}
