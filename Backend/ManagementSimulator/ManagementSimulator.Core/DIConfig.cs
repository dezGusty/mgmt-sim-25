using ManagementSimulator.Database.Context;
using ManagementSimulator.Database.Repositories.Intefaces;
using ManagementSimulator.Database.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Core.Services;

namespace ManagementSimulator.Core
{
    public static class DIConfig
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IDepartmentService, DepartmentService>();
            //services.AddScoped<IJobTitleRepository, JobTitleRepository>();
            //services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
            //services.AddScoped<ILeaveRequestTypeRepository, LeaveRequestTypeRepository>();
            //services.AddScoped<IUserRepository, UserRepository>();

            return services;
        }

    }
}
