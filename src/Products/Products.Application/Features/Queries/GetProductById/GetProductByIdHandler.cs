using MediatR;
using Products.Application.DTOs;
using Products.Domain.Interfaces;

namespace Products.Application.Features.Queries.GetProductById
{
    public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
    {
        private readonly IProductRepository _repository;

        public GetProductByIdHandler(IProductRepository repository) => _repository = repository;

        public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var p = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (p == null || p.IsDeleted) 
                return null;
            return new ProductDto(p.Id, p.Name, p.Description, p.Price, p.IsAvailable, p.OwnerId, p.CreatedAt);
        }
    }
}
