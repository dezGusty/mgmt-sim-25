using ManagementSimulator.Database.Context;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories.Intefaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Database.Repositories
{
    public class BaseRepository<T>(MGMTSimulatorDbContext databaseContext) : IBaseRepostory<T> where T : BaseEntity
    {
        private DbSet<T> DbSet { get; } = databaseContext.Set<T>();

        public async Task<List<T>> GetAllAsync(bool includeDeletedEntities = false)
        {
            return await GetRecords(includeDeletedEntities).ToListAsync();
        }

        public Task<T?> GetFirstOrDefaultAsync(int primaryKey, bool includeDeletedEntities = false)
        {
            var records = GetRecords(includeDeletedEntities);
            return records.FirstOrDefaultAsync(record => record.Id == primaryKey);
        }

        public void Insert(params T[] records)
        {
            DbSet.AddRange(records);
        }

        public void Update(params T[] records)
        {
            foreach (var baseEntity in records)
            {
                baseEntity.ModifiedAt = DateTime.UtcNow;
            }
            DbSet.UpdateRange(records);
        }

        public void SoftDelete(params T[] records)
        {
            foreach (var baseEntity in records)
            {
                baseEntity.DeletedAt = DateTime.UtcNow;
            }
            Update(records);
        }

        public async Task SaveChangesAsync()
        {
            await databaseContext.SaveChangesAsync();
        }

        protected IQueryable<T> GetRecords(bool includeDeletedEntities = false)
        {
            var result = DbSet.AsQueryable();
            if (includeDeletedEntities is false)
            {
                result = result.Where(r => r.DeletedAt == null);
            }
            return result;
        }

        public async Task<T> AddAsync(T entity)
        {
            Insert(entity);
            await SaveChangesAsync();
            return entity;
        }

        public async Task<T?> UpdateAsync(T entity)
        {
            Update(entity);
            await SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int primaryKey)
        {
            var entity = await GetFirstOrDefaultAsync(primaryKey);
            if (entity is null)
            {
                return false;
            }
            SoftDelete(entity);
            return true;
        }
    }
}
