using MediatR;
using Products.Domain.Entities;
using Products.Domain.Interfaces;

namespace Products.Application.Features.Commands.SoftDeteleProducts
{
    public class SoftDeleteAllProductsByOwnerHandler : IRequestHandler<SoftDeleteAllProductsByOwnerCommand, bool>
    {
        public readonly IProductRepository _productRepository;
        public SoftDeleteAllProductsByOwnerHandler(IProductRepository productRepository) => _productRepository = productRepository;

        public async Task<bool> Handle(SoftDeleteAllProductsByOwnerCommand request, CancellationToken cancellationToken)
        {
            var products = await _productRepository.GetByOwnerIdAsync(request.OwnerId, cancellationToken);
            if (products == null || !products.Any())
                return false;
            
            foreach (var product in products)
            {
                product.SoftDelete(); 
                _productRepository.Update(product);
            }

            await _productRepository.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
