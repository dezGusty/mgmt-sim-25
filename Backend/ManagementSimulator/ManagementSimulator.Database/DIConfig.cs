using ManagementSimulator.Database.Context;
using ManagementSimulator.Database.Repositories;
using ManagementSimulator.Database.Repositories.Intefaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Database
{
    public static class DIConfig
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddDbContext<MGMTSimulatorDbContext>();
            services.AddScoped<IDeparmentRepository, DepartmentRepository>();
            //services.AddScoped<IJobTitleRepository, JobTitleRepository>();
            //services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
            //services.AddScoped<ILeaveRequestTypeRepository, LeaveRequestTypeRepository>();
            //services.AddScoped<IUserRepository, UserRepository>();

            return services;
        }
    }
}
