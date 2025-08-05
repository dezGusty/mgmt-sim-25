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
        Task<(List<User>? Data, int TotalCount)> GetAllUnassignedUsersFilteredAsync(QueryParams parameters, string? globalSearch = null, string? unassignedName = null, string? jobTitle = null, bool includeDeleted = false, bool tracking = false);
        Task<List<User>> GetAllAdminsAsync(string? name, string? email, bool includeDeleted = false, bool tracking = false);
        Task<List<User>> GetAllUsersIncludeRelationshipsAsync(bool includeDeleted = false, bool tracking = false);
        Task<User?> GetUserByEmail(string email, bool includeDeleted = false, bool tracking = false);
        Task<(List<User>? Data, int TotalCount)> GetAllUsersWithReferencesFilteredAsync(string? name, string? email, string? department, string? jobTitle, string? globalSearch, QueryParams parameters, bool includeDeleted = false, bool tracking = false);
        Task<List<User>> GetAllUsersWithReferencesAsync(bool includeDeleted = false, bool tracking = false);
        Task<(List<User>? Data, int TotalCount)> GetAllInactiveUsersWithReferencesFilteredAsync(string? name, string? email, string? department, string? jobTitle, string? globalSearch, QueryParams parameters, bool tracking = false); 
        Task<User?> GetUserWithReferencesByIdAsync(int id, bool includeDeleted = false, bool tracking = false);
        Task<List<User>> GetUsersByManagerIdAsync(int managerId, bool includeDeleted = false);
        Task<bool> RestoreUserByIdAsync(int id);
        Task<User?> GetUserByIdAsync(int id, bool includeDeleted = false, bool tracking = false);
        Task<List<User>> GetSubordinatesByUserIdsAsync(List<int> ids, bool includeDeleted = false, bool tracking = false);
        Task<List<User>> GetManagersByUserIdsAsync(List<int> ids, bool includeDeleted = false, bool tracking = false);
        Task<(List<User> Data, int TotalCount)> GetAllManagersFilteredAsync(string? globalSearch, string? managerName, string? employeeName, string? managerEmail, string? employeeEmail, string? jobTitle, string? department, QueryParams parameters, bool includeDeleted = false, bool tracking = false);
    }
}
