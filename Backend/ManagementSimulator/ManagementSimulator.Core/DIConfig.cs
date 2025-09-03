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
            services.AddScoped<IJobTitleService, JobTitleService>();
            services.AddScoped<ILeaveRequestService, LeaveRequestService>();
            services.AddScoped<ILeaveRequestTypeService, LeaveRequestTypeService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IEmployeeManagerService, EmployeeManagerService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IEmployeeRoleService, EmployeeRoleService>();
            services.AddScoped<IResourceAuthorizationService, ResourceAuthorizationService>();
            services.AddScoped<ISecondManagerService, SecondManagerService>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IPublicHolidayService, PublicHolidayService>();

            return services;
        }

    }
}
