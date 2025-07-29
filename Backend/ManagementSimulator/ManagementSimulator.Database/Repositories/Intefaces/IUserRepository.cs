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
        Task<(List<User>? Data, int TotalCount)> GetAllUnassignedUsersFilteredAsync(QueryParams parameters);
        Task<List<User>> GetAllAdminsAsync(string? lastName, string? email);
        Task<List<User>> GetAllUsersIncludeRelationships();
        Task<User?> GetUserByEmail(string email);
        Task<(List<User>? Data, int TotalCount)> GetAllUsersWithReferencesFilteredAsync(string? lastName,string? email, QueryParams parameters);
        Task<List<User>> GetAllUsersWithReferencesAsync();
        Task<User?> GetUserWithReferencesByIdAsync(int id);
        Task<List<User>> GetUsersByManagerIdAsync(int managerId);
        Task<bool> RestoreUserByIdAsync(int id);
        Task<User?> GetUserByIdIncludeDeletedAsync(int id);
        Task<User?> GetUserByIdAsync(int id);
        Task<List<User>?> GetSubordinatesByUserIdsAsync(List<int> ids);
        Task<List<User>?> GetManagersByUserIdsAsync(List<int> ids);
        Task<(List<User>? Data, int TotalCount)> GetAllManagersFilteredAsync(string? lastName, string? email, QueryParams parameters, bool includeDeleted = false);
    }
}
