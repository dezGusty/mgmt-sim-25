using ManagementSimulator.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Database.Repositories.Intefaces
{
    public interface IUserRepository : IBaseRepostory<User>
    {
        public Task<User?> GetUserByEmail(string email);
        public Task<List<User>> GetAllUsersWithReferencesAsync();
        public Task<User?> GetUserWithReferencesByIdAsync(int id);
        Task<List<User>> GetUsersByManagerIdAsync(int managerId);
    }
}
