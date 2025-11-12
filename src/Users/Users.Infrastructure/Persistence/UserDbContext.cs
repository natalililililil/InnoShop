using Microsoft.EntityFrameworkCore;
using Users.Domain.Entities;


namespace Users.Infrastructure.Persistence
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new Configurations.UserConfiguration());
            base.OnModelCreating(modelBuilder);
        }
    }
}
