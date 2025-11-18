using MediatR;
using Microsoft.AspNetCore.Identity;
using Users.Domain.Entities;
using Users.Domain.Enums;
using Users.Domain.Interfaces;

namespace Users.Application.Features.Commands.CreateUser
{
    public class CreateUserHandler : IRequestHandler<CreateUserCommand, Guid>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<object> _passwordHasher;

        public CreateUserHandler(IUserRepository userRepository, IPasswordHasher<object> passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var dto = request.User;
            var role = Enum.TryParse<Role>(dto.Role, true, out var parsedRole) ? parsedRole : Role.User;

            var passwordHash = _passwordHasher.HashPassword(null!, dto.Password);
            var user = new User(dto.Name, dto.Email, passwordHash, role);

            _userRepository.Create(user);
            await _userRepository.SaveAsync(cancellationToken);

            return user.Id;
        }
    }
}
