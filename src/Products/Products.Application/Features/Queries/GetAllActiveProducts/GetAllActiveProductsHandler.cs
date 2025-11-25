using MediatR;
using Products.Application.DTOs;
using Products.Domain.Interfaces;

namespace Products.Application.Features.Queries.GetAllActiveProducts
{
    public class GetAllActiveProductsHandler : IRequestHandler<GetAllActiveProductsQuery, IEnumerable<ProductDto>>
    {
        private readonly IProductRepository _productrepository;
        public GetAllActiveProductsHandler(IProductRepository productrepository) => _productrepository = productrepository;

        public async Task<IEnumerable<ProductDto>> Handle(GetAllActiveProductsQuery request, CancellationToken cancellationToken)
        {
            var products = await _productrepository.GetAllActiveProductsAsync(cancellationToken);
            return products.Select(p => new ProductDto(p.Id, p.Name, p.Description, p.Price, p.IsAvailable, p.OwnerId, p.CreatedAt));
        }
    }
}
