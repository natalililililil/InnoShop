using Microsoft.EntityFrameworkCore;
using Products.Domain.Entities;
using Products.Domain.Interfaces;
using Products.Infrastructure.Persistence;

namespace Products.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ProductsDbContext _dbContext;
        public ProductRepository(ProductsDbContext dbContext) => _dbContext = dbContext;

        public async Task<IEnumerable<Product?>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Products.AsNoTracking().ToListAsync(cancellationToken);
        }
        public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        public async Task CreateAsync(Product product, CancellationToken cancellationToken = default)
            => await _dbContext.Products.AddAsync(product, cancellationToken);

        public void Update(Product product) => _dbContext.Products.Update(product);

        public void Delete(Product product) => _dbContext.Products.Remove(product);
        public void SoftDelete(Product product) => _dbContext.Products.Remove(product);

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => await _dbContext.SaveChangesAsync(cancellationToken);
        public async Task<IEnumerable<Product>> SearchAsync(string query, CancellationToken cancellationToken)
        {
            query = query.Trim();

            return await _dbContext.Products
                .Where(p => p.Name.Contains(query) || p.Description.Contains(query))
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Product>> FilterAsync(decimal? minPrice, decimal? maxPrice, bool? isAvailable, CancellationToken cancellationToken)
        {
            var q = _dbContext.Products.AsQueryable();

            if (minPrice.HasValue)
                q = q.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                q = q.Where(p => p.Price <= maxPrice.Value);

            if (isAvailable.HasValue)
                q = q.Where(p => p.IsAvailable == isAvailable.Value);

            return await q.ToListAsync(cancellationToken);
        }

    }
}
