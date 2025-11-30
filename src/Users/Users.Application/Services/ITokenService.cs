using Users.Domain.Entities;

namespace Users.Application.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user, DateTime expiryTime);
    }
}