using ManagementSimulator.Database.Dtos.QueryParams;
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
        Task<(List<User>? Data, int TotalCount)> GetAllUnassignedUsersFilteredAsync(QueryParams parameters, bool includeDeleted = false);
        Task<List<User>> GetAllAdminsAsync(string? lastName, string? email, bool includeDeleted = false);
        Task<List<User>> GetAllUsersIncludeRelationshipsAsync(bool includeDeleted = false);
        Task<User?> GetUserByEmail(string email, bool includeDeleted = false);
        Task<(List<User>? Data, int TotalCount)> GetAllUsersWithReferencesFilteredAsync(string? lastName,string? email,string? department,string? jobTitle,string? globalSearch, QueryParams parameters, bool includeDeleted = false);
        Task<List<User>> GetAllUsersWithReferencesAsync(bool includeDeleted = false);
        Task<User?> GetUserWithReferencesByIdAsync(int id, bool includeDeleted = false);
        Task<List<User>> GetUsersByManagerIdAsync(int managerId, bool includeDeleted = false);
        Task<bool> RestoreUserByIdAsync(int id);
        Task<User?> GetUserByIdAsync(int id, bool includeDeleted = false);
        Task<List<User>?> GetSubordinatesByUserIdsAsync(List<int> ids, bool includeDeleted = false);
        Task<List<User>?> GetManagersByUserIdsAsync(List<int> ids, bool includeDeleted = false);
        Task<(List<User>? Data, int TotalCount)> GetAllManagersFilteredAsync(string? lastName, string? email, QueryParams parameters, bool includeDeleted = false);
    }
}
