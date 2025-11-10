using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Domain.Entities;

namespace Users.Domain.Interfaces
{
    public interface IUserRepository<TKey>
    {
        Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<User?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        void Create(User user);
        void Update(User user);
        void Delete(User user);
        Task SaveAsync(CancellationToken cancellationToken = default);
    }
}
