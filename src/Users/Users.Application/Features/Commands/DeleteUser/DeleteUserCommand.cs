using MediatR;

namespace Users.Application.Features.Commands.DeleteUser
{
    public record DeleteUserCommand(Guid Id) : IRequest<bool>;
}
