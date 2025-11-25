using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Products.Api.Filters;
using Products.Application.DTOs;
using Products.Application.Features.Commands.CreateProduct;
using Products.Application.Features.Commands.DeleteProduct;
using Products.Application.Features.Commands.SoftDeteleProducts;
using Products.Application.Features.Commands.SoftRestoreProducts;
using Products.Application.Features.Commands.UpdateProduct;
using Products.Application.Features.Queries.FilterProducts;
using Products.Application.Features.Queries.GetAllActiveProducts;
using Products.Application.Features.Queries.GetProductById;
using Products.Application.Features.Queries.SearchProducts;

namespace Products.Api.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : Controller
    {
        private readonly IMediator _mediator;
        public ProductsController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _mediator.Send(new GetAllActiveProductsQuery());
            if (products == null) 
                return NotFound();
            return Ok(products);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var product = await _mediator.Send(new GetProductByIdQuery(id));
            if (product == null) 
                return NotFound();  
            return Ok(product);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
            var ownerId = GetUserIdFromClaims();
            var id = await _mediator.Send(new CreateProductCommand(ownerId, dto));
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [Authorize]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDto dto)
        {
            var ownerId = GetUserIdFromClaims();
            var success = await _mediator.Send(new UpdateProductCommand(id, ownerId, dto));
            return success ? NoContent() : NotFound();
        }

        [Authorize]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)    
        {
            var ownerId = GetUserIdFromClaims();
            var success = await _mediator.Send(new DeleteProductCommand(id, ownerId));
            return success ? NoContent() : NotFound();
        }

        private Guid GetUserIdFromClaims()
        {
            var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                      ?? User.FindFirst("sub")?.Value;
            if (sub == null) 
                throw new UnauthorizedAccessException("Пользователь отсутвствует");
            return Guid.Parse(sub);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [ServiceFilter(typeof(ApiKeyAuthFilter))]
        [HttpPatch("owner/{ownerId:guid}/soft-delete")]
        public async Task<IActionResult> SoftDelete(Guid ownerId)
        {
            var success = await _mediator.Send(new SoftDeleteAllProductsByOwnerCommand(ownerId));
            return success ? NoContent() : NotFound();
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [ServiceFilter(typeof(ApiKeyAuthFilter))]
        [HttpPatch("owner/{ownerId:guid}/soft-restore")]
        public async Task<IActionResult> SoftRestore(Guid ownerId)
        {
            var success = await _mediator.Send(new SoftRestoreAllProductsByOwnerCommand(ownerId));

            return success ? NoContent() : NotFound();
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            var result = await _mediator.Send(new SearchProductsQuery(query));
            return Ok(result);
        }

        [HttpGet("filter")]
        public async Task<IActionResult> Filter([FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice, [FromQuery] bool? isAvailable)
        {
            var result = await _mediator.Send(
                new FilterProductsQuery(minPrice, maxPrice, isAvailable));

            return Ok(result);
        }
    }
}
