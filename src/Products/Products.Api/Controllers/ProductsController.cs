using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Products.Application.DTOs;
using Products.Application.Features.Commands.CreateProduct;
using Products.Application.Features.Commands.DeleteProduct;
using Products.Application.Features.Commands.SoftDeteleProduct;
using Products.Application.Features.Commands.UpdateProduct;
using Products.Application.Features.Queries.GetAllProducts;
using Products.Application.Features.Queries.GetProductById;

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
            var products = await _mediator.Send(new GetAllProductsQuery());
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
        [HttpDelete("{id:guid}/softdelete")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            var ownerId = GetUserIdFromClaims();
            var success = await _mediator.Send(new SoftDeleteProductCommand(id, ownerId));
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
    }
}
