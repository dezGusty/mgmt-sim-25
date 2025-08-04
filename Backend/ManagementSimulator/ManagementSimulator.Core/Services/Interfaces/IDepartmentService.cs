using ManagementSimulator.Core.Dtos.Requests.Departments;
using ManagementSimulator.Core.Dtos.Responses;
using ManagementSimulator.Core.Dtos.Responses.PagedResponse;


namespace ManagementSimulator.Core.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<PagedResponseDto<DepartmentResponseDto>> GetAllDepartmentsFilteredAsync(QueriedDepartmentRequestDto payload);
        Task<List<DepartmentResponseDto>> GetAllDepartmentsAsync();
        Task<DepartmentResponseDto?> GetDepartmentByIdAsync(int id);
        Task<DepartmentResponseDto> AddDepartmentAsync(CreateDepartmentRequestDto request);
        Task<DepartmentResponseDto?> UpdateDepartmentAsync(int id, UpdateDepartmentRequestDto request);
        Task<bool> DeleteDepartmentAsync(int id);
        Task<bool> RestoreDepartmentAsync(int id);
    }
}
