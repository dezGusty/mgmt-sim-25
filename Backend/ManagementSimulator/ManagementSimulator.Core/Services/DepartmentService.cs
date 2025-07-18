using ManagementSimulator.Core.Dtos.Requests.Departments;
using ManagementSimulator.Core.Dtos.Responses;
using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories.Intefaces;
using ManagementSimulator.Infrastructure.Exceptions;


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
            {
                throw new EntryNotFoundException(nameof(Department), id);
            }

            return new DepartmentResponseDto
            {
                Id = department.Id,
                Name = department.Name,
                Description = department.Description
            };
        }

        public async Task<DepartmentResponseDto> AddDepartmentAsync(CreateDepartmentRequestDto request)
        {
            if (await _repository.GetDepartmentByNameAsync(request.Name) != null)
            {
                throw new UniqueConstraintViolationException(nameof(Department), nameof(Department.Name));
            }

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

        public async Task<DepartmentResponseDto?> UpdateDepartmentAsync(int id, UpdateDepartmentRequestDto request)
        {
            var existing = await _repository.GetFirstOrDefaultAsync(id);
            if (existing == null)
            { 
                throw new EntryNotFoundException(nameof(Department), request.Id); 
            }

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
            if(_repository.GetFirstOrDefaultAsync(id) == null)
            {
                throw new EntryNotFoundException(nameof(Department), id);
            }

            return await _repository.DeleteAsync(id);
        }
    }
}
