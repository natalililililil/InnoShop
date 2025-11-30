using MediatR;
using Products.Domain.Interfaces;

namespace Products.Application.Features.Commands.UpdateProduct
{
    public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, bool>
    {
        private readonly IProductRepository _repository;

        public UpdateProductHandler(IProductRepository repository) => _repository = repository;

        public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (product == null) return false;

            if (product.OwnerId != request.OwnerId)
                throw new UnauthorizedAccessException("Вы можете изменять только свои продукты");

            product.Update(request.Product.Name, request.Product.Description, request.Product.Price);
            product.SetAvailability(request.Product.IsAvailable);

            _repository.Update(product);
            await _repository.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
