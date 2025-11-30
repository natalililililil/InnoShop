using MediatR;

namespace Users.Application.Features.Commands.ConfirmEmail
{
    public record ConfirmEmailCommand(string Email, string Token) : IRequest<bool>;
}