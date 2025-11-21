using Products.Domain.Entities;

namespace Products.Domain.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product?>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task CreateAsync(Product product, CancellationToken cancellationToken = default);
        void Update(Product product);
        void Delete(Product product);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
