using System.ComponentModel.DataAnnotations;
using Users.Domain.Enums;

namespace Users.Domain.Entities
{
    public class User : Entity<Guid>
    {
        public string Name { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public Role Role { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

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
