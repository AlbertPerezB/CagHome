using CagHome.IngestionService.Domain.Enums;

namespace CagHome.IngestionService.Domain.Models;

public class ValidationError
{
    public string Message { get; }

    public ValidationCode Code { get; }

    public ValidationError(string message, ValidationCode code)
    {
        Message = message;
        Code = code;
    }
}
