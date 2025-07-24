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
        Task<List<User>> GetAllUsersIncludeRelationships();
        public Task<User?> GetUserByEmail(string email);
        public Task<List<User>> GetAllUsersWithReferencesAsync();
        public Task<User?> GetUserWithReferencesByIdAsync(int id);
        Task<List<User>> GetUsersByManagerIdAsync(int managerId);
        Task<bool> RestoreUserByIdAsync(int id);
        Task<User?> GetUserByIdIncludeDeletedAsync(int id);
        public Task<User?> GetUserByIdAsync(int id);
        Task<List<User>?> GetSubordinatesByUserIdsAsync(List<int> ids);
        Task<List<User>?> GetManagersByUserIdsAsync(List<int> ids);
    }
}
