using MediatR;
using Users.Application.DTOs;

namespace Users.Application.Features.Queries.GetUserById
{
    public record GetUserByIdQuery(Guid Id) : IRequest<UserDto?>;
}
