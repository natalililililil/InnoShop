using MediatR;
using Products.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Products.Application.Features.Commands.SoftRestoreProducts
{
    public class SoftRestoreAllProductsByOwnerHandler : IRequestHandler<SoftRestoreAllProductsByOwnerCommand, bool>
    {
        private readonly IProductRepository _productRepository;

        public SoftRestoreAllProductsByOwnerHandler(IProductRepository productRepository) => _productRepository = productRepository;

        public async Task<bool> Handle(SoftRestoreAllProductsByOwnerCommand request, CancellationToken cancellationToken)
        {
            var products = await _productRepository.GetByOwnerIdAsync(request.OwnerId, cancellationToken);

            if (products == null || !products.Any())
                return false;

            foreach (var product in products)
            {
                product.Restore();
                _productRepository.Update(product);
            }

            await _productRepository.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
