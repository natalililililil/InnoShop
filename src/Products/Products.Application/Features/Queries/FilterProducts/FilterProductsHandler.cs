using MediatR;
using Products.Application.DTOs;
using Products.Application.Features.Queries.FilterProducts;
using Products.Domain.Interfaces;

public class FilterProductsHandler : IRequestHandler<FilterProductsQuery, IEnumerable<ProductDto>>
{
    private readonly IProductRepository _productrepository;

    public FilterProductsHandler(IProductRepository repo)
    {
        _productrepository = repo;
    }

    public async Task<IEnumerable<ProductDto>> Handle(FilterProductsQuery request, CancellationToken token)
    {
        var products = await _productrepository.FilterAsync(
            request.MinPrice,
            request.MaxPrice,
            request.IsAvailable,
            token
        );

        return products.Select(p => new ProductDto(p.Id, p.Name, p.Description, p.Price, p.IsAvailable, p.OwnerId, p.CreatedAt));
    }
}
