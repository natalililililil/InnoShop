using Users.Application.DTOs;
using Users.Application.Exceptions;
using Users.Application.Interfaces;
using Users.Domain.Entities;
using Users.Domain.Enums;
using Users.Domain.Interfaces;

namespace Users.Application.Services
{
    public class UserService<TKey> : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var users = await _userRepository.GetAllAsync(cancellationToken);
            return users.Select(u => new UserDto(u.Id, u.Name, u.Email, u.Role.ToString(), u.IsActive));
        }

        public async Task<UserDto?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(id, cancellationToken);
            return user == null ? null : new UserDto(user.Id, user.Name, user.Email, user.Role.ToString(), user.IsActive);
        }

        public async Task<Guid> CreateAsync(string name, string email, string password, Role role, CancellationToken cancellationToken = default)
        {
            var user = new User(name, email, password, role);
            _userRepository.Create(user);
            await _userRepository.SaveAsync(cancellationToken);
            return user.Id;
        }

        public async Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException($"User with ID {id} not found");
            user.Deactivate();
            _userRepository.Update(user);
            await _userRepository.SaveAsync(cancellationToken);
        }

        public async Task ActivateAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException($"User with ID {id} not found");
            user.Activate();
            _userRepository.Update(user);
            await _userRepository.SaveAsync(cancellationToken);
        }
    }
}
