using MediatR;
using Users.Application.DTOs;

namespace Users.Application.Features.Queries.GetUserById
{
    public class GetUserByIdQuery : IRequest<UserDto?>
    {
        public Guid Id { get; set; } = default!;
    }
}
