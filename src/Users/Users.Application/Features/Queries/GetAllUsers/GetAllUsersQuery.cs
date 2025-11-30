using MediatR;
using Users.Application.DTOs;

namespace Users.Application.Features.Queries.GetAllUsers
{
    public class GetAllUsersQuery : IRequest<IEnumerable<UserDto>> { }
}
