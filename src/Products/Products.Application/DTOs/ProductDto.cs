namespace Products.Application.DTOs
{
    public record ProductDto(Guid Id, string Name, string Description, decimal Price, bool IsAvailable, Guid OwnerId, DateTime CreatedAt);
}
