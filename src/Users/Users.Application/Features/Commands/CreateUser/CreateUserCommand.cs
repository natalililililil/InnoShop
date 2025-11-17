using MediatR;
using Users.Application.DTOs;

namespace Users.Application.Features.Commands.CreateUser
{
    public record CreateUserCommand(CreateUserDto User) : IRequest<Guid>;
}
