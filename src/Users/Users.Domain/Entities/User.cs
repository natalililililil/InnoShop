using System.ComponentModel.DataAnnotations;
using Users.Domain.Enums;

namespace Users.Domain.Entities
{
    public class User : Entity<Guid>
    {
        public string Name { get; private set; } = null!;

        public string Email { get; private set; } = null!;

        public string PasswordHash { get; private set; } = null!;

        public Role Role { get; private set; }

        public bool IsActive { get; private set; } = true;
        public bool EmailConfirmed { get; private set; } = false;
        public string? EmailConfirmationToken { get; private set; }
        public DateTime? EmailConfirmationTokenExpiry { get; private set; }
        public string? PasswordResetToken { get; private set; } 
        public DateTime? PasswordResetTokenExpiry { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public User() { }
        public User(string name, string email, string passwordHash, Role role)
        {
            Name = name;
            Email = email;
            PasswordHash = passwordHash;
            Role = role;
        }
        public User(Guid id, string name, string email, string passwordHash, Role role) : base(id)
        {
            Name = name;
            Email = email;
            PasswordHash = passwordHash;
            Role = role;
        }

        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;
        public void UpdateName(string name) => Name = name;
        public void UpdateEmail(string email) => Email = email;
        public void UpdateRole(Role role) => Role = role;
        public void SetEmailConfirmationToken(string token, DateTime expiry)
        {
            EmailConfirmationToken = token;
            EmailConfirmationTokenExpiry = expiry;
        }
        public void ClearEmailConfirmationToken()
        {
            EmailConfirmationToken = null;
            EmailConfirmationTokenExpiry = null;
        }
        public void ConfirmEmail() => EmailConfirmed = true;
        public void SetPasswordResetToken(string token, DateTime expiry) 
        {
            PasswordResetToken = token;
            PasswordResetTokenExpiry = expiry;
        }
        public void ClearPasswordResetToken()
        {
            PasswordResetToken = null;
            PasswordResetTokenExpiry = null;
        }
        public void UpdatePasswordHash(string newHash) => PasswordHash = newHash;
    }
}
