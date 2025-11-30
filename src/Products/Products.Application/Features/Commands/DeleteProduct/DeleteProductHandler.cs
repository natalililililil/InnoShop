using MediatR;
using Products.Domain.Interfaces;

namespace Products.Application.Features.Commands.DeleteProduct
{
    public class DeleteProductHandler : IRequestHandler<DeleteProductCommand, bool>
    {
        private readonly IProductRepository _repository;

        public DeleteProductHandler(IProductRepository repository) => _repository = repository;

        public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (product == null) return false;
            if (product.OwnerId != request.OwnerId)
                throw new UnauthorizedAccessException("Вы можете удалять только свои продукты");

            _repository.Delete(product);
            await _repository.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
