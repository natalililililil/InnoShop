using MediatR;
using Users.Application.DTOs;
using Users.Application.Exceptions;
using Users.Domain.Interfaces;

namespace Users.Application.Features.Queries.GetUserById
{
    public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
    {
        private readonly IUserRepository _userRepository;

        public GetUserByIdHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new NotFoundException($"User with ID {request.Id} not found");

            return new UserDto(user.Id, user.Name, user.Email, user.Role.ToString(), user.IsActive);
        }
    }
}
