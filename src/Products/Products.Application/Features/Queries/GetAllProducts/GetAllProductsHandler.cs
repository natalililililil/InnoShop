using MediatR;
using Products.Application.DTOs;
using Products.Domain.Interfaces;

namespace Products.Application.Features.Queries.GetAllProducts
{
    public class GetAllProductsHandler : IRequestHandler<GetAllProductsQuery, IEnumerable<ProductDto>>
    {
        private readonly IProductRepository _productrepository;
        public GetAllProductsHandler(IProductRepository productrepository) => _productrepository = productrepository;

        public async Task<IEnumerable<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            var products = await _productrepository.GetAllAsync(cancellationToken);
            return products.Select(p => new ProductDto(p.Id, p.Name, p.Description, p.Price, p.IsAvailable, p.OwnerId, p.CreatedAt));
        }
    }
}
