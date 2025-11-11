using Users.Application.DTOs;
using Users.Domain.Enums;

namespace Users.Application.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<UserDto?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Guid> CreateAsync(string name, string email, string password, Role role, CancellationToken cancellationToken = default);
        Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
        Task ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
