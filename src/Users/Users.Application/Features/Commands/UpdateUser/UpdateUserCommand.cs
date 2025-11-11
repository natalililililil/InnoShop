using MediatR;

namespace Users.Application.Features.Commands.UpdateUser
{
    public class UpdateUserCommand : IRequest
    {
        public Guid Id { get; set; } = default!;
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
    }
}
