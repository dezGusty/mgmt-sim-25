using ManagementSimulator.Core.Dtos.Requests.PublicHolidays;
using ManagementSimulator.Core.Dtos.Responses.PagedResponse;
using ManagementSimulator.Core.Dtos.Responses.PublicHolidays;
using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories.Intefaces;
using ManagementSimulator.Infrastructure.Exceptions;

namespace ManagementSimulator.Core.Services
{
    public class PublicHolidayService : IPublicHolidayService
    {
        private readonly IPublicHolidayRepository _repository;

        public PublicHolidayService(IPublicHolidayRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<PublicHolidayResponseDto>> GetHolidaysByYearAsync(int year)
        {
            var holidays = await _repository.GetHolidaysByYearAsync(year);

            return holidays.Select(h => new PublicHolidayResponseDto
            {
                Id = h.Id,
                Name = h.Name,
                Date = h.Date,
                IsRecurring = h.IsRecurring,
                CreatedAt = h.CreatedAt,
                ModifiedAt = h.ModifiedAt,
                DeletedAt = h.DeletedAt
            }).ToList();
        }

        public async Task<PublicHolidayResponseDto?> GetHolidayByIdAsync(int id)
        {
            var holiday = await _repository.GetFirstOrDefaultAsync(id);
            if (holiday == null)
            {
                throw new EntryNotFoundException(nameof(PublicHoliday), id);
            }

            return new PublicHolidayResponseDto
            {
                Id = holiday.Id,
                Name = holiday.Name,
                Date = holiday.Date,
                IsRecurring = holiday.IsRecurring,
                CreatedAt = holiday.CreatedAt,
                ModifiedAt = holiday.ModifiedAt,
                DeletedAt = holiday.DeletedAt
            };
        }

        public async Task<PublicHolidayResponseDto> CreateHolidayAsync(CreatePublicHolidayRequestDto request)
        {
            // Check for duplicates (only active records since we use hard delete)
            var existingHoliday = await _repository.GetHolidayByNameAndDateAsync(request.Name, request.Date, includeDeleted: false);
            if (existingHoliday != null)
            {
                throw new UniqueConstraintViolationException(nameof(PublicHoliday), $"{nameof(PublicHoliday.Name)} and {nameof(PublicHoliday.Date)}");
            }

            var newHoliday = new PublicHoliday
            {
                Name = request.Name.Trim(),
                Date = request.Date.Date, // Ensure we only store the date part
                IsRecurring = request.IsRecurring
            };

            await _repository.AddAsync(newHoliday);

            return new PublicHolidayResponseDto
            {
                Id = newHoliday.Id,
                Name = newHoliday.Name,
                Date = newHoliday.Date,
                IsRecurring = newHoliday.IsRecurring,
                CreatedAt = newHoliday.CreatedAt,
                ModifiedAt = newHoliday.ModifiedAt,
                DeletedAt = newHoliday.DeletedAt
            };
        }

        public async Task<PublicHolidayResponseDto?> UpdateHolidayAsync(int id, UpdatePublicHolidayRequestDto request)
        {
            var existing = await _repository.GetFirstOrDefaultAsync(id);
            if (existing == null)
            {
                throw new EntryNotFoundException(nameof(PublicHoliday), id);
            }

            // Check for duplicates (excluding current record, only active records since we use hard delete)
            var duplicateCheck = await _repository.GetHolidayByNameAndDateAsync(request.Name, request.Date, includeDeleted: false);
            if (duplicateCheck != null && duplicateCheck.Id != id)
            {
                throw new UniqueConstraintViolationException(nameof(PublicHoliday), $"{nameof(PublicHoliday.Name)} and {nameof(PublicHoliday.Date)}");
            }

            existing.Name = request.Name.Trim();
            existing.Date = request.Date.Date; // Ensure we only store the date part
            existing.IsRecurring = request.IsRecurring;

            await _repository.UpdateAsync(existing);

            return new PublicHolidayResponseDto
            {
                Id = existing.Id,
                Name = existing.Name,
                Date = existing.Date,
                IsRecurring = existing.IsRecurring,
                CreatedAt = existing.CreatedAt,
                ModifiedAt = existing.ModifiedAt,
                DeletedAt = existing.DeletedAt
            };
        }

        public async Task<bool> DeleteHolidayAsync(int id)
        {
            var existing = await _repository.GetFirstOrDefaultAsync(id);
            if (existing == null)
            {
                throw new EntryNotFoundException(nameof(PublicHoliday), id);
            }

            return await _repository.HardDeleteAsync(id);
        }

        public async Task<bool> HardDeleteHolidayAsync(int id)
        {
            var existing = await _repository.GetFirstOrDefaultAsync(id, true); // Include deleted entities
            if (existing == null)
            {
                throw new EntryNotFoundException(nameof(PublicHoliday), id);
            }

            return await _repository.HardDeleteAsync(id);
        }

        public async Task<PagedResponseDto<PublicHolidayResponseDto>> GetAllHolidaysFilteredAsync(QueriedPublicHolidayRequestDto request)
        {
            int year = request.Year ?? DateTime.Now.Year;
            var holidays = await _repository.GetHolidaysByYearAsync(year);

            if (!holidays.Any())
            {
                return new PagedResponseDto<PublicHolidayResponseDto>
                {
                    Data = new List<PublicHolidayResponseDto>(),
                    Page = request.PagedQueryParams.Page ?? 1,
                    PageSize = request.PagedQueryParams.PageSize ?? 10,
                    TotalPages = 0
                };
            }

            // Apply paging
            int page = request.PagedQueryParams.Page ?? 1;
            int pageSize = request.PagedQueryParams.PageSize ?? 10;
            int skip = (page - 1) * pageSize;

            var pagedHolidays = holidays
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            return new PagedResponseDto<PublicHolidayResponseDto>
            {
                Data = pagedHolidays.Select(h => new PublicHolidayResponseDto
                {
                    Id = h.Id,
                    Name = h.Name,
                    Date = h.Date,
                    IsRecurring = h.IsRecurring,
                    CreatedAt = h.CreatedAt,
                    ModifiedAt = h.ModifiedAt,
                    DeletedAt = h.DeletedAt
                }).ToList(),
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)holidays.Count / pageSize)
            };
        }

        public async Task<List<PublicHolidayResponseDto>> GetHolidaysInRangeAsync(DateTime startDate, DateTime endDate)
        {
            var holidays = await _repository.GetHolidaysInRangeAsync(startDate, endDate);

            return holidays.Select(h => new PublicHolidayResponseDto
            {
                Id = h.Id,
                Name = h.Name,
                Date = h.Date,
                IsRecurring = h.IsRecurring,
                CreatedAt = h.CreatedAt,
                ModifiedAt = h.ModifiedAt,
                DeletedAt = h.DeletedAt
            }).ToList();
        }
    }
}