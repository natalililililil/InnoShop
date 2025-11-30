using MediatR;
using Products.Application.DTOs;

namespace Products.Application.Features.Commands.UpdateProduct
{
    public record UpdateProductCommand(Guid Id, Guid OwnerId, UpdateProductDto Product) : IRequest<bool>;

}
