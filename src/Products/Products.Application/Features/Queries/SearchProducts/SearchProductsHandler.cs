using MediatR;
using Products.Application.DTOs;
using Products.Application.Features.Queries.SearchProducts;
using Products.Domain.Interfaces;

public class SearchProductsHandler : IRequestHandler<SearchProductsQuery, IEnumerable<ProductDto>>
{
    private readonly IProductRepository _productrepository;

    public SearchProductsHandler(IProductRepository repo)
    {
        _productrepository = repo;
    }

    public async Task<IEnumerable<ProductDto>> Handle(SearchProductsQuery request, CancellationToken token)
    {
        var products = await _productrepository.SearchAsync(request.Query, token);

        return products.Select(p => new ProductDto(p.Id, p.Name, p.Description, p.Price, p.IsAvailable, p.OwnerId, p.CreatedAt));
    }
}
