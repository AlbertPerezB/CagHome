using CagHome.IngestionService.Domain.Enums;

namespace CagHome.IngestionService.Application.Validation;

public class ValidationResult
{
    public bool IsValid { get; }
    public string? ErrorMessage { get; }
    public ValidationCode? ErrorCode { get; }

    private ValidationResult(bool isValid, string? errorMessage, ValidationCode? errorCode)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
    }

    public static ValidationResult Success() => new(true, null, null);

    public static ValidationResult Failure(string errorMessage, ValidationCode errorCode) =>
        new(false, errorMessage, errorCode);
}
