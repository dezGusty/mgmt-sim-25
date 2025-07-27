using ManagementSimulator.Core.Dtos.Requests.Departments;
using ManagementSimulator.Core.Dtos.Responses;
using ManagementSimulator.Core.Dtos.Responses.PagedResponse;
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
                throw new EntryNotFoundException(nameof(Department), id); 
            }

            if (await _repository.GetDepartmentByNameAsync(request.Name) != null)
            {
                throw new UniqueConstraintViolationException(nameof(Department), nameof(Department.Name));
            }

            PatchHelper.PatchRequestToEntity.PatchFrom<UpdateDepartmentRequestDto, Department>(existing, request);
            existing.ModifiedAt = DateTime.UtcNow;

            await _repository.SaveChangesAsync();

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
            if(await _repository.GetFirstOrDefaultAsync(id) == null)
            {
                throw new EntryNotFoundException(nameof(Department), id);
            }

            return await _repository.DeleteAsync(id);
        }

        public async Task<PagedResponseDto<DepartmentResponseDto>> GetAllDepartmentsFilteredAsync(QueriedDepartmentRequestDto payload)
        {
            var (result, totalCount) = await _repository.GetAllDepartmentsFilteredAsync(payload.Name, payload.PagedQueryParams.ToQueryParams());

            if (result == null || !result.Any())
                return new PagedResponseDto<DepartmentResponseDto>
                {
                    Data = new List<DepartmentResponseDto>(),
                    Page = payload.PagedQueryParams.Page ?? 1,
                    PageSize = payload.PagedQueryParams.PageSize ?? 1,
                    TotalPages = 0
                };

            return new PagedResponseDto<DepartmentResponseDto>
            {
                Data = result.Select(d => new DepartmentResponseDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                }),
                Page = payload.PagedQueryParams.Page ?? 1,
                PageSize = payload.PagedQueryParams.PageSize ?? 1,
                TotalPages = payload.PagedQueryParams.PageSize != null ?
                    (int)Math.Ceiling((double)totalCount / (int)payload.PagedQueryParams.PageSize) : 1 
            };
        }
    }
}
