using MediatR;
using Products.Application.DTOs;

namespace Products.Application.Features.Queries.FilterProducts
{
    public record FilterProductsQuery(decimal? MinPrice, decimal? MaxPrice, bool? IsAvailable) : IRequest<IEnumerable<ProductDto>>;
}
