using MediatR;
using Products.Application.DTOs;

namespace Products.Application.Features.Queries.GetAllProducts
{
    public class GetAllProductsQuery : IRequest<IEnumerable<ProductDto>> { }

}
