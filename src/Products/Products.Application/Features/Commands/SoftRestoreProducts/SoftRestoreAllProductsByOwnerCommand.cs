using MediatR;

namespace Products.Application.Features.Commands.SoftRestoreProducts
{
    public record SoftRestoreAllProductsByOwnerCommand(Guid OwnerId) : IRequest<bool>;
}
