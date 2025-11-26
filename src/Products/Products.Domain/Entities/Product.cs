using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Products.Domain.Entities
{
    public class Product : Entity<Guid>
    {
        public string Name { get; private set; } = null!;
        public string Description { get; private set; } = null!;
        public decimal Price { get; private set; }
        public bool IsAvailable { get; private set; } = true;
        public Guid OwnerId { get; private set; } 
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public bool IsDeleted { get; private set; } = false;

        protected Product() { }

        public Product(string name, string description, decimal price, Guid ownerId)
        {
            Id = Guid.NewGuid();
            Update(name, description, price);
            OwnerId = ownerId;
            CreatedAt = DateTime.UtcNow;
            IsAvailable = true;
            IsDeleted = false;
        }
        public Product(Guid id, string name, string description, decimal price, Guid ownerId)
        {
            Id = id;
            Update(name, description, price);
            OwnerId = ownerId;
            CreatedAt = DateTime.UtcNow;
            IsAvailable = true;
            IsDeleted = false;
        }

        public void Update(string name, string description, decimal price)
        {
            Name = name;
            Description = description ?? string.Empty;
            Price = price;
        }

        public void SetAvailability(bool available) => IsAvailable = available;

        public void SoftDelete() => IsDeleted = true;
        public void Restore() => IsDeleted = false;
    }
}

