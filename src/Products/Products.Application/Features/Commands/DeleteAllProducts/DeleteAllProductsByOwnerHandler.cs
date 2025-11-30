using MediatR;
using Products.Domain.Interfaces;
using Products.Application.Features.Commands.DeleteAllProducts;

namespace Products.Application.Features.Commands.DeleteAllProducts
{
    public class DeleteAllProductsByOwnerHandler : IRequestHandler<DeleteAllProductsByOwnerCommand, bool>
    {
        private readonly IProductRepository _repository;

        public DeleteAllProductsByOwnerHandler(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(DeleteAllProductsByOwnerCommand request, CancellationToken cancellationToken)
        {
            var products = await _repository.GetByOwnerIdAsync(request.OwnerId);

            if (products == null || !products.Any())
            {
                return true;
            }

            _repository.DeleteRange(products);
            await _repository.SaveChangesAsync();

            return true;
        }
    }
}