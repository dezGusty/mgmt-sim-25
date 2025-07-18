using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagementSimulator.Database.Repositories.Intefaces;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Context;
using Microsoft.EntityFrameworkCore;


namespace ManagementSimulator.Database.Repositories
{
    internal class LeaveRequestTypeRepository : BaseRepository<LeaveRequestType>, ILeaveRequestTypeRepository
    {
        private readonly MGMTSimulatorDbContext _dbContext;
        public LeaveRequestTypeRepository(MGMTSimulatorDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<LeaveRequestType?> GetLeaveRequestTypesByDescriptionAsync(string description)
        {
            return await _dbContext.LeaveRequestTypes.FirstOrDefaultAsync(lrt => lrt.Description == description);
        }
    }
}
