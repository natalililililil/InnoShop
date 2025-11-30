using MediatR;
using Products.Application.DTOs;

namespace Products.Application.Features.Queries.GetAllActiveProducts
{
    public class GetAllActiveProductsQuery : IRequest<IEnumerable<ProductDto>> { }

}
