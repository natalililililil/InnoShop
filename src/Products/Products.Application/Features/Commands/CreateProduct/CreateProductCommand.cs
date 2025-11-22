using MediatR;
using Products.Application.DTOs;

namespace Products.Application.Features.Commands.CreateProduct
{
    public record CreateProductCommand(Guid OwnerId, CreateProductDto Product) : IRequest<Guid>;
}
