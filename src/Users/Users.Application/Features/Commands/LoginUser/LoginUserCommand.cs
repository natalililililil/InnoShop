using MediatR;
using Users.Application.DTOs;

namespace Users.Application.Features.Commands.LoginUser
{
    public record LoginUserCommand(LoginDto Login) : IRequest<AuthResultDto>;
}