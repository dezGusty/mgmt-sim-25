using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core
{
    internal class DIConfig
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<IJobTitleService, JobTitleService>();
            services.AddScoped<ILeaveRequestService, LeaveRequestService>();
            services.AddScoped<ILeaveRequestTypeService, LeaveRequestTypeService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IEmployeeManagerService, EmployeeManagerService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }

    }
}
