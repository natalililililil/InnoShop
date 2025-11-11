using MediatR;
using Users.Application.DTOs;

namespace Users.Application.Features.Commands.CreateUser
{
    public class CreateUserCommand : IRequest
    {
        public CreateUserDto User { get; set; } = null!;
    }
}
