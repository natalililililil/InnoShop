using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace Users.Domain.Entities
{
    public abstract class Entity<TKey>
    {
        public TKey Id { get; protected set; } = default!;
        protected Entity() { }
        protected Entity(TKey id) 
        {
            Id = id;
        }
    }
}
