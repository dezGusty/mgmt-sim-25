using ManagementSimulator.Core.Dtos.Requests.Departments;
using ManagementSimulator.Core.Dtos.Responses;


namespace ManagementSimulator.Core.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<List<DepartmentResponseDto>> GetAllDepartmentsAsync();
        Task<DepartmentResponseDto?> GetDepartmentByIdAsync(int id);
        Task<DepartmentResponseDto> AddDepartmentAsync(CreateDepartmentRequestDto request);
        Task<DepartmentResponseDto?> UpdateDepartmentAsync(int id, UpdateDepartmentRequestDto request);
        Task<bool> DeleteDepartmentAsync(int id);
    }
}
