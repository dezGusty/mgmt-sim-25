using ManagementSimulator.Core.Dtos.Requests.Departments;
using ManagementSimulator.Core.Dtos.Responses;
using ManagementSimulator.Core.Dtos.Responses.PagedResponse;
using ManagementSimulator.Core.Enums;
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
            if (string.IsNullOrEmpty(request.Name))
            {
                throw new ArgumentException("Department name is required.", nameof(request));
            }

            if (await _repository.GetDepartmentByNameAsync(request.Name, includeDeleted: true, tracking: false) != null)
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

            PatchHelper.PatchRequestToEntity.PatchFrom<UpdateDepartmentRequestDto, Department>(existing, request);

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
            if (await _repository.GetFirstOrDefaultAsync(id) == null)
            {
                throw new EntryNotFoundException(nameof(Department), id);
            }

            return await _repository.DeleteAsync(id);
        }

        public async Task<PagedResponseDto<DepartmentResponseDto>> GetAllDepartmentsFilteredAsync(QueriedDepartmentRequestDto payload)
        {
            if (payload.ActivityStatus != null && payload.ActivityStatus == DepartmentActivityStatus.INACTIVE)
            {
                var (deletedResult, deletedTotalCount) = await _repository.GetAllInactiveDepartmentsFilteredAsync(
                    payload.Name,
                    payload.PagedQueryParams.ToQueryParams()
                );

                if (deletedResult == null || !deletedResult.Any())
                    return new PagedResponseDto<DepartmentResponseDto>
                    {
                        Data = new List<DepartmentResponseDto>(),
                        Page = payload.PagedQueryParams.Page ?? 1,
                        PageSize = payload.PagedQueryParams.PageSize ?? 1,
                        TotalPages = 0
                    };

                return new PagedResponseDto<DepartmentResponseDto>
                {
                    Data = deletedResult.Select(d => new DepartmentResponseDto
                    {
                        Id = d.Id,
                        Name = d.Name,
                        Description = d.Description,
                        EmployeeCount = d.EmployeeCount,
                        DeletedAt = d.DeletedAt,
                    }).ToList(),
                    Page = payload.PagedQueryParams.Page ?? 1,
                    PageSize = payload.PagedQueryParams.PageSize ?? 1,
                    TotalPages = payload.PagedQueryParams.PageSize != null ?
                        (int)Math.Ceiling((double)deletedTotalCount / (int)payload.PagedQueryParams.PageSize) : 1
                };
            }

            bool includeDeleted = payload.ActivityStatus == null || payload.ActivityStatus == DepartmentActivityStatus.ALL;

            var (result, totalCount) = await _repository.GetAllDepartmentsFilteredAsync(
                payload.Name,
                payload.PagedQueryParams.ToQueryParams(),
                includeDeleted: includeDeleted
            );

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
                    EmployeeCount = d.EmployeeCount,
                    DeletedAt = d.DeletedAt,
                }).ToList(),
                Page = payload.PagedQueryParams.Page ?? 1,
                PageSize = payload.PagedQueryParams.PageSize ?? 1,
                TotalPages = payload.PagedQueryParams.PageSize != null ?
                    (int)Math.Ceiling((double)totalCount / (int)payload.PagedQueryParams.PageSize) : 1
            };
        }

        public async Task<bool> RestoreDepartmentAsync(int id)
        {
            Department? departmentToRestore = await _repository.GetDepartmentByIdAsync(id, includeDeleted: true, tracking: true);
            if (departmentToRestore == null)
            {
                throw new EntryNotFoundException(nameof(Department), nameof(Department.Id));
            }

            departmentToRestore.DeletedAt = null;
            await _repository.SaveChangesAsync();
            return true;
        }
    }
}
