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
        void SoftDelete(Product product);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Product>> SearchAsync(string query, CancellationToken cancellationToken = default);
        Task<IEnumerable<Product>> FilterAsync(decimal? minPrice, decimal? maxPrice, bool? isAvailable, CancellationToken cancellationToken = default);

    }
}
