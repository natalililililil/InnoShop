using MediatR;
using Users.Domain.Entities;
using Users.Domain.Enums;
using Users.Domain.Interfaces;

namespace Users.Application.Features.Commands.CreateUser
{
    public class CreateUserHandler : IRequestHandler<CreateUserCommand>
    {
        private readonly IUserRepository _userRepository;

        public CreateUserHandler(IUserRepository userRepository) => _userRepository = userRepository;

        public async Task Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var dto = request.User;
            var role = Enum.TryParse<Role>(dto.Role, true, out var parsedRole) ? parsedRole : Role.User;

            var user = new User(dto.Name, dto.Email, dto.Password, role);
            
            await _userRepository.SaveAsync(cancellationToken);
        }
    }
}
