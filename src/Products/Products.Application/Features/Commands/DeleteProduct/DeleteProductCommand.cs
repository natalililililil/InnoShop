using MediatR;

namespace Products.Application.Features.Commands.DeleteProduct
{
    public record DeleteProductCommand(Guid Id, Guid OwnerId) : IRequest<bool>;
}
