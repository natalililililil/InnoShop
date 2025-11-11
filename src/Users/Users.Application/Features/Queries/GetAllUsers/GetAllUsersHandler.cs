using MediatR;
using System.Linq;
using Users.Application.DTOs;
using Users.Domain.Interfaces;

namespace Users.Application.Features.Queries.GetAllUsers
{
    public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, IEnumerable<UserDto>>
    {
        private readonly IUserRepository _userRepository;

        public GetAllUsersHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await _userRepository.GetAllAsync(cancellationToken);
            return users.Select(u => new UserDto(u.Id, u.Name, u.Email, u.Role.ToString(), u.IsActive));
        }
    }
}
