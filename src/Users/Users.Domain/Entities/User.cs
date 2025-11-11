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

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public User() { }
        public User(string name, string email, string passwordHash, Role role)
        {
            Name = name;
            Email = email;
            PasswordHash = passwordHash;
            Role = role;
        }

        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;
        // public ICollection<Product> Products { get; set; }
    }
}
