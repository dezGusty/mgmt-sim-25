using ManagementSimulator.Core.Dtos.Requests.Departments;
using ManagementSimulator.Core.Dtos.Responses;
using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories.Intefaces;


namespace ManagementSimulator.Core.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDeparmentRepository _repository;

        public DepartmentService(IDeparmentRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<DepartmentResponseDto>> GetAllDepartmentsAsync()
        {
            var departments = await _repository.GetAllAsync();

            return departments.Select(d => new DepartmentResponseDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description
            }).ToList();
        }

        public async Task<DepartmentResponseDto?> GetDepartmentByIdAsync(int id)
        {
            var department = await _repository.GetFirstOrDefaultAsync(id);
            if (department == null)
                return null;

            return new DepartmentResponseDto
            {
                Id = department.Id,
                Name = department.Name,
                Description = department.Description
            };
        }

        public async Task<DepartmentResponseDto> AddDepartmentAsync(CreateDepartmentRequestDto request)
        {
            var newDepartment = new Department
            {
                Name = request.Name,
                Description = request.Description ?? string.Empty 
            };

            await _repository.AddAsync(newDepartment);

            return new DepartmentResponseDto
            {
                Id = newDepartment.Id,
                Name = newDepartment.Name,
                Description = newDepartment.Description
            };
        }

        public async Task<DepartmentResponseDto?> UpdateDepartmentAsync(UpdateDepartmentRequestDto request)
        {
            var existing = await _repository.GetFirstOrDefaultAsync(request.Id);
            if (existing == null)
                return null;

            existing.Name = request.Name;
            existing.Description = request.Description ?? string.Empty;
            existing.ModifiedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(existing);

            return new DepartmentResponseDto
            {
                Id = existing.Id,
                Name = existing.Name,
                Description = existing.Description,
                ModifiedAt = existing.ModifiedAt
            };
        }

        public async Task<bool> DeleteDepartmentAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }
    }
}
