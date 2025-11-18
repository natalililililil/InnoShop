using MediatR;

namespace Users.Application.Features.Commands.ActivateUser
{
    public record ActivateUserCommand(Guid Id) : IRequest<bool>;
}
