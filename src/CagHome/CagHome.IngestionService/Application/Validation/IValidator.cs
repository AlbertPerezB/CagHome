namespace CagHome.IngestionService.Domain.Models
{
    public interface IValidator<T>
    {
        Task<ValidationOutcome> ValidateAsync(T item, CancellationToken ct);
    }
}
