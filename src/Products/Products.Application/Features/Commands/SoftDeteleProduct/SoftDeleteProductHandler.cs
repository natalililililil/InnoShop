using MediatR;
using Products.Domain.Interfaces;

namespace Products.Application.Features.Commands.SoftDeteleProduct
{
    public class SoftDeleteProductHandler : IRequestHandler<SoftDeleteProductCommand, bool>
    {
        public readonly IProductRepository _productRepository;
        public SoftDeleteProductHandler(IProductRepository productRepository) => _productRepository = productRepository;

        public async Task<bool> Handle(SoftDeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
            if (product == null) return false;
            if (product.OwnerId != request.OwnerId)
                throw new UnauthorizedAccessException("Вы можете удалять только свои продукты");
            product.SoftDelete();
            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
