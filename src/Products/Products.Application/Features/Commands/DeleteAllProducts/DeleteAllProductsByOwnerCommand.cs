using MediatR;

namespace Products.Application.Features.Commands.DeleteAllProducts
{
    public record DeleteAllProductsByOwnerCommand(Guid OwnerId) : IRequest<bool>;
}
