using MediatR;
using Users.Application.DTOs;

namespace Users.Application.Features.Commands.UpdateUser
{
    public record UpdateUserCommand(Guid Id, UpdateUserDto User) : IRequest<bool>;

}
