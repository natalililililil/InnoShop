using MediatR;

namespace Users.Application.Features.Commands.ChangeUserStatusBase.DeactivateUser
{
    public record DeactivateUserCommand(Guid Id) : IRequest<bool>;
}
