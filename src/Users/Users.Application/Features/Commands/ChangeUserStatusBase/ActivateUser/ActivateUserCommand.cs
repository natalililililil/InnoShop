using MediatR;

namespace Users.Application.Features.Commands.ChangeUserStatusBase.ActivateUser
{
    public record ActivateUserCommand(Guid Id) : IRequest<bool>;
}
