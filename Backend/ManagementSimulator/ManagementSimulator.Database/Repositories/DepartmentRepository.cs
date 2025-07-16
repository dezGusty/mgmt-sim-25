using ManagementSimulator.Database.Context;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories.Intefaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ManagementSimulator.Database.Repositories
{
    public class DepartmentRepository: BaseRepository<Department>, IDeparmentRepository
    {
        private readonly MGMTSimulatorDbContext _databaseContext;
        public DepartmentRepository(MGMTSimulatorDbContext databaseContext): base(databaseContext)
        {
            _databaseContext = databaseContext;
        }
    }
}
