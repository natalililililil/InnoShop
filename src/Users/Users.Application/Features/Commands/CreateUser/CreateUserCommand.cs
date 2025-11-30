using MediatR;
using Users.Application.DTOs;
using Users.Domain.Entities;

namespace Users.Application.Features.Commands.CreateUser
{
    public record CreateUserCommand(CreateUserDto User) : IRequest<User>;
}
