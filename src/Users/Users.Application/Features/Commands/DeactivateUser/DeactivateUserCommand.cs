using MediatR;

namespace Users.Application.Features.Commands.DeactivateUser
{
    public record DeactivateUserCommand(Guid Id) : IRequest<bool>;
}
